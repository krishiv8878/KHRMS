using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using KHRMS.Services.Workflow.Interfaces;
using Serilog;

namespace KHRMS.Services
{
    public class ResignationService : IResignationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _sendEmailService;
        private readonly IUserContextService _userContextService;
        private readonly IChecklistService? _checklistService;
        private readonly IWorkflowEngineService? _workflowEngine;

        public ResignationService(
            IUnitOfWork unitOfWork,
            ISendEmailService sendEmailService,
            IUserContextService userContextService,
            IChecklistService? checklistService = null,
            IWorkflowEngineService? workflowEngine = null)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = sendEmailService;
            _userContextService = userContextService;
            _checklistService = checklistService;
            _workflowEngine = workflowEngine;
        }

        public async Task<bool> AddResignations(ResignationRequestModel resignation)
        {
            long empid = _userContextService.GetCurrentEmployeeId();
            if (resignation.EmployeeId > 0 && (_userContextService.IsAdmin() || _userContextService.IsHR()))
            {
                empid = resignation.EmployeeId;
            }

            var employee = await _unitOfWork.Employees.GetById(empid);
            if (employee == null)
                throw new Exception("Employee not found.");

            // Check if already active resignation exists
            var existingActiveResignation = (await _unitOfWork.Resignation.GetAll())
                .FirstOrDefault(r => r.EmployeeId == empid && !r.IsDeleted &&
                                     r.ResignationStatus != "Withdrawn" &&
                                     r.ResignationStatus != "Rejected" &&
                                     r.ResignationStatus != "Exited");
            if (existingActiveResignation != null)
            {
                throw new InvalidOperationException("You already have an active resignation request in progress.");
            }

            Employee? manager = null;
            if (employee.ManagerId.HasValue && employee.ManagerId.Value > 0)
            {
                manager = (await _unitOfWork.Employees.GetAll())
                              .FirstOrDefault(t => t.Id == employee.ManagerId.Value);
            }
            if (manager == null)
            {
                manager = (await _unitOfWork.Employees.GetAll())
                              .FirstOrDefault(t => t.Id != empid && !string.IsNullOrEmpty(t.EmailAddress)) ?? employee;
            }

            var roles = (await _unitOfWork.EmployeeRoleMappings.GetAll()).Where(t => t.EmployeeId == empid);
            var roleid = roles.Select(r => r.RoleId).ToList();

            // 1. Resolve Notice Period Policy Snapshot
            int resolvedNoticeDays = 30;
            string noticeSource = "CompanyDefault";

            // Resolution Order 1: Employee override
            if (employee.NoticePeriodOverrideDays.HasValue && employee.NoticePeriodOverrideDays.Value > 0)
            {
                resolvedNoticeDays = employee.NoticePeriodOverrideDays.Value;
                noticeSource = "EmployeeOverride";
            }
            else if (employee.DesignationId.HasValue && employee.DesignationId.Value > 0)
            {
                var desig = await _unitOfWork.Designations.GetById(employee.DesignationId.Value);
                if (desig != null)
                {
                    bool isUnderProbation = string.Equals(employee.ProbationStatus, "UnderProbation", StringComparison.OrdinalIgnoreCase) ||
                                            (employee.ProbationEndDate.HasValue && employee.ProbationEndDate.Value > DateTime.UtcNow);

                    // Resolution Order 2a: Designation probation policy
                    if (isUnderProbation && desig.ProbationNoticePeriodDays.HasValue && desig.ProbationNoticePeriodDays.Value > 0)
                    {
                        resolvedNoticeDays = desig.ProbationNoticePeriodDays.Value;
                        noticeSource = "DesignationProbationPolicy";
                    }
                    // Resolution Order 2b: Designation standard policy
                    else if (desig.NoticePeriodDays > 0)
                    {
                        resolvedNoticeDays = desig.NoticePeriodDays;
                        noticeSource = "DesignationPolicy";
                    }
                }
            }

            var resignDate = resignation.Resignation_Date ?? DateTime.Today;
            var proposedLwd = resignDate.AddDays(resolvedNoticeDays);
            var requestedLwd = resignation.RequestedLwd ?? proposedLwd;

            int shortfallDays = 0;
            if (requestedLwd < proposedLwd)
            {
                shortfallDays = (int)(proposedLwd - requestedLwd).TotalDays;
            }

            var newResign = new Resignation
            {
                EmployeeId = empid,
                Reason = resignation.Reason,
                Status = false,
                ManagerId = manager.Id,
                NoticePeriod = $"{resolvedNoticeDays} Days",
                NoticePeriodDaysSnapshot = resolvedNoticeDays,
                NoticePeriodSourceSnapshot = noticeSource,
                CreatedDate = DateTime.Now,
                Resignation_Date = resignDate,
                RoleId = roleid,
                ResignationStatus = "Submitted",
                RequestedLwd = requestedLwd,
                ProposedLwd = proposedLwd,
                ApprovedLwd = null,
                NoticeShortfallDays = shortfallDays,
                NoticeHandling = shortfallDays > 0 ? "ShortfallRecovery" : "Standard",
                FnFStatus = "Pending",
                IsLwdLocked = false,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.Resignation.Add(newResign);

            // Update Employee employment status to NoticePeriod
            employee.EmploymentStatus = "NoticePeriod";
            employee.UpdatedDate = DateTime.Now;
            _unitOfWork.Employees.Update(employee);

            var result = _unitOfWork.Save();

            // Format dates for email
            try
            {
                string formattedStartDate = GetFormattedDate(newResign.Resignation_Date);
                var managerEmail = manager.EmailAddress;
                var managerName = $"{manager.FirstName} {manager.LastName}".Trim();
                var employeeName = $"{employee.FirstName} {employee.LastName}".Trim();
                var temptype = "ResignationRequest";

                var dict = new Dictionary<string, string>
                {
                    { "ManagerName", managerName },
                    { "Resignation-Date", formattedStartDate },
                    { "NoticePeriod", newResign.NoticePeriod ?? "" },
                    { "EmployeeName", employeeName },
                    { "ManagerEmail", managerEmail ?? "" },
                    { "Reason", resignation.Reason ?? "" }
                };

                var subject = $"Resignation Request from {employeeName}";
                if (!string.IsNullOrEmpty(managerEmail))
                {
                    await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, temptype);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to send resignation email notification");
            }

            return result > 0;
        }

        public async Task<bool> WithdrawResignationAsync(long resignationId)
        {
            var res = await _unitOfWork.Resignation.GetById(resignationId);
            if (res == null || res.IsDeleted) throw new KeyNotFoundException("Resignation not found.");

            var currentEmpId = _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();

            if (!isAdminOrHr && res.EmployeeId != currentEmpId)
            {
                throw new UnauthorizedAccessException("You can only withdraw your own resignation.");
            }

            if (res.IsLwdLocked)
            {
                throw new InvalidOperationException("Resignation cannot be withdrawn after HR has approved and locked the Last Working Day.");
            }

            if (string.Equals(res.ResignationStatus, "Exited", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot withdraw a finalized exit.");
            }

            res.ResignationStatus = "Withdrawn";
            res.Status = false;
            res.IsActive = false;
            res.UpdatedDate = DateTime.Now;
            res.UpdatedBy = (int)currentEmpId;
            _unitOfWork.Resignation.Update(res);

            // Reset employee EmploymentStatus back to Active
            var employee = await _unitOfWork.Employees.GetById(res.EmployeeId);
            if (employee != null)
            {
                employee.EmploymentStatus = "Active";
                employee.UpdatedDate = DateTime.Now;
                _unitOfWork.Employees.Update(employee);
            }

            return _unitOfWork.Save() > 0;
        }

        public async Task<bool> ManagerReviewResignationAsync(long resignationId, ManagerResignationReviewDto dto)
        {
            var res = await _unitOfWork.Resignation.GetById(resignationId);
            if (res == null || res.IsDeleted) throw new KeyNotFoundException("Resignation not found.");

            var currentEmpId = _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            var employee = await _unitOfWork.Employees.GetById(res.EmployeeId);

            if (!isAdminOrHr && res.ManagerId != currentEmpId && employee?.ManagerId != currentEmpId)
            {
                throw new UnauthorizedAccessException("Only the designated reporting manager or HR can review this resignation.");
            }

            var now = DateTime.Now;

            if (dto.Accept)
            {
                if (dto.ProposedLwd.HasValue)
                {
                    if (dto.ProposedLwd.Value < (res.RequestedLwd ?? res.ProposedLwd ?? DateTime.Today) && string.IsNullOrWhiteSpace(dto.Remarks))
                    {
                        throw new InvalidOperationException("Remarks are required when proposing an earlier Last Working Day.");
                    }
                    res.ProposedLwd = dto.ProposedLwd.Value;
                }

                res.ResignationStatus = "PendingHR";
                res.ManagerRemarks = dto.Remarks;
                res.UpdatedBy = (int)currentEmpId;
                res.UpdatedDate = now;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Remarks))
                {
                    throw new InvalidOperationException("Comments are required when rejecting a resignation request.");
                }

                res.ResignationStatus = "Rejected";
                res.Status = false;
                res.ManagerRemarks = dto.Remarks;
                res.UpdatedBy = (int)currentEmpId;
                res.UpdatedDate = now;

                if (employee != null)
                {
                    employee.EmploymentStatus = "Active";
                    employee.UpdatedDate = now;
                    _unitOfWork.Employees.Update(employee);
                }
            }

            _unitOfWork.Resignation.Update(res);
            return _unitOfWork.Save() > 0;
        }

        public async Task<bool> HrApproveResignationAsync(long resignationId, HrResignationApprovalDto dto)
        {
            var res = await _unitOfWork.Resignation.GetById(resignationId);
            if (res == null || res.IsDeleted) throw new KeyNotFoundException("Resignation not found.");

            var currentEmpId = _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();

            if (!isAdminOrHr)
            {
                throw new UnauthorizedAccessException("Only HR or Admin can approve and lock the Last Working Day.");
            }

            var now = DateTime.Now;
            res.ApprovedLwd = dto.ApprovedLwd;
            res.IsLwdLocked = true;
            res.NoticeHandling = !string.IsNullOrWhiteSpace(dto.NoticeHandling) ? dto.NoticeHandling : "Standard";
            res.HrRemarks = dto.Remarks;
            res.ResignationStatus = "ExitClearance";
            res.Status = true;
            res.UpdatedBy = (int)currentEmpId;
            res.UpdatedDate = now;

            // Compute shortfall days if early exit or shortfall recovery
            if (string.Equals(res.NoticeHandling, "ShortfallRecovery", StringComparison.OrdinalIgnoreCase) && res.ProposedLwd.HasValue)
            {
                res.NoticeShortfallDays = Math.Max(0, (res.ProposedLwd.Value.Date - dto.ApprovedLwd.Date).Days);
            }
            else
            {
                res.NoticeShortfallDays = 0;
            }

            _unitOfWork.Resignation.Update(res);
            _unitOfWork.Save();

            // Auto-instantiate Exit Checklist for the exiting employee
            if (_checklistService != null)
            {
                try
                {
                    await _checklistService.InstantiateChecklistForEmployeeAsync(res.EmployeeId, "Offboarding");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to instantiate offboarding checklist for Employee #{EmployeeId}", res.EmployeeId);
                }
            }

            return true;
        }

        public async Task<bool> FinalizeEmployeeExitAsync(long resignationId, FinalizeExitDto dto)
        {
            var res = await _unitOfWork.Resignation.GetById(resignationId);
            if (res == null || res.IsDeleted) throw new KeyNotFoundException("Resignation not found.");

            var currentEmpId = _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();

            if (!isAdminOrHr)
            {
                throw new UnauthorizedAccessException("Only HR or Admin can finalize an employee exit.");
            }

            // Gate 1: Verify Mandatory Exit Checklist Clearance
            var offboardingChecklist = (await _unitOfWork.EmployeeChecklists.GetAll())
                .FirstOrDefault(c => c.EmployeeId == res.EmployeeId &&
                                     string.Equals(c.ChecklistType, "Offboarding", StringComparison.OrdinalIgnoreCase) &&
                                     !c.IsDeleted);
            if (offboardingChecklist != null)
            {
                var tasks = (await _unitOfWork.ChecklistTasks.GetAll())
                    .Where(t => t.EmployeeChecklistId == offboardingChecklist.Id && !t.IsDeleted && t.IsMandatory)
                    .ToList();
                var pendingTasks = tasks.Where(t => !string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase) &&
                                                    !string.Equals(t.Status, "Waived", StringComparison.OrdinalIgnoreCase))
                                        .ToList();
                if (pendingTasks.Any())
                {
                    var taskNames = string.Join(", ", pendingTasks.Take(3).Select(t => t.TaskName));
                    throw new InvalidOperationException($"Cannot finalize exit: {pendingTasks.Count} mandatory offboarding checklist task(s) are still pending clearance ({taskNames}...). All mandatory tasks must be Completed or Waived with reason.");
                }
            }

            // Gate 2: Verify Asset Clearance
            if (_checklistService != null)
            {
                var assetStatus = await _checklistService.VerifyAssetReturnsAsync(res.EmployeeId);
                if (!assetStatus.AllCleared && string.IsNullOrWhiteSpace(dto.OverrideReason))
                {
                    throw new InvalidOperationException($"Cannot mark employee Exited while {assetStatus.PendingAssets} asset(s) are pending return ({string.Join(", ", assetStatus.PendingAssetDetails)}). An HR override reason is mandatory to proceed.");
                }
            }

            // Gate 3: Verify Final Settlement (FnF) Readiness
            bool isFnFReady = string.Equals(res.FnFStatus, "Prepared", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(res.FnFStatus, "NotApplicable", StringComparison.OrdinalIgnoreCase);
            if (!isFnFReady)
            {
                throw new InvalidOperationException("Cannot finalize exit: Full & Final (FnF) settlement must be marked as 'Prepared' or 'NotApplicable' by HR before the employee can be transitioned to Exited.");
            }

            var now = DateTime.Now;
            res.ResignationStatus = "Exited";
            res.AssetClearanceOverrideReason = dto.OverrideReason;
            res.ExitInterviewRemarks = dto.Remarks;
            res.UpdatedBy = (int)currentEmpId;
            res.UpdatedDate = now;
            _unitOfWork.Resignation.Update(res);

            // Update Employee: EmploymentStatus = Exited, IsActive = false, ExitDate
            var employee = await _unitOfWork.Employees.GetById(res.EmployeeId);
            if (employee != null)
            {
                employee.EmploymentStatus = "Exited";
                employee.IsActive = false;
                employee.ExitDate = res.ApprovedLwd ?? res.ProposedLwd ?? now;
                employee.UpdatedBy = (int)currentEmpId;
                employee.UpdatedDate = now;
                _unitOfWork.Employees.Update(employee);

                // Disable UserLogin
                var userLogin = (await _unitOfWork.UserLogins.GetAll())
                    .FirstOrDefault(u => u.Email != null && u.Email.Equals(employee.EmailAddress, StringComparison.OrdinalIgnoreCase));
                if (userLogin != null)
                {
                    userLogin.IsActive = false;
                    userLogin.UpdatedDate = now;
                    _unitOfWork.UserLogins.Update(userLogin);
                }

                // Reassign pending approvals owned by exiting manager to their superior
                if (employee.ManagerId.HasValue && employee.ManagerId.Value > 0)
                {
                    var superiorId = employee.ManagerId.Value;
                    var pendingSteps = (await _unitOfWork.WorkflowInstanceSteps.GetAll())
                        .Where(s => s.Status == "Pending" && (s.ActionByEmployeeId == employee.Id || (s.EligibleApproverIdsJson != null && s.EligibleApproverIdsJson.Contains(employee.Id.ToString()))))
                        .ToList();

                    foreach (var step in pendingSteps)
                    {
                        step.ActionByEmployeeId = superiorId;
                        var ids = new List<long> { superiorId };
                        step.EligibleApproverIdsJson = JsonSerializer.Serialize(ids);
                        step.UpdatedDate = now;
                        _unitOfWork.WorkflowInstanceSteps.Update(step);
                    }
                }
            }

            return _unitOfWork.Save() > 0;
        }

        public async Task<bool> RehireEmployeeAsync(long employeeId, RehireEmployeeDto dto)
        {
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            if (!isAdminOrHr)
            {
                throw new UnauthorizedAccessException("Only HR or Admin can rehire an exited employee.");
            }

            var oldEmployee = await _unitOfWork.Employees.GetById(employeeId);
            if (oldEmployee == null) throw new KeyNotFoundException($"Employee #{employeeId} not found.");

            if (!string.Equals(oldEmployee.EmploymentStatus, "Exited", StringComparison.OrdinalIgnoreCase) && oldEmployee.IsActive)
            {
                throw new InvalidOperationException("Only exited employees can be rehired.");
            }

            // Check setting: ReusePreviousEmployeeCodeOnRehire (default false)
            var setting = (await _unitOfWork.SystemSettings.GetAll())
                .FirstOrDefault(s => s.SettingKey == "ReusePreviousEmployeeCodeOnRehire" && !s.IsDeleted);
            bool reuseCodeSetting = setting != null && bool.TryParse(setting.SettingValue, out var b) && b;
            bool reuseCode = dto.ReusePreviousEmployeeCode ?? reuseCodeSetting;

            long newCode;
            if (reuseCode && oldEmployee.EmployeeCode.HasValue)
            {
                newCode = oldEmployee.EmployeeCode.Value;
            }
            else
            {
                var allEmps = await _unitOfWork.Employees.GetAll();
                var maxCode = allEmps.Where(e => e.EmployeeCode.HasValue).Select(e => e.EmployeeCode!.Value).DefaultIfEmpty(1000).Max();
                newCode = maxCode + 1;
            }

            var now = DateTime.UtcNow;
            var newJoinDate = dto.NewJoiningDate != default ? dto.NewJoiningDate : DateTime.Today;
            var probationDays = dto.ProbationDays.GetValueOrDefault(90);

            // Create a brand new Employee record for the new tenure
            var newEmployee = new Employee
            {
                EmployeeCode = newCode,
                FirstName = oldEmployee.FirstName,
                LastName = oldEmployee.LastName,
                EmailAddress = oldEmployee.EmailAddress,
                PrimaryEmailAddress = oldEmployee.PrimaryEmailAddress,
                PersonalEmail = oldEmployee.PersonalEmail,
                MobileNumber = oldEmployee.MobileNumber,
                Gender = oldEmployee.Gender,
                DateOfBirth = oldEmployee.DateOfBirth,
                PanNumber = oldEmployee.PanNumber,
                UanNumber = oldEmployee.UanNumber,
                CurrentAddress = oldEmployee.CurrentAddress,
                PermanentAddress = oldEmployee.PermanentAddress,
                Nationality = oldEmployee.Nationality,
                ProfileImage = oldEmployee.ProfileImage,
                DesignationId = dto.DesignationId ?? oldEmployee.DesignationId,
                Designation = dto.Designation ?? oldEmployee.Designation,
                Branch = oldEmployee.Branch,
                ManagerId = dto.ManagerId ?? oldEmployee.ManagerId,
                ShiftIds = !string.IsNullOrWhiteSpace(dto.ShiftId) ? dto.ShiftId : oldEmployee.ShiftIds,
                DateOfJoining = newJoinDate,
                EmploymentStatus = "Active",
                ProbationStatus = probationDays > 0 ? "UnderProbation" : "Confirmed",
                ProbationEndDate = probationDays > 0 ? newJoinDate.AddDays(probationDays) : null,
                EmploymentType = "FullTime",
                PreviousEmployeeId = oldEmployee.Id,
                PersonId = !string.IsNullOrWhiteSpace(oldEmployee.PersonId) ? oldEmployee.PersonId : oldEmployee.Id.ToString(),
                IsActive = true,
                IsDeleted = false,
                CreatedBy = (int)_userContextService.GetCurrentEmployeeId(),
                CreatedDate = now,
                ProfileCompleted = true
            };

            await _unitOfWork.Employees.Add(newEmployee);
            _unitOfWork.Save();

            // The old employee record remains Exited and read-only (unmodified)
            // Repoint UserLogin to the new Employee record & force password reset on first login
            var userLogins = (await _unitOfWork.UserLogins.GetAll())
                .Where(u => u.Email != null && u.Email.Equals(oldEmployee.EmailAddress, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var userLogin in userLogins)
            {
                userLogin.UserId = newEmployee.Id;
                userLogin.IsActive = true;
                userLogin.IsDeleted = false;
                userLogin.IsResetPasswordRequired = true; // Force password reset on next login!
                userLogin.UpdatedDate = now;
                _unitOfWork.UserLogins.Update(userLogin);
            }
            _unitOfWork.Save();

            // Auto-instantiate Onboarding Checklist for the new tenure
            if (_checklistService != null)
            {
                await _checklistService.InstantiateChecklistForEmployeeAsync(newEmployee.Id, "Onboarding");
            }

            return true;
        }

        public async Task<bool> UpdateFnFStatusAsync(long resignationId, FnFStatusUpdateDto dto)
        {
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            if (!isAdminOrHr)
            {
                throw new UnauthorizedAccessException("Only HR or Admin can update Full & Final settlement status.");
            }

            var res = await _unitOfWork.Resignation.GetById(resignationId);
            if (res == null || res.IsDeleted) throw new KeyNotFoundException("Resignation record not found.");

            // INT-03: Verify Asset Clearance before advancing FnF to "Prepared" or "Settled"
            bool isSettlementAdvance = string.Equals(dto.Status, "Prepared", StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(dto.Status, "Settled", StringComparison.OrdinalIgnoreCase);

            if (isSettlementAdvance && _checklistService != null)
            {
                var assetStatus = await _checklistService.VerifyAssetReturnsAsync(res.EmployeeId);
                if (!assetStatus.AllCleared && string.IsNullOrWhiteSpace(dto.OverrideReason))
                {
                    throw new InvalidOperationException(
                        $"Cannot mark FnF settlement as '{dto.Status}' while {assetStatus.PendingAssets} asset(s) are pending return " +
                        $"({string.Join(", ", assetStatus.PendingAssetDetails)}). Company assets must be recovered before final settlement payout, or an HR override reason must be provided.");
                }
            }

            res.FnFStatus = dto.Status;
            if (!string.IsNullOrWhiteSpace(dto.OverrideReason))
            {
                res.AssetClearanceOverrideReason = dto.OverrideReason;
            }
            res.FnFRemarks = dto.Remarks;
            res.FnFPreparedDate = DateTime.UtcNow;
            res.UpdatedBy = (int)_userContextService.GetCurrentEmployeeId();
            res.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Resignation.Update(res);
            return _unitOfWork.Save() > 0;
        }

        public async Task<int> GetOverdueExitsCountAsync()
        {
            var today = DateTime.Today;
            var activeResignations = (await _unitOfWork.Resignation.GetAll())
                .Where(r => !r.IsDeleted && r.IsActive &&
                            r.ResignationStatus != "Exited" &&
                            r.ResignationStatus != "Withdrawn" &&
                            r.ResignationStatus != "Rejected")
                .ToList();

            int count = activeResignations.Count(r =>
            {
                var targetLwd = r.ApprovedLwd ?? r.ProposedLwd ?? r.RequestedLwd;
                return targetLwd.HasValue && targetLwd.Value.Date < today;
            });

            return count;
        }

        public async Task<IEnumerable<Resignation>> GetOverdueExitsAsync()
        {
            var today = DateTime.Today;
            var activeResignations = (await _unitOfWork.Resignation.GetAll())
                .Where(r => !r.IsDeleted && r.IsActive &&
                            r.ResignationStatus != "Exited" &&
                            r.ResignationStatus != "Withdrawn" &&
                            r.ResignationStatus != "Rejected")
                .ToList();

            var overdue = activeResignations.Where(r =>
            {
                var targetLwd = r.ApprovedLwd ?? r.ProposedLwd ?? r.RequestedLwd;
                return targetLwd.HasValue && targetLwd.Value.Date < today;
            }).OrderBy(r => r.ApprovedLwd ?? r.ProposedLwd).ToList();

            return overdue;
        }

        private string GetFormattedDate(DateTime date)
        {
            int day = date.Day;
            string suffix = (day % 10 == 1 && day != 11) ? "st"
                         : (day % 10 == 2 && day != 12) ? "nd"
                         : (day % 10 == 3 && day != 13) ? "rd"
                         : "th";

            return $"{day}{suffix} {date:MMMM yyyy}";
        }

        public async Task<bool> ApproveOrRejectResignation(Resignation resignation)
        {
            if (resignation == null) return false;
            var res = await _unitOfWork.Resignation.GetById(resignation.Id);
            if (res == null) return false;

            var managerid = _userContextService.GetCurrentEmployeeId();
            var manager = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t => t.Id == managerid);
            var employee = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t => t.Id == res.EmployeeId);

            res.Status = resignation.Status;
            res.ResignationStatus = resignation.Status ? "NoticePeriod" : "Rejected";
            res.UpdatedDate = DateTime.Now;
            res.UpdatedBy = (int)managerid;

            string formattedStartDate = GetFormattedDate(res.Resignation_Date);
            var employeeEmail = employee?.EmailAddress ?? string.Empty;
            var managerName = manager != null ? $"{manager.FirstName} {manager.LastName}".Trim() : "Management";
            var employeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : "Employee";

            if (res.Status)
            {
                var temptype = "ResignationRequestApproved";
                var dict = new Dictionary<string, string>
                {
                    { "ManagerName", managerName },
                    { "Resignation-Date", formattedStartDate },
                    { "NoticePeriod", res.NoticePeriod ?? "" },
                    { "EmployeeName", employeeName }
                };
                var subject = $"Resignation Request from {employeeName} is Approved";
                try
                {
                    await _sendEmailService.SendTemplateEmailAsync(employeeEmail, subject, dict, temptype);
                }
                catch { }

                if (employee != null)
                {
                    employee.EmploymentStatus = "NoticePeriod";
                    _unitOfWork.Employees.Update(employee);
                }
            }
            else
            {
                var temptype = "ResignationRequestRejected";
                var dict = new Dictionary<string, string>
                {
                    { "ManagerName", managerName },
                    { "Resignation-Date", formattedStartDate },
                    { "EmployeeName", employeeName }
                };
                var subject = $"Resignation Request from {employeeName} is Rejected";
                try
                {
                    await _sendEmailService.SendTemplateEmailAsync(employeeEmail, subject, dict, temptype);
                }
                catch { }

                if (employee != null)
                {
                    employee.EmploymentStatus = "Active";
                    _unitOfWork.Employees.Update(employee);
                }
            }

            _unitOfWork.Resignation.Update(res);
            return _unitOfWork.Save() > 0;
        }

        public async Task<bool> DeleteResignation(long ResignationId)
        {
            if (ResignationId <= 0) return false;
            var resignation = await _unitOfWork.Resignation.GetById(ResignationId);
            if (resignation != null)
            {
                resignation.IsDeleted = true;
                resignation.IsActive = false;
                _unitOfWork.Resignation.Update(resignation);
                return _unitOfWork.Save() > 0;
            }
            return false;
        }

        public async Task<IEnumerable<ResignationRequestModel>> GetAllResignations()
        {
            var empid = _userContextService.GetCurrentEmployeeId();
            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();
            var allResignations = (await _unitOfWork.Resignation.GetAll()).Where(x => !x.IsDeleted).ToList();
            var roleMappings = await _unitOfWork.EmployeeRoleMappings.GetAll();
            var roles = (await _unitOfWork.RoleMaster.GetAll()).ToDictionary(r => r.Id);

            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR() || await _userContextService.HasPermissionAsync("RESIGNATION_MANAGE");
            bool isManager = _userContextService.IsManager();

            if (!isAdminOrHr && empid <= 0)
            {
                return Enumerable.Empty<ResignationRequestModel>();
            }

            IEnumerable<Resignation> scoped;
            if (isAdminOrHr)
            {
                scoped = allResignations;
            }
            else if (isManager)
            {
                var reporteeIds = allEmployees.Where(e => e.ManagerId == empid).Select(e => e.Id).ToHashSet();
                reporteeIds.Add(empid);
                scoped = allResignations.Where(x => x.ManagerId == empid || reporteeIds.Contains(x.EmployeeId));
            }
            else
            {
                scoped = allResignations.Where(x => x.EmployeeId == empid);
            }

            var employeeDictionary = allEmployees.ToDictionary(emp => emp.Id);

            var result = scoped.Select(p =>
            {
                employeeDictionary.TryGetValue(p.ManagerId, out var mgr);
                employeeDictionary.TryGetValue(p.EmployeeId, out var emp);

                return new ResignationRequestModel
                {
                    Id = p.Id,
                    EmployeeId = p.EmployeeId,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : null,
                    EmployeeEmail = emp?.EmailAddress,
                    Department = emp?.Branch,
                    Reason = p.Reason,
                    Status = p.Status,
                    ManagerId = p.ManagerId,
                    ManagerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : null,
                    NoticePeriod = p.NoticePeriod,
                    Resignation_Date = p.Resignation_Date,
                    ResignationStatus = p.ResignationStatus ?? (p.Status ? "Approved" : "Submitted"),
                    RequestedLwd = p.RequestedLwd,
                    ProposedLwd = p.ProposedLwd,
                    ApprovedLwd = p.ApprovedLwd,
                    NoticeShortfallDays = p.NoticeShortfallDays,
                    NoticeHandling = p.NoticeHandling,
                    ManagerRemarks = p.ManagerRemarks,
                    HrRemarks = p.HrRemarks,
                    ExitInterviewRemarks = p.ExitInterviewRemarks,
                    IsLwdLocked = p.IsLwdLocked,
                    AssetClearanceOverrideReason = p.AssetClearanceOverrideReason,
                    RoleId = roleMappings
                        .Where(mapping => mapping.EmployeeId == p.EmployeeId && mapping.IsActive)
                        .Select(mapping => mapping.RoleId)
                        .Where(roleId => roles.ContainsKey(roleId))
                        .ToList(),
                    RoleName = roleMappings
                        .Where(mapping => mapping.EmployeeId == p.EmployeeId && mapping.IsActive)
                        .Select(mapping => mapping.RoleId)
                        .Where(roleId => roles.ContainsKey(roleId))
                        .Select(roleId => roles[roleId].RoleName)
                        .ToList(),
                };
            }).OrderByDescending(r => r.Resignation_Date).ToList();

            return result;
        }

        public async Task<Resignation?> GetResignationById(int resignationId)
        {
            var res = await _unitOfWork.Resignation.GetByIdAsync(resignationId);
            if (res == null || res.IsDeleted) return null;

            var empid = _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR() || await _userContextService.HasPermissionAsync("RESIGNATION_MANAGE");
            bool isManager = _userContextService.IsManager();

            if (!isAdminOrHr)
            {
                if (res.EmployeeId != empid)
                {
                    if (isManager)
                    {
                        var emp = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(e => e.Id == res.EmployeeId);
                        if (emp == null || (emp.ManagerId != empid && res.ManagerId != empid))
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return res;
        }

        public async Task<bool> UpdateResignation(Resignation resignation)
        {
            if (resignation == null) return false;
            var resgin = await _unitOfWork.Resignation.GetById(resignation.Id);
            if (resgin == null || resgin.IsDeleted) return false;

            var empid = _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR() || await _userContextService.HasPermissionAsync("RESIGNATION_MANAGE");

            if (!isAdminOrHr && resgin.EmployeeId != empid)
            {
                throw new UnauthorizedAccessException("You are not authorized to update another employee's resignation.");
            }

            resgin.Reason = resignation.Reason;
            resgin.NoticePeriod = resignation.NoticePeriod;
            if (resignation.RequestedLwd.HasValue) resgin.RequestedLwd = resignation.RequestedLwd;
            if (resignation.ProposedLwd.HasValue) resgin.ProposedLwd = resignation.ProposedLwd;
            resgin.UpdatedDate = DateTime.Now;

            _unitOfWork.Resignation.Update(resgin);
            return _unitOfWork.Save() > 0;
        }
    }
}
