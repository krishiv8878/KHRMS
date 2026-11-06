using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _memoryCache;
        private readonly IAuditService _auditService;

        public PermissionService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _memoryCache = memoryCache;
            _auditService = auditService;
        }

        public async Task<IEnumerable<PermissionMaster>> GetAllPermissionsAsync()
        {
            await SeedDefaultPermissionsAsync();
            var all = await _unitOfWork.PermissionMaster.GetAll();
            return all.Where(p => p.IsActive && !p.IsDeleted)
                      .OrderBy(p => p.SortOrder)
                      .ThenBy(p => p.ModuleName)
                      .ToList();
        }

        public async Task<List<long>> GetRolePermissionIdsAsync(long roleId)
        {
            var mappings = await _unitOfWork.RolePermissionMappings.GetAll();
            return mappings.Where(m => m.RoleId == roleId && m.IsActive && !m.IsDeleted)
                           .Select(m => m.PermissionId)
                           .Distinct()
                           .ToList();
        }

        public async Task<List<RolePermissionViewDto>> GetRolePermissionsWithScopeAsync(long roleId)
        {
            var allPerms = (await GetAllPermissionsAsync()).ToList();
            var mappings = (await _unitOfWork.RolePermissionMappings.GetAll())
                .Where(m => m.RoleId == roleId && m.IsActive && !m.IsDeleted)
                .ToDictionary(m => m.PermissionId, m => m.Scope ?? "All");

            return allPerms.Select(p => new RolePermissionViewDto
            {
                PermissionId = p.Id,
                PermissionCode = p.PermissionCode,
                ModuleName = p.ModuleName,
                DisplayName = p.DisplayName,
                Description = p.Description,
                Scope = mappings.ContainsKey(p.Id) ? mappings[p.Id] : "All",
                IsSensitive = p.IsSensitive
            }).ToList();
        }

        public async Task<bool> SaveRolePermissionsAsync(long roleId, List<long> permissionIds)
        {
            var defaultAssignments = (permissionIds ?? new List<long>())
                .Select(id => new RolePermissionAssignmentDto { PermissionId = id, Scope = "All" })
                .ToList();
            return await SaveRolePermissionsWithScopeAsync(roleId, defaultAssignments, "Role permissions updated via standard API");
        }

        public async Task<bool> SaveRolePermissionsWithScopeAsync(long roleId, List<RolePermissionAssignmentDto> permissions, string reason)
        {
            if (roleId <= 0)
            {
                throw new ArgumentException("Valid RoleId is required.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Reason is mandatory for modifying role permissions.");
            }

            try
            {
                var role = await _unitOfWork.RoleMaster.GetById(roleId);
                if (role == null) return false;

                var existingMappings = (await _unitOfWork.RolePermissionMappings.GetAll())
                    .Where(m => m.RoleId == roleId)
                    .ToList();

                var beforeSnapshot = existingMappings
                    .Where(m => m.IsActive && !m.IsDeleted)
                    .Select(m => new { m.PermissionId, m.Scope })
                    .ToList();

                var targetDict = (permissions ?? new List<RolePermissionAssignmentDto>())
                    .GroupBy(p => p.PermissionId)
                    .ToDictionary(g => g.Key, g => string.IsNullOrWhiteSpace(g.First().Scope) ? "All" : g.First().Scope!.Trim());

                // Remove or deactivate unselected mappings
                foreach (var existing in existingMappings)
                {
                    if (!targetDict.ContainsKey(existing.PermissionId))
                    {
                        _unitOfWork.RolePermissionMappings.Delete(existing);
                    }
                }

                // Add or update selected mappings
                foreach (var (permId, scope) in targetDict)
                {
                    var existing = existingMappings.FirstOrDefault(m => m.PermissionId == permId);
                    if (existing != null)
                    {
                        existing.Scope = scope;
                        existing.IsActive = true;
                        existing.IsDeleted = false;
                        existing.UpdatedDate = DateTime.UtcNow;
                        _unitOfWork.RolePermissionMappings.Update(existing);
                    }
                    else
                    {
                        await _unitOfWork.RolePermissionMappings.Add(new RolePermissionMapping
                        {
                            RoleId = roleId,
                            PermissionId = permId,
                            Scope = scope,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        });
                    }
                }

                _unitOfWork.Save();

                // Audit logging
                await _auditService.LogAsync(
                    moduleName: "Security & Governance",
                    entityName: "RolePermissionMapping",
                    entityId: roleId.ToString(),
                    action: "UPDATE_PERMISSIONS",
                    beforeValues: beforeSnapshot,
                    afterValues: targetDict.Select(kvp => new { PermissionId = kvp.Key, Scope = kvp.Value }).ToList(),
                    reason: reason
                );

                // Invalidate permission caches
                InvalidateCache();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving permissions with scope for role {RoleId}", roleId);
                throw;
            }
        }

        public async Task<List<string>> GetUserEffectivePermissionsAsync(long employeeId)
        {
            if (employeeId <= 0) return new List<string>();

            string cacheKey = $"UserPermissions_{employeeId}";
            if (_memoryCache.TryGetValue(cacheKey, out List<string>? cached) && cached != null)
            {
                return cached;
            }

            // Fetch user's assigned roles
            var roleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => m.EmployeeId == employeeId && m.IsActive && !m.IsDeleted)
                .Select(m => m.RoleId)
                .ToList();

            var allRoles = (await _unitOfWork.RoleMaster.GetAll()).ToList();

            // Fallback: If no explicit roles are assigned, grant default Employee role
            if (!roleMappings.Any())
            {
                var defaultEmpRole = allRoles.FirstOrDefault(r => string.Equals(r.RoleName, "Employee", StringComparison.OrdinalIgnoreCase));
                if (defaultEmpRole != null)
                {
                    roleMappings.Add(defaultEmpRole.Id);
                }
            }

            var userRoles = allRoles.Where(r => roleMappings.Contains(r.Id)).Select(r => r.RoleName).ToList();

            var allPermissions = (await GetAllPermissionsAsync()).ToList();

            // Super Admin automatically gets all permissions
            if (userRoles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(r, "System Admin", StringComparison.OrdinalIgnoreCase)))
            {
                var allCodes = allPermissions.Select(p => p.PermissionCode).ToList();
                _memoryCache.Set(cacheKey, allCodes, TimeSpan.FromMinutes(5));
                return allCodes;
            }

            // Permissions via assigned roles
            var rolePermMappings = (await _unitOfWork.RolePermissionMappings.GetAll())
                .Where(m => roleMappings.Contains(m.RoleId) && m.IsActive && !m.IsDeleted)
                .Select(m => m.PermissionId)
                .Distinct()
                .ToHashSet();

            // Direct user grants
            var userDirectGrants = (await _unitOfWork.UserPermissionMappings.GetAll())
                .Where(m => m.EmployeeId == employeeId && m.IsGranted && m.IsActive && !m.IsDeleted)
                .Select(m => m.PermissionId)
                .Distinct()
                .ToHashSet();

            var effectivePermissionIds = rolePermMappings.Union(userDirectGrants).ToHashSet();

            var result = allPermissions
                .Where(p => effectivePermissionIds.Contains(p.Id))
                .Select(p => p.PermissionCode)
                .Distinct()
                .ToList();

            _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }

        public async Task<string> GetEffectiveScopeAsync(long employeeId, string permissionCode)
        {
            if (employeeId <= 0 || string.IsNullOrWhiteSpace(permissionCode)) return "None";

            var allRoles = (await _unitOfWork.RoleMaster.GetAll()).ToList();
            var roleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => m.EmployeeId == employeeId && m.IsActive && !m.IsDeleted)
                .Select(m => m.RoleId)
                .ToList();

            if (!roleMappings.Any())
            {
                var defaultEmpRole = allRoles.FirstOrDefault(r => string.Equals(r.RoleName, "Employee", StringComparison.OrdinalIgnoreCase));
                if (defaultEmpRole != null)
                {
                    roleMappings.Add(defaultEmpRole.Id);
                }
            }

            var userRoles = allRoles.Where(r => roleMappings.Contains(r.Id)).Select(r => r.RoleName).ToList();

            // Super Admin automatically gets "All" scope
            if (userRoles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(r, "System Admin", StringComparison.OrdinalIgnoreCase)))
            {
                return "All";
            }

            var allPerms = (await GetAllPermissionsAsync()).ToList();
            var targetPerm = allPerms.FirstOrDefault(p => string.Equals(p.PermissionCode, permissionCode, StringComparison.OrdinalIgnoreCase));
            if (targetPerm == null) return "None";

            // Role scopes for this permission
            var roleScopes = (await _unitOfWork.RolePermissionMappings.GetAll())
                .Where(m => roleMappings.Contains(m.RoleId) && m.PermissionId == targetPerm.Id && m.IsActive && !m.IsDeleted)
                .Select(m => m.Scope ?? "All")
                .ToList();

            // Direct user scopes
            var userScopes = (await _unitOfWork.UserPermissionMappings.GetAll())
                .Where(m => m.EmployeeId == employeeId && m.PermissionId == targetPerm.Id && m.IsGranted && m.IsActive && !m.IsDeleted)
                .Select(m => m.Scope ?? "All")
                .ToList();

            var allScopes = roleScopes.Concat(userScopes).ToList();
            if (!allScopes.Any())
            {
                return "None";
            }

            // Strict Hierarchy: Widest scope wins: Self < Team < TeamAndIndirect < Department < Branch < All
            int GetScopeRank(string s) => s?.Trim().ToLowerInvariant() switch
            {
                "all" => 6,
                "branch" => 5,
                "department" => 4,
                "teamandindirect" => 3,
                "team" => 2,
                "self" => 1,
                _ => 0
            };

            var bestScope = allScopes.OrderByDescending(GetScopeRank).First();
            return bestScope;
        }

        public async Task<bool> HasPermissionAsync(long employeeId, string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode)) return true;
            var permissions = await GetUserEffectivePermissionsAsync(employeeId);
            return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
        }

        public async Task SeedDefaultPermissionsAsync()
        {
            try
            {
                var existing = (await _unitOfWork.PermissionMaster.GetAll()).ToList();
                var existingCodes = existing.Select(p => p.PermissionCode).ToHashSet(StringComparer.OrdinalIgnoreCase);

                var defaultPerms = new List<PermissionMaster>
                {
                    // Attendance Module
                    new() { PermissionCode = "ATTENDANCE_VIEW_SELF", ModuleName = "Attendance", DisplayName = "View Own Attendance", Description = "View personal punch history and attendance calendar", SortOrder = 1, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_VIEW_ALL", ModuleName = "Attendance", DisplayName = "View Workforce Attendance", Description = "Audit all employee clock records, break logs, and status", SortOrder = 2, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_PUNCH", ModuleName = "Attendance", DisplayName = "Clock In / Clock Out", Description = "Record personal punches and break periods", SortOrder = 3, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_REGULARIZE_APPLY", ModuleName = "Attendance", DisplayName = "Apply Regularization", Description = "Submit attendance adjustment requests for missed punches", SortOrder = 4, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_REGULARIZE_APPROVE", ModuleName = "Attendance", DisplayName = "Approve Regularizations", Description = "Review and approve/reject team regularization requests", SortOrder = 5, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_LOCK", ModuleName = "Attendance", DisplayName = "Lock / Unlock Attendance", Description = "Freeze or unfreeze monthly workforce attendance records for payroll processing", SortOrder = 6, IsActive = true, IsSensitive = true },

                    // Leave Management Module
                    new() { PermissionCode = "LEAVE_APPLY_SELF", ModuleName = "Leave Management", DisplayName = "Apply for Leave", Description = "Submit personal leave requests and view balances", SortOrder = 10, IsActive = true },
                    new() { PermissionCode = "LEAVE_VIEW_ALL", ModuleName = "Leave Management", DisplayName = "View Workforce Leaves", Description = "View team or organization-wide leave applications", SortOrder = 11, IsActive = true },
                    new() { PermissionCode = "LEAVE_APPROVE", ModuleName = "Leave Management", DisplayName = "Approve / Reject Leaves", Description = "Authority to approve or reject employee leave applications", SortOrder = 12, IsActive = true },
                    new() { PermissionCode = "LEAVE_CONFIG_TYPES", ModuleName = "Leave Management", DisplayName = "Configure Leave Policies", Description = "Create leave types, annual quotas, and carry-forward rules", SortOrder = 13, IsActive = true },
                    new() { PermissionCode = "LEAVE_POLICY_MANAGE", ModuleName = "Leave Management", DisplayName = "Manage Leave Policies & Accrual", Description = "Configure sandwich rules, negative balances, trigger accruals and rollovers", SortOrder = 14, IsActive = true },
                    new() { PermissionCode = "LEAVE_ENCASH_APPLY", ModuleName = "Leave Management", DisplayName = "Apply for Leave Encashment", Description = "Submit requests to convert eligible leave balances to monetary payout", SortOrder = 15, IsActive = true },
                    new() { PermissionCode = "LEAVE_ENCASH_APPROVE", ModuleName = "Leave Management", DisplayName = "Approve Leave Encashment", Description = "Review and approve leave encashment payouts for payroll inclusion", SortOrder = 16, IsActive = true },
                    new() { PermissionCode = "COMP_OFF_APPLY", ModuleName = "Leave Management", DisplayName = "Apply for Comp-Off Grant", Description = "Request compensatory off credits for working on weekends or holidays", SortOrder = 17, IsActive = true },
                    new() { PermissionCode = "COMP_OFF_APPROVE", ModuleName = "Leave Management", DisplayName = "Approve Comp-Off Grants", Description = "Review and approve compensatory off credit grants", SortOrder = 18, IsActive = true },
                    new() { PermissionCode = "LOP_CONVERT_APPLY", ModuleName = "Leave Management", DisplayName = "Apply for LOP Conversion", Description = "Request conversion of past approved LOP days to paid leave", SortOrder = 19, IsActive = true },
                    new() { PermissionCode = "LOP_CONVERT_APPROVE", ModuleName = "Leave Management", DisplayName = "Approve LOP Conversion", Description = "Review and approve conversion of LOP days to paid leave", SortOrder = 20, IsActive = true },

                    // Payroll Module (Sensitive permissions marked)
                    new() { PermissionCode = "PAYROLL_VIEW_SELF", ModuleName = "Payroll", DisplayName = "View Personal Payslips", Description = "Download monthly salary slips and tax summaries", SortOrder = 20, IsActive = true },
                    new() { PermissionCode = "PAYROLL_VIEW_ALL", ModuleName = "Payroll", DisplayName = "View Company Payroll", Description = "Access salary register, CTC breakdowns, and disbursements", SortOrder = 21, IsActive = true, IsSensitive = true },
                    new() { PermissionCode = "PAYROLL_PROCESS", ModuleName = "Payroll", DisplayName = "Process Monthly Salary", Description = "Calculate LOP deductions, finalize payroll batches, and mark credited", SortOrder = 22, IsActive = true },
                    new() { PermissionCode = "PAYROLL_CONFIG_RULES", ModuleName = "Payroll", DisplayName = "Configure Payroll Formulas", Description = "Define LOP deduction basis, allowance brackets, and pay cycles", SortOrder = 23, IsActive = true },
                    new() { PermissionCode = "SALARY_VIEW_ALL", ModuleName = "Payroll", DisplayName = "View Unmasked Compensation", Description = "Access unmasked employee CTC, salary structure, and bank account details", SortOrder = 24, IsActive = true, IsSensitive = true },

                    // Timesheet Module
                    new() { PermissionCode = "TIMESHEET_LOG_SELF", ModuleName = "Timesheets", DisplayName = "Submit Timesheets", Description = "Log project work hours against shifts and tasks", SortOrder = 30, IsActive = true },
                    new() { PermissionCode = "TIMESHEET_VIEW_ALL", ModuleName = "Timesheets", DisplayName = "View Workforce Timesheets", Description = "Audit billable hours, project timesheet logs, and utilization", SortOrder = 31, IsActive = true },
                    new() { PermissionCode = "TIMESHEET_APPROVE", ModuleName = "Timesheets", DisplayName = "Approve Timesheets", Description = "Approve or reject weekly/monthly project timesheets", SortOrder = 32, IsActive = true },

                    // Workforce & Employees Module
                    new() { PermissionCode = "EMPLOYEE_VIEW_DIRECTORY", ModuleName = "Workforce", DisplayName = "View Workforce Directory", Description = "Browse organization employee profiles and contact details", SortOrder = 40, IsActive = true },
                    new() { PermissionCode = "EMPLOYEE_MANAGE", ModuleName = "Workforce", DisplayName = "Manage Employee Profiles", Description = "Create, edit, onboard, and offboard employee profiles", SortOrder = 41, IsActive = true },
                    new() { PermissionCode = "EMPLOYEE_DOCUMENTS_MANAGE", ModuleName = "Workforce", DisplayName = "Verify Documents & KYC", Description = "Upload and verify government IDs, degrees, and certificates", SortOrder = 42, IsActive = true },
                    new() { PermissionCode = "EMPLOYEE_PAYMENT_MANAGE", ModuleName = "Workforce", DisplayName = "Manage Bank & Payment Info", Description = "Update employee bank account, IFSC, and payment profiles", SortOrder = 43, IsActive = true, IsSensitive = true },
                    new() { PermissionCode = "RESIGNATION_MANAGE", ModuleName = "Workforce", DisplayName = "Manage Resignations & FnF", Description = "Process employee resignation requests and full & final settlements", SortOrder = 44, IsActive = true, IsSensitive = true },
                    new() { PermissionCode = "EXIT_CLEAR_HR", ModuleName = "Workforce", DisplayName = "Clear HR Exit Tasks", Description = "Authority to inspect and clear HR-specific exit checklist tasks", SortOrder = 45, IsActive = true },
                    new() { PermissionCode = "EXIT_CLEAR_IT", ModuleName = "Workforce", DisplayName = "Clear IT Exit Tasks", Description = "Authority to inspect asset return and clear IT exit checklist tasks", SortOrder = 46, IsActive = true },
                    new() { PermissionCode = "EXIT_CLEAR_FINANCE", ModuleName = "Workforce", DisplayName = "Clear Finance Exit Tasks", Description = "Authority to audit dues, claims, and clear Finance exit checklist tasks", SortOrder = 47, IsActive = true },
                    new() { PermissionCode = "EXIT_CLEAR_FACILITIES", ModuleName = "Workforce", DisplayName = "Clear Facilities Exit Tasks", Description = "Authority to verify physical access, keycards, and clear Facilities tasks", SortOrder = 48, IsActive = true },
                    new() { PermissionCode = "EXIT_TASK_WAIVE", ModuleName = "Workforce", DisplayName = "Waive Exit Checklist Tasks", Description = "Privileged authority to waive mandatory or optional exit tasks with justification", SortOrder = 49, IsActive = true, IsSensitive = true },
                    new() { PermissionCode = "NOTICE_OVERRIDE", ModuleName = "Workforce", DisplayName = "Override Notice Period", Description = "Privileged authority to configure custom notice period days on employee contracts", SortOrder = 49, IsActive = true, IsSensitive = true },

                    // Security & Role Governance Module (Sensitive permissions marked)
                    new() { PermissionCode = "ROLE_VIEW", ModuleName = "Security & Governance", DisplayName = "View Security Roles", Description = "View security tiers, active roles, and personnel mapping", SortOrder = 50, IsActive = true },
                    new() { PermissionCode = "ROLE_MANAGE", ModuleName = "Security & Governance", DisplayName = "Configure Roles & Permissions", Description = "Create roles and toggle permission matrix access rights", SortOrder = 51, IsActive = true },
                    new() { PermissionCode = "ROLE_ASSIGN", ModuleName = "Security & Governance", DisplayName = "Assign Roles to Workforce", Description = "Map employees to security roles and access tiers", SortOrder = 52, IsActive = true },
                    new() { PermissionCode = "AUDIT_VIEW", ModuleName = "Security & Governance", DisplayName = "View System Audit Logs", Description = "Access administrative audit trails, security changes, and IP logs", SortOrder = 53, IsActive = true, IsSensitive = true },

                    // Assets Module
                    new() { PermissionCode = "ASSET_REQUEST_SELF", ModuleName = "Assets & Equipment", DisplayName = "Request Office Equipment", Description = "Submit asset procurement or replacement requests", SortOrder = 60, IsActive = true },
                    new() { PermissionCode = "ASSET_VIEW_ALL", ModuleName = "Assets & Equipment", DisplayName = "View Company Inventory", Description = "View office assets, serial numbers, and equipment inventory", SortOrder = 61, IsActive = true },
                    new() { PermissionCode = "ASSET_APPROVE", ModuleName = "Assets & Equipment", DisplayName = "Allocate & Approve Assets", Description = "Approve hardware/laptop requisitions and assign inventory", SortOrder = 62, IsActive = true },
                    new() { PermissionCode = "ASSET_MANAGE", ModuleName = "Assets & Equipment", DisplayName = "Manage Asset Inventory", Description = "Register new hardware, track depreciations, and maintenance", SortOrder = 63, IsActive = true },

                    // Operations & Settings Module
                    new() { PermissionCode = "SHIFT_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Shifts & Schedules", Description = "Configure day/night shifts, timings, and grace windows", SortOrder = 70, IsActive = true },
                    new() { PermissionCode = "SHIFT_ASSIGN", ModuleName = "System Operations", DisplayName = "Assign Shifts & Rosters", Description = "Assign shift schedules and week-off patterns to employees", SortOrder = 71, IsActive = true },
                    new() { PermissionCode = "HOLIDAY_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Holiday Calendar", Description = "Configure national, regional, and company holidays", SortOrder = 72, IsActive = true },
                    new() { PermissionCode = "EMAIL_TEMPLATE_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Email Templates", Description = "Configure notification triggers and automated email templates", SortOrder = 72, IsActive = true },
                    new() { PermissionCode = "DESIGNATION_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Designations", Description = "Create, update, or delete job designations", SortOrder = 73, IsActive = true },
                    new() { PermissionCode = "DEPARTMENT_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Departments", Description = "Create, update, or delete departments and assign department heads", SortOrder = 73, IsActive = true },
                    new() { PermissionCode = "SKILL_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Skills Library", Description = "Create and manage skill categories and proficiency levels", SortOrder = 74, IsActive = true },
                    new() { PermissionCode = "PROJECT_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Projects", Description = "Create and manage project master records", SortOrder = 75, IsActive = true },

                    // Expense Management Module
                    new() { PermissionCode = "EXPENSE_VIEW_SELF", ModuleName = "Expense Management", DisplayName = "Submit Expense Claims", Description = "Submit and track personal expense reimbursements", SortOrder = 80, IsActive = true },
                    new() { PermissionCode = "EXPENSE_VIEW_ALL", ModuleName = "Expense Management", DisplayName = "View All Expense Claims", Description = "View all employee expense claim submissions", SortOrder = 81, IsActive = true },
                    new() { PermissionCode = "EXPENSE_APPROVE", ModuleName = "Expense Management", DisplayName = "Approve Expense Claims", Description = "Approve or reject employee expense reimbursement requests", SortOrder = 82, IsActive = true },
                    new() { PermissionCode = "EXPENSE_MANAGE", ModuleName = "Expense Management", DisplayName = "Manage Expense Policies", Description = "Configure expense categories and reimbursement policies", SortOrder = 83, IsActive = true },

                    // Recruitment Module
                    new() { PermissionCode = "RECRUITMENT_VIEW", ModuleName = "Recruitment", DisplayName = "View Recruitment Pipeline", Description = "View job requisitions, candidate pipeline, and interview schedules", SortOrder = 90, IsActive = true },
                    new() { PermissionCode = "RECRUITMENT_MANAGE", ModuleName = "Recruitment", DisplayName = "Manage Recruitment", Description = "Create job requisitions, manage candidates, schedule interviews, issue offers", SortOrder = 91, IsActive = true },

                    // Performance Management
                    new() { PermissionCode = "PMS_VIEW_SELF", ModuleName = "Performance Management", DisplayName = "View Own Reviews", Description = "View personal performance reviews and goals", SortOrder = 100, IsActive = true },
                    new() { PermissionCode = "PMS_MANAGE", ModuleName = "Performance Management", DisplayName = "Manage Performance Reviews", Description = "Create review cycles, assign goals, and finalize ratings", SortOrder = 101, IsActive = true },

                    // Approval Workflow Permissions
                    new() { PermissionCode = "WORKFLOW_CONFIGURE", ModuleName = "System Operations", DisplayName = "Configure Approval Workflows", Description = "Configure workflow definitions, steps, and routing matrix", SortOrder = 110, IsActive = true },
                    new() { PermissionCode = "REQUEST_OVERRIDE", ModuleName = "System Operations", DisplayName = "Override Approval Decisions", Description = "Administrative authority to approve or reject requests outside the standard approver chain", SortOrder = 111, IsActive = true },
                    new() { PermissionCode = "REQUEST_REASSIGN", ModuleName = "System Operations", DisplayName = "Reassign Pending Approvals", Description = "Authority to reassign pending workflow approval steps to other eligible approvers", SortOrder = 112, IsActive = true }
                };

                bool changesMade = false;

                // Sync new or updated permissions
                var missingPerms = defaultPerms.Where(p => !existingCodes.Contains(p.PermissionCode)).ToList();
                foreach (var p in missingPerms)
                {
                    p.CreatedDate = DateTime.UtcNow;
                    p.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.PermissionMaster.Add(p);
                    changesMade = true;
                }

                // Update IsSensitive on existing permissions if mismatch
                foreach (var ex in existing)
                {
                    var def = defaultPerms.FirstOrDefault(d => string.Equals(d.PermissionCode, ex.PermissionCode, StringComparison.OrdinalIgnoreCase));
                    if (def != null && ex.IsSensitive != def.IsSensitive)
                    {
                        ex.IsSensitive = def.IsSensitive;
                        _unitOfWork.PermissionMaster.Update(ex);
                        changesMade = true;
                    }
                }

                if (changesMade)
                {
                    _unitOfWork.Save();
                }

                // Seed Default Role Mappings for base roles
                var savedPerms = (await _unitOfWork.PermissionMaster.GetAll()).ToList();
                var roles = (await _unitOfWork.RoleMaster.GetAll()).ToList();
                var existingRolePerms = (await _unitOfWork.RolePermissionMappings.GetAll()).ToList();

                var empPermCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "ATTENDANCE_VIEW_SELF", "ATTENDANCE_PUNCH", "ATTENDANCE_REGULARIZE_APPLY",
                    "LEAVE_APPLY_SELF", "LEAVE_ENCASH_APPLY", "COMP_OFF_APPLY", "LOP_CONVERT_APPLY",
                    "PAYROLL_VIEW_SELF", "TIMESHEET_LOG_SELF",
                    "EMPLOYEE_VIEW_DIRECTORY", "ASSET_REQUEST_SELF", "EXPENSE_VIEW_SELF", "PMS_VIEW_SELF"
                };

                var mgrPermCodes = new HashSet<string>(empPermCodes, StringComparer.OrdinalIgnoreCase)
                {
                    "ATTENDANCE_VIEW_ALL", "ATTENDANCE_REGULARIZE_APPROVE",
                    "LEAVE_VIEW_ALL", "LEAVE_APPROVE", "COMP_OFF_APPROVE", "LOP_CONVERT_APPROVE",
                    "TIMESHEET_VIEW_ALL", "TIMESHEET_APPROVE",
                    "ASSET_APPROVE", "EXPENSE_APPROVE",
                    "RECRUITMENT_VIEW", "PMS_MANAGE", "RESIGNATION_MANAGE", "SHIFT_MANAGE"
                };

                var hrPermCodes = new HashSet<string>(empPermCodes, StringComparer.OrdinalIgnoreCase)
                {
                    "ATTENDANCE_VIEW_ALL", "ATTENDANCE_REGULARIZE_APPROVE",
                    "LEAVE_VIEW_ALL", "LEAVE_APPROVE", "LEAVE_CONFIG_TYPES", "LEAVE_POLICY_MANAGE",
                    "LEAVE_ENCASH_APPROVE", "COMP_OFF_APPROVE", "LOP_CONVERT_APPROVE",
                    "PAYROLL_VIEW_ALL", "PAYROLL_PROCESS", "PAYROLL_CONFIG_RULES",
                    "TIMESHEET_VIEW_ALL", "TIMESHEET_APPROVE",
                    "EMPLOYEE_MANAGE", "EMPLOYEE_DOCUMENTS_MANAGE", "EMPLOYEE_PAYMENT_MANAGE",
                    "ROLE_VIEW", "HOLIDAY_MANAGE", "EMAIL_TEMPLATE_MANAGE",
                    "ASSET_VIEW_ALL", "ASSET_APPROVE", "ASSET_MANAGE",
                    "SHIFT_MANAGE", "SHIFT_ASSIGN",
                    "EXPENSE_VIEW_ALL", "EXPENSE_APPROVE", "EXPENSE_MANAGE",
                    "RECRUITMENT_VIEW", "RECRUITMENT_MANAGE",
                    "PMS_MANAGE",
                    "DESIGNATION_MANAGE", "DEPARTMENT_MANAGE", "SKILL_MANAGE", "PROJECT_MANAGE", "RESIGNATION_MANAGE",
                    "EXIT_CLEAR_HR", "EXIT_CLEAR_IT", "EXIT_CLEAR_FINANCE", "EXIT_CLEAR_FACILITIES",
                    "EXIT_TASK_WAIVE", "NOTICE_OVERRIDE", "ATTENDANCE_LOCK"
                };

                var adminPermCodes = savedPerms.Select(p => p.PermissionCode).ToHashSet(StringComparer.OrdinalIgnoreCase);

                var hrRoles = roles.Where(r => string.Equals(r.RoleName, "HR", StringComparison.OrdinalIgnoreCase) || string.Equals(r.RoleName, "HR Operations", StringComparison.OrdinalIgnoreCase)).ToList();
                var mgrRoles = roles.Where(r => string.Equals(r.RoleName, "Manager", StringComparison.OrdinalIgnoreCase) || string.Equals(r.RoleName, "Management", StringComparison.OrdinalIgnoreCase)).ToList();
                var empRoles = roles.Where(r => string.Equals(r.RoleName, "Employee", StringComparison.OrdinalIgnoreCase)).ToList();
                var adminRoles = roles.Where(r => string.Equals(r.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) || string.Equals(r.RoleName, "System Admin", StringComparison.OrdinalIgnoreCase)).ToList();

                bool roleChangesMade = false;

                void SyncRolePerms(RoleMaster role, HashSet<string> requiredCodes, string defaultScope)
                {
                    var currentRoleMappings = existingRolePerms
                        .Where(m => m.RoleId == role.Id && m.IsActive && !m.IsDeleted)
                        .Select(m => m.PermissionId)
                        .ToHashSet();

                    var targetPerms = savedPerms.Where(p => requiredCodes.Contains(p.PermissionCode));
                    foreach (var p in targetPerms)
                    {
                        if (!currentRoleMappings.Contains(p.Id))
                        {
                            _unitOfWork.RolePermissionMappings.Add(new RolePermissionMapping
                            {
                                RoleId = role.Id,
                                PermissionId = p.Id,
                                Scope = defaultScope,
                                IsActive = true,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedDate = DateTime.UtcNow
                            });
                            roleChangesMade = true;
                        }
                    }
                }

                foreach (var r in empRoles) SyncRolePerms(r, empPermCodes, "Self");
                foreach (var r in mgrRoles) SyncRolePerms(r, mgrPermCodes, "TeamAndIndirect");
                foreach (var r in hrRoles) SyncRolePerms(r, hrPermCodes, "All");
                foreach (var r in adminRoles) SyncRolePerms(r, adminPermCodes, "All");

                if (roleChangesMade)
                {
                    _unitOfWork.Save();
                }

                Log.Information("Successfully reconciled default HRMS permissions and role mappings.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to seed default permissions");
            }
        }

        private void InvalidateCache()
        {
            if (_memoryCache is MemoryCache memCache)
            {
                memCache.Compact(1.0); // clears all cached user permissions
            }
        }
    }
}
