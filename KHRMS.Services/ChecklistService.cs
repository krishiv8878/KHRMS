using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Serilog;

namespace KHRMS.Services
{
    public class ChecklistService : IChecklistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;
        private readonly IAuditService? _auditService;

        public ChecklistService(IUnitOfWork unitOfWork, IUserContextService userContextService, IAuditService? auditService = null)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _auditService = auditService;
        }

        public async Task<IEnumerable<ChecklistTemplateDto>> GetTemplatesAsync(string? checklistType = null, string? department = null)
        {
            var templates = (await _unitOfWork.ChecklistTemplates.GetAll())
                .Where(t => !t.IsDeleted && t.IsActive)
                .ToList();

            if (!string.IsNullOrWhiteSpace(checklistType))
            {
                templates = templates.Where(t => string.Equals(t.ChecklistType, checklistType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(department) && !string.Equals(department, "All", StringComparison.OrdinalIgnoreCase))
            {
                templates = templates.Where(t => string.Equals(t.Department, "All", StringComparison.OrdinalIgnoreCase) ||
                                                 string.Equals(t.Department, department, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var allTasks = (await _unitOfWork.ChecklistTemplateTasks.GetAll())
                .Where(tk => !tk.IsDeleted && tk.IsActive)
                .ToList();

            var result = templates.Select(tpl => new ChecklistTemplateDto
            {
                Id = tpl.Id,
                Name = tpl.Name,
                ChecklistType = tpl.ChecklistType,
                Description = tpl.Description,
                Department = tpl.Department,
                Tasks = allTasks.Where(tk => tk.TemplateId == tpl.Id)
                                .OrderBy(tk => tk.DisplayOrder)
                                .Select(tk => new ChecklistTemplateTaskDto
                                {
                                    Id = tk.Id,
                                    TemplateId = tk.TemplateId,
                                    TaskName = tk.TaskName,
                                    Description = tk.Description,
                                    Category = tk.Category,
                                    DefaultAssigneeRole = tk.DefaultAssigneeRole,
                                    DueDays = tk.DueDays,
                                    IsMandatory = tk.IsMandatory,
                                    DisplayOrder = tk.DisplayOrder
                                }).ToList()
            }).ToList();

            return result;
        }

        public async Task<ChecklistTemplateDto?> GetTemplateByIdAsync(long id)
        {
            var tpl = await _unitOfWork.ChecklistTemplates.GetById(id);
            if (tpl == null || tpl.IsDeleted) return null;

            var tasks = (await _unitOfWork.ChecklistTemplateTasks.GetAll())
                .Where(tk => tk.TemplateId == id && !tk.IsDeleted && tk.IsActive)
                .OrderBy(tk => tk.DisplayOrder)
                .Select(tk => new ChecklistTemplateTaskDto
                {
                    Id = tk.Id,
                    TemplateId = tk.TemplateId,
                    TaskName = tk.TaskName,
                    Description = tk.Description,
                    Category = tk.Category,
                    DefaultAssigneeRole = tk.DefaultAssigneeRole,
                    DueDays = tk.DueDays,
                    IsMandatory = tk.IsMandatory,
                    DisplayOrder = tk.DisplayOrder
                }).ToList();

            return new ChecklistTemplateDto
            {
                Id = tpl.Id,
                Name = tpl.Name,
                ChecklistType = tpl.ChecklistType,
                Description = tpl.Description,
                Department = tpl.Department,
                Tasks = tasks
            };
        }

        public async Task<ChecklistTemplateDto> SaveTemplateAsync(ChecklistTemplateDto dto)
        {
            var currentUserId = (int)_userContextService.GetCurrentEmployeeId();
            var now = DateTime.UtcNow;

            ChecklistTemplate template;
            if (dto.Id > 0)
            {
                template = await _unitOfWork.ChecklistTemplates.GetById(dto.Id)
                    ?? throw new KeyNotFoundException($"Template {dto.Id} not found");

                template.Name = dto.Name;
                template.ChecklistType = dto.ChecklistType;
                template.Description = dto.Description;
                template.Department = dto.Department;
                template.UpdatedBy = currentUserId;
                template.UpdatedDate = now;
                _unitOfWork.ChecklistTemplates.Update(template);
            }
            else
            {
                template = new ChecklistTemplate
                {
                    Name = dto.Name,
                    ChecklistType = dto.ChecklistType,
                    Description = dto.Description,
                    Department = dto.Department ?? "All",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId,
                    CreatedDate = now
                };
                await _unitOfWork.ChecklistTemplates.Add(template);
            }
            _unitOfWork.Save();

            // Sync tasks
            var existingTasks = (await _unitOfWork.ChecklistTemplateTasks.GetAll())
                .Where(t => t.TemplateId == template.Id && !t.IsDeleted)
                .ToList();

            var incomingIds = dto.Tasks.Where(t => t.Id > 0).Select(t => t.Id).ToHashSet();

            // Soft-delete removed tasks
            foreach (var et in existingTasks.Where(t => !incomingIds.Contains(t.Id)))
            {
                et.IsDeleted = true;
                et.UpdatedBy = currentUserId;
                et.UpdatedDate = now;
                _unitOfWork.ChecklistTemplateTasks.Update(et);
            }

            int order = 1;
            foreach (var tDto in dto.Tasks)
            {
                if (tDto.Id > 0)
                {
                    var et = existingTasks.FirstOrDefault(t => t.Id == tDto.Id);
                    if (et != null)
                    {
                        et.TaskName = tDto.TaskName;
                        et.Description = tDto.Description;
                        et.Category = tDto.Category;
                        et.DefaultAssigneeRole = tDto.DefaultAssigneeRole;
                        et.DueDays = tDto.DueDays;
                        et.IsMandatory = tDto.IsMandatory;
                        et.DisplayOrder = order++;
                        et.UpdatedBy = currentUserId;
                        et.UpdatedDate = now;
                        _unitOfWork.ChecklistTemplateTasks.Update(et);
                    }
                }
                else
                {
                    var newTask = new ChecklistTemplateTask
                    {
                        TemplateId = template.Id,
                        TaskName = tDto.TaskName,
                        Description = tDto.Description,
                        Category = tDto.Category,
                        DefaultAssigneeRole = tDto.DefaultAssigneeRole,
                        DueDays = tDto.DueDays,
                        IsMandatory = tDto.IsMandatory,
                        DisplayOrder = order++,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = currentUserId,
                        CreatedDate = now
                    };
                    await _unitOfWork.ChecklistTemplateTasks.Add(newTask);
                }
            }
            _unitOfWork.Save();

            return (await GetTemplateByIdAsync(template.Id))!;
        }

        public async Task<bool> DeleteTemplateAsync(long id)
        {
            var template = await _unitOfWork.ChecklistTemplates.GetById(id);
            if (template == null) return false;

            template.IsDeleted = true;
            template.IsActive = false;
            template.UpdatedBy = (int)_userContextService.GetCurrentEmployeeId();
            template.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.ChecklistTemplates.Update(template);
            return _unitOfWork.Save() > 0;
        }

        public async Task<EmployeeChecklistDto> InstantiateChecklistForEmployeeAsync(long employeeId, string checklistType, long? templateId = null)
        {
            var employee = await _unitOfWork.Employees.GetById(employeeId)
                ?? throw new KeyNotFoundException($"Employee #{employeeId} not found");

            ChecklistTemplate? template = null;
            if (templateId.HasValue && templateId.Value > 0)
            {
                template = await _unitOfWork.ChecklistTemplates.GetById(templateId.Value);
            }

            if (template == null)
            {
                var templates = (await _unitOfWork.ChecklistTemplates.GetAll())
                    .Where(t => !t.IsDeleted && t.IsActive && string.Equals(t.ChecklistType, checklistType, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                template = templates.FirstOrDefault(t => !string.IsNullOrEmpty(employee.Branch) && string.Equals(t.Department, employee.Branch, StringComparison.OrdinalIgnoreCase))
                        ?? templates.FirstOrDefault(t => string.Equals(t.Department, "All", StringComparison.OrdinalIgnoreCase))
                        ?? templates.FirstOrDefault();
            }

            List<ChecklistTemplateTask> templateTasks = new();
            if (template != null)
            {
                templateTasks = (await _unitOfWork.ChecklistTemplateTasks.GetAll())
                    .Where(t => t.TemplateId == template.Id && !t.IsDeleted && t.IsActive)
                    .OrderBy(t => t.DisplayOrder)
                    .ToList();
            }

            var now = DateTime.UtcNow;
            var currentUserId = (int)_userContextService.GetCurrentEmployeeId();

            var employeeChecklist = new EmployeeChecklist
            {
                EmployeeId = employeeId,
                TemplateId = template?.Id,
                ChecklistType = checklistType,
                Title = $"{checklistType} Checklist - {employee.FirstName} {employee.LastName}".Trim(),
                Status = "InProgress",
                TotalTasks = 0,
                CompletedTasks = 0,
                InitiatedDate = now,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = currentUserId > 0 ? currentUserId : (int)employeeId,
                CreatedDate = now
            };

            await _unitOfWork.EmployeeChecklists.Add(employeeChecklist);
            _unitOfWork.Save();

            // If no tasks were configured in DB template, provide fallback default tasks
            if (!templateTasks.Any())
            {
                if (string.Equals(checklistType, "Onboarding", StringComparison.OrdinalIgnoreCase))
                {
                    templateTasks = new List<ChecklistTemplateTask>
                    {
                        new() { TaskName = "Upload Identity & Address Proofs", Category = "Employee", DefaultAssigneeRole = "Employee", DueDays = 3, IsMandatory = true, DisplayOrder = 1 },
                        new() { TaskName = "Fill Profile & Emergency Contacts", Category = "Employee", DefaultAssigneeRole = "Employee", DueDays = 2, IsMandatory = true, DisplayOrder = 2 },
                        new() { TaskName = "Acknowledge Company Policies & Handbook", Category = "Employee", DefaultAssigneeRole = "Employee", DueDays = 5, IsMandatory = true, DisplayOrder = 3 },
                        new() { TaskName = "Verify Uploaded Documents", Category = "HR", DefaultAssigneeRole = "HR", DueDays = 4, IsMandatory = true, DisplayOrder = 4 },
                        new() { TaskName = "Assign Shift & Leave Allocation Policy", Category = "HR", DefaultAssigneeRole = "HR", DueDays = 2, IsMandatory = true, DisplayOrder = 5 },
                        new() { TaskName = "Provision Work Email & System Accounts", Category = "IT", DefaultAssigneeRole = "IT", DueDays = 1, IsMandatory = true, DisplayOrder = 6 },
                        new() { TaskName = "Assign Laptop & Hardware Assets", Category = "IT", DefaultAssigneeRole = "IT", DueDays = 2, IsMandatory = true, DisplayOrder = 7 }
                    };
                }
                else
                {
                    templateTasks = new List<ChecklistTemplateTask>
                    {
                        new() { TaskName = "Handover Documentation & KT Notes", Category = "Employee", DefaultAssigneeRole = "Employee", DueDays = 10, IsMandatory = true, DisplayOrder = 1 },
                        new() { TaskName = "Manager Knowledge Transfer Sign-Off", Category = "Manager", DefaultAssigneeRole = "Manager", DueDays = 12, IsMandatory = true, DisplayOrder = 2 },
                        new() { TaskName = "Asset Return: Laptop, Hardware & Badges", Category = "IT", DefaultAssigneeRole = "IT", DueDays = 14, IsMandatory = true, DisplayOrder = 3 },
                        new() { TaskName = "Deactivate Email & SSO Access", Category = "IT", DefaultAssigneeRole = "IT", DueDays = 15, IsMandatory = true, DisplayOrder = 4 },
                        new() { TaskName = "Finance & Expense Settlement Clearance", Category = "Finance", DefaultAssigneeRole = "Finance", DueDays = 14, IsMandatory = true, DisplayOrder = 5 },
                        new() { TaskName = "Exit Interview Completion", Category = "HR", DefaultAssigneeRole = "HR", DueDays = 13, IsMandatory = true, DisplayOrder = 6 },
                        new() { TaskName = "Full & Final Settlement Preparation", Category = "HR", DefaultAssigneeRole = "HR", DueDays = 15, IsMandatory = true, DisplayOrder = 7 }
                    };
                }
            }

            foreach (var tt in templateTasks)
            {
                long? assignedEmpId = null;
                if (string.Equals(tt.Category, "Employee", StringComparison.OrdinalIgnoreCase))
                {
                    assignedEmpId = employeeId;
                }
                else if (string.Equals(tt.Category, "Manager", StringComparison.OrdinalIgnoreCase) && employee.ManagerId.HasValue)
                {
                    assignedEmpId = employee.ManagerId.Value;
                }
                else if (tt.DefaultAssigneeEmployeeId.HasValue && tt.DefaultAssigneeEmployeeId.Value > 0)
                {
                    assignedEmpId = tt.DefaultAssigneeEmployeeId.Value;
                }

                var task = new ChecklistTask
                {
                    EmployeeChecklistId = employeeChecklist.Id,
                    TaskName = tt.TaskName,
                    Description = tt.Description,
                    Category = tt.Category,
                    Status = "Pending",
                    DueDate = now.AddDays(tt.DueDays > 0 ? tt.DueDays : 3),
                    AssignedEmployeeId = assignedEmpId,
                    AssignedRole = tt.DefaultAssigneeRole ?? tt.Category,
                    IsMandatory = tt.IsMandatory,
                    DisplayOrder = tt.DisplayOrder,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId > 0 ? currentUserId : (int)employeeId,
                    CreatedDate = now
                };
                await _unitOfWork.ChecklistTasks.Add(task);
            }

            employeeChecklist.TotalTasks = templateTasks.Count;
            _unitOfWork.EmployeeChecklists.Update(employeeChecklist);
            _unitOfWork.Save();

            return (await GetChecklistByIdAsync(employeeChecklist.Id))!;
        }

        public async Task<IEnumerable<EmployeeChecklistDto>> GetEmployeeChecklistsAsync(long? employeeId = null, string? checklistType = null)
        {
            var checklists = (await _unitOfWork.EmployeeChecklists.GetAll())
                .Where(c => !c.IsDeleted)
                .ToList();

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                checklists = checklists.Where(c => c.EmployeeId == employeeId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(checklistType))
            {
                checklists = checklists.Where(c => string.Equals(c.ChecklistType, checklistType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);
            var allTasks = (await _unitOfWork.ChecklistTasks.GetAll())
                .Where(t => !t.IsDeleted)
                .ToList();

            var result = checklists.Select(c =>
            {
                allEmployees.TryGetValue(c.EmployeeId, out var emp);
                var cTasks = allTasks.Where(t => t.EmployeeChecklistId == c.Id).OrderBy(t => t.DisplayOrder).ToList();
                int completed = cTasks.Count(t => t.Status == "Completed" || t.Status == "Waived");

                return new EmployeeChecklistDto
                {
                    Id = c.Id,
                    EmployeeId = c.EmployeeId,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : null,
                    EmployeeEmail = emp?.EmailAddress,
                    Department = emp?.Branch,
                    TemplateId = c.TemplateId,
                    ChecklistType = c.ChecklistType,
                    Title = c.Title,
                    Status = c.Status,
                    TotalTasks = cTasks.Count,
                    CompletedTasks = completed,
                    InitiatedDate = c.InitiatedDate,
                    CompletedDate = c.CompletedDate,
                    Remarks = c.Remarks,
                    Tasks = cTasks.Select(t =>
                    {
                        allEmployees.TryGetValue(t.AssignedEmployeeId ?? 0, out var assignEmp);
                        allEmployees.TryGetValue(t.ClearedByEmployeeId ?? 0, out var clearEmp);
                        allEmployees.TryGetValue(t.WaivedByEmployeeId ?? 0, out var waiveEmp);

                        var currentUserId = _userContextService.GetCurrentEmployeeId();
                        bool isOwnOffboarding = string.Equals(c.ChecklistType, "Offboarding", StringComparison.OrdinalIgnoreCase) && c.EmployeeId == currentUserId;
                        bool canClearThis = (!isOwnOffboarding || string.Equals(t.Category, "Employee", StringComparison.OrdinalIgnoreCase)) &&
                                            (t.Status == "Pending" || t.Status == "InProgress");
                        bool canWaiveThis = (t.Status == "Pending" || t.Status == "InProgress") && !isOwnOffboarding;

                        return new ChecklistTaskDto
                        {
                            Id = t.Id,
                            EmployeeChecklistId = t.EmployeeChecklistId,
                            TaskName = t.TaskName,
                            Description = t.Description,
                            Category = t.Category,
                            Status = t.Status,
                            DueDate = t.DueDate,
                            AssignedEmployeeId = t.AssignedEmployeeId,
                            AssignedEmployeeName = assignEmp != null ? $"{assignEmp.FirstName} {assignEmp.LastName}".Trim() : null,
                            AssignedRole = t.AssignedRole,
                            IsMandatory = t.IsMandatory,
                            ClearedDate = t.ClearedDate,
                            ClearedByEmployeeId = t.ClearedByEmployeeId,
                            ClearedByEmployeeName = clearEmp != null ? $"{clearEmp.FirstName} {clearEmp.LastName}".Trim() : null,
                            Remarks = t.Remarks,
                            WaiveReason = t.WaiveReason,
                            WaivedByEmployeeId = t.WaivedByEmployeeId,
                            WaivedByEmployeeName = waiveEmp != null ? $"{waiveEmp.FirstName} {waiveEmp.LastName}".Trim() : null,
                            AttachmentUrl = t.AttachmentUrl,
                            DisplayOrder = t.DisplayOrder,
                            CanClear = canClearThis,
                            CanWaive = canWaiveThis
                        };
                    }).ToList()
                };
            }).OrderByDescending(c => c.InitiatedDate).ToList();

            return result;
        }

        public async Task<EmployeeChecklistDto?> GetChecklistByIdAsync(long id)
        {
            var c = await _unitOfWork.EmployeeChecklists.GetById(id);
            if (c == null || c.IsDeleted) return null;

            var emp = await _unitOfWork.Employees.GetById(c.EmployeeId);
            var tasks = (await _unitOfWork.ChecklistTasks.GetAll())
                .Where(t => t.EmployeeChecklistId == id && !t.IsDeleted)
                .OrderBy(t => t.DisplayOrder)
                .ToList();

            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);

            int completed = tasks.Count(t => t.Status == "Completed" || t.Status == "Waived");

            return new EmployeeChecklistDto
            {
                Id = c.Id,
                EmployeeId = c.EmployeeId,
                EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : null,
                EmployeeEmail = emp?.EmailAddress,
                Department = emp?.Branch,
                TemplateId = c.TemplateId,
                ChecklistType = c.ChecklistType,
                Title = c.Title,
                Status = c.Status,
                TotalTasks = tasks.Count,
                CompletedTasks = completed,
                InitiatedDate = c.InitiatedDate,
                CompletedDate = c.CompletedDate,
                Remarks = c.Remarks,
                Tasks = tasks.Select(t =>
                {
                    allEmployees.TryGetValue(t.AssignedEmployeeId ?? 0, out var assignEmp);
                    allEmployees.TryGetValue(t.ClearedByEmployeeId ?? 0, out var clearEmp);
                    allEmployees.TryGetValue(t.WaivedByEmployeeId ?? 0, out var waiveEmp);

                    var currentUserId = _userContextService.GetCurrentEmployeeId();
                    bool isOwnOffboarding = string.Equals(c.ChecklistType, "Offboarding", StringComparison.OrdinalIgnoreCase) && c.EmployeeId == currentUserId;
                    bool canClearThis = (!isOwnOffboarding || string.Equals(t.Category, "Employee", StringComparison.OrdinalIgnoreCase)) &&
                                        (t.Status == "Pending" || t.Status == "InProgress");
                    bool canWaiveThis = (t.Status == "Pending" || t.Status == "InProgress") && !isOwnOffboarding;

                    return new ChecklistTaskDto
                    {
                        Id = t.Id,
                        EmployeeChecklistId = t.EmployeeChecklistId,
                        TaskName = t.TaskName,
                        Description = t.Description,
                        Category = t.Category,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        AssignedEmployeeId = t.AssignedEmployeeId,
                        AssignedEmployeeName = assignEmp != null ? $"{assignEmp.FirstName} {assignEmp.LastName}".Trim() : null,
                        AssignedRole = t.AssignedRole,
                        IsMandatory = t.IsMandatory,
                        ClearedDate = t.ClearedDate,
                        ClearedByEmployeeId = t.ClearedByEmployeeId,
                        ClearedByEmployeeName = clearEmp != null ? $"{clearEmp.FirstName} {clearEmp.LastName}".Trim() : null,
                        Remarks = t.Remarks,
                        WaiveReason = t.WaiveReason,
                        WaivedByEmployeeId = t.WaivedByEmployeeId,
                        WaivedByEmployeeName = waiveEmp != null ? $"{waiveEmp.FirstName} {waiveEmp.LastName}".Trim() : null,
                        AttachmentUrl = t.AttachmentUrl,
                        DisplayOrder = t.DisplayOrder,
                        CanClear = canClearThis,
                        CanWaive = canWaiveThis
                    };
                }).ToList()
            };
        }

        public async Task<bool> UpdateTaskStatusAsync(long taskId, ChecklistTaskUpdateDto dto)
        {
            var task = await _unitOfWork.ChecklistTasks.GetById(taskId);
            if (task == null || task.IsDeleted) return false;

            var parent = await _unitOfWork.EmployeeChecklists.GetById(task.EmployeeChecklistId);
            if (parent == null || parent.IsDeleted) return false;

            var currentUserId = _userContextService.GetCurrentEmployeeId();
            bool isAdmin = _userContextService.IsAdmin();
            bool isHr = _userContextService.IsHR();
            var now = DateTime.UtcNow;

            // Rule 1: Nobody can clear a clearance task in their own exit checklist
            if (string.Equals(parent.ChecklistType, "Offboarding", StringComparison.OrdinalIgnoreCase) &&
                parent.EmployeeId == currentUserId &&
                !string.Equals(task.Category, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Security Rule Violation: You are not permitted to sign off or clear checklist clearance tasks for your own offboarding.");
            }

            if (string.Equals(dto.Status, "Waived", StringComparison.OrdinalIgnoreCase))
            {
                // Rule 2: Waiving requires EXIT_TASK_WAIVE permission and mandatory justification reason
                bool canWaive = isAdmin || await _userContextService.HasPermissionAsync("EXIT_TASK_WAIVE");
                if (!canWaive)
                {
                    throw new UnauthorizedAccessException("Permission Denied: Waiving an exit checklist task requires the privileged 'EXIT_TASK_WAIVE' permission.");
                }

                if (string.IsNullOrWhiteSpace(dto.WaiveReason))
                {
                    throw new InvalidOperationException("A mandatory justification reason is required when waiving an exit checklist task.");
                }

                task.Status = "Waived";
                task.WaiveReason = dto.WaiveReason;
                task.WaivedByEmployeeId = currentUserId > 0 ? currentUserId : null;
                task.ClearedDate = now;
                task.ClearedByEmployeeId = currentUserId > 0 ? currentUserId : null;
                task.Remarks = dto.Remarks ?? dto.WaiveReason;

                if (_auditService != null)
                {
                    await _auditService.LogAsync("Offboarding", "ChecklistTask", taskId.ToString(), "WAIVE",
                        new { task.TaskName, OldStatus = "Pending" },
                        new { task.TaskName, NewStatus = "Waived", dto.WaiveReason },
                        dto.WaiveReason);
                }
            }
            else if (string.Equals(dto.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                // Rule 3: Clearance Authority Verification
                bool isAuthorized = false;

                // Direct Named Assignee check
                if (task.AssignedEmployeeId.HasValue && task.AssignedEmployeeId.Value == currentUserId)
                {
                    isAuthorized = true;
                }
                else
                {
                    // Fallback to department queue clearance permissions
                    switch (task.Category.ToUpperInvariant())
                    {
                        case "HR":
                            isAuthorized = isAdmin || isHr || await _userContextService.HasPermissionAsync("EXIT_CLEAR_HR");
                            break;
                        case "IT":
                            isAuthorized = isAdmin || await _userContextService.HasPermissionAsync("EXIT_CLEAR_IT");
                            break;
                        case "FINANCE":
                            isAuthorized = isAdmin || await _userContextService.HasPermissionAsync("EXIT_CLEAR_FINANCE");
                            break;
                        case "FACILITIES":
                            isAuthorized = isAdmin || await _userContextService.HasPermissionAsync("EXIT_CLEAR_FACILITIES");
                            break;
                        case "MANAGER":
                            var subjectEmp = await _unitOfWork.Employees.GetById(parent.EmployeeId);
                            isAuthorized = isAdmin || isHr || (subjectEmp != null && subjectEmp.ManagerId == currentUserId);
                            break;
                        case "EMPLOYEE":
                            isAuthorized = isAdmin || isHr || (parent.EmployeeId == currentUserId);
                            break;
                        default:
                            isAuthorized = isAdmin || isHr;
                            break;
                    }
                }

                if (!isAuthorized)
                {
                    throw new UnauthorizedAccessException($"Permission Denied: You do not have clearance authority for category '{task.Category}'. Required permission: EXIT_CLEAR_{task.Category.ToUpperInvariant()} or assigned named role.");
                }

                task.Status = "Completed";
                task.ClearedDate = now;
                task.ClearedByEmployeeId = currentUserId > 0 ? currentUserId : null;
                task.Remarks = dto.Remarks;
                if (!string.IsNullOrWhiteSpace(dto.AttachmentUrl))
                {
                    task.AttachmentUrl = dto.AttachmentUrl;
                }
            }
            else
            {
                task.Status = dto.Status;
                task.ClearedDate = null;
                task.ClearedByEmployeeId = null;
                task.Remarks = dto.Remarks;
            }

            task.UpdatedBy = (int)currentUserId;
            task.UpdatedDate = now;
            _unitOfWork.ChecklistTasks.Update(task);
            _unitOfWork.Save();

            // Recompute parent checklist progress
            var allTasks = (await _unitOfWork.ChecklistTasks.GetAll())
                .Where(t => t.EmployeeChecklistId == task.EmployeeChecklistId && !t.IsDeleted)
                .ToList();

            if (parent != null)
            {
                parent.TotalTasks = allTasks.Count;
                parent.CompletedTasks = allTasks.Count(t => t.Status == "Completed" || t.Status == "Waived");

                bool allDone = allTasks.All(t => !t.IsMandatory || t.Status == "Completed" || t.Status == "Waived");
                if (allDone)
                {
                    parent.Status = "Completed";
                    parent.CompletedDate = now;
                }
                else
                {
                    parent.Status = "InProgress";
                    parent.CompletedDate = null;
                }

                parent.UpdatedBy = (int)currentUserId;
                parent.UpdatedDate = now;
                _unitOfWork.EmployeeChecklists.Update(parent);
                _unitOfWork.Save();
            }

            return true;
        }

        public async Task<bool> UpdateTaskAssigneeAsync(long taskId, TaskAssigneeUpdateDto dto)
        {
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            if (!isAdminOrHr)
            {
                throw new UnauthorizedAccessException("Only HR or Admin can change task assignees on an exit checklist.");
            }

            var task = await _unitOfWork.ChecklistTasks.GetById(taskId);
            if (task == null || task.IsDeleted) return false;

            task.AssignedEmployeeId = dto.AssignedEmployeeId;
            if (!string.IsNullOrWhiteSpace(dto.AssignedRole))
            {
                task.AssignedRole = dto.AssignedRole;
            }
            task.UpdatedBy = (int)_userContextService.GetCurrentEmployeeId();
            task.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.ChecklistTasks.Update(task);
            return _unitOfWork.Save() > 0;
        }

        public async Task<AssetReturnVerificationDto> VerifyAssetReturnsAsync(long employeeId)
        {
            var assignedAssets = (await _unitOfWork.AssetsMasters.GetAll())
                .Where(a => a.EmployeeId == employeeId && !a.IsDeleted &&
                            !string.Equals(a.Status, "Available", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(a.Status, "Returned", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var allPending = assignedAssets.Select(a => $"{a.AssetsMasterName ?? "Asset"} (S/N: {a.SerialNumber ?? "N/A"}, Type: {a.AssetType ?? "Hardware"})").ToList();

            return new AssetReturnVerificationDto
            {
                TotalAssets = assignedAssets.Count,
                PendingAssets = assignedAssets.Count,
                ReturnedAssets = 0,
                AllCleared = !assignedAssets.Any(),
                PendingAssetDetails = allPending
            };
        }

        public async Task<IEnumerable<ChecklistTaskDto>> GetPendingTasksForUserAsync(long? employeeId = null)
        {
            var empId = employeeId ?? _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            bool isManager = _userContextService.IsManager();

            var allTasks = (await _unitOfWork.ChecklistTasks.GetAll())
                .Where(t => !t.IsDeleted && (t.Status == "Pending" || t.Status == "InProgress"))
                .ToList();

            var allChecklists = (await _unitOfWork.EmployeeChecklists.GetAll())
                .Where(c => !c.IsDeleted)
                .ToDictionary(c => c.Id);

            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);

            IEnumerable<ChecklistTask> scopedTasks;
            if (isAdminOrHr)
            {
                scopedTasks = allTasks;
            }
            else
            {
                scopedTasks = allTasks.Where(t =>
                {
                    if (t.AssignedEmployeeId.HasValue && t.AssignedEmployeeId.Value == empId) return true;
                    if (allChecklists.TryGetValue(t.EmployeeChecklistId, out var chk))
                    {
                        if (chk.EmployeeId == empId && string.Equals(t.Category, "Employee", StringComparison.OrdinalIgnoreCase)) return true;
                        if (isManager && allEmployees.TryGetValue(chk.EmployeeId, out var subjectEmp) && subjectEmp.ManagerId == empId && string.Equals(t.Category, "Manager", StringComparison.OrdinalIgnoreCase)) return true;
                    }
                    return false;
                });
            }

            return scopedTasks.Select(t =>
            {
                allEmployees.TryGetValue(t.AssignedEmployeeId ?? 0, out var assignEmp);
                allEmployees.TryGetValue(t.ClearedByEmployeeId ?? 0, out var clearEmp);

                return new ChecklistTaskDto
                {
                    Id = t.Id,
                    EmployeeChecklistId = t.EmployeeChecklistId,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    Category = t.Category,
                    Status = t.Status,
                    DueDate = t.DueDate,
                    AssignedEmployeeId = t.AssignedEmployeeId,
                    AssignedEmployeeName = assignEmp != null ? $"{assignEmp.FirstName} {assignEmp.LastName}".Trim() : null,
                    AssignedRole = t.AssignedRole,
                    IsMandatory = t.IsMandatory,
                    ClearedDate = t.ClearedDate,
                    ClearedByEmployeeId = t.ClearedByEmployeeId,
                    ClearedByEmployeeName = clearEmp != null ? $"{clearEmp.FirstName} {clearEmp.LastName}".Trim() : null,
                    Remarks = t.Remarks,
                    AttachmentUrl = t.AttachmentUrl,
                    DisplayOrder = t.DisplayOrder
                };
            }).OrderBy(t => t.DueDate).ToList();
        }
    }
}
