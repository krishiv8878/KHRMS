using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using System.Security.Claims;

namespace KHRMS.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _memoryCache;

        public PermissionService(IUnitOfWork unitOfWork, IMemoryCache memoryCache)
        {
            _unitOfWork = unitOfWork;
            _memoryCache = memoryCache;
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

        public async Task<bool> SaveRolePermissionsAsync(long roleId, List<long> permissionIds)
        {
            try
            {
                var role = await _unitOfWork.RoleMaster.GetById(roleId);
                if (role == null) return false;

                var existingMappings = (await _unitOfWork.RolePermissionMappings.GetAll())
                    .Where(m => m.RoleId == roleId)
                    .ToList();

                // Remove unselected mappings
                foreach (var existing in existingMappings)
                {
                    if (!permissionIds.Contains(existing.PermissionId))
                    {
                        _unitOfWork.RolePermissionMappings.Delete(existing);
                    }
                }

                // Add newly selected mappings
                var existingPermIds = existingMappings.Select(m => m.PermissionId).ToHashSet();
                foreach (var permId in permissionIds)
                {
                    if (!existingPermIds.Contains(permId))
                    {
                        await _unitOfWork.RolePermissionMappings.Add(new RolePermissionMapping
                        {
                            RoleId = roleId,
                            PermissionId = permId,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow
                        });
                    }
                }

                _unitOfWork.Save();

                // Invalidate permission caches
                InvalidateCache();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving permissions for role {RoleId}", roleId);
                return false;
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

            var allRoles = await _unitOfWork.RoleMaster.GetAll();
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
                if (existing.Count > 0) return;

                var defaultPerms = new List<PermissionMaster>
                {
                    // Attendance Module
                    new() { PermissionCode = "ATTENDANCE_VIEW_SELF", ModuleName = "Attendance", DisplayName = "View Own Attendance", Description = "View personal punch history and attendance calendar", SortOrder = 1, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_VIEW_ALL", ModuleName = "Attendance", DisplayName = "View Workforce Attendance", Description = "Audit all employee clock records, break logs, and status", SortOrder = 2, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_PUNCH", ModuleName = "Attendance", DisplayName = "Clock In / Clock Out", Description = "Record personal punches and break periods", SortOrder = 3, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_REGULARIZE_APPLY", ModuleName = "Attendance", DisplayName = "Apply Regularization", Description = "Submit attendance adjustment requests for missed punches", SortOrder = 4, IsActive = true },
                    new() { PermissionCode = "ATTENDANCE_REGULARIZE_APPROVE", ModuleName = "Attendance", DisplayName = "Approve Regularizations", Description = "Review and approve/reject team regularization requests", SortOrder = 5, IsActive = true },

                    // Leave Management Module
                    new() { PermissionCode = "LEAVE_APPLY_SELF", ModuleName = "Leave Management", DisplayName = "Apply for Leave", Description = "Submit personal leave requests and view balances", SortOrder = 10, IsActive = true },
                    new() { PermissionCode = "LEAVE_VIEW_ALL", ModuleName = "Leave Management", DisplayName = "View Workforce Leaves", Description = "View team or organization-wide leave applications", SortOrder = 11, IsActive = true },
                    new() { PermissionCode = "LEAVE_APPROVE", ModuleName = "Leave Management", DisplayName = "Approve / Reject Leaves", Description = "Authority to approve or reject employee leave applications", SortOrder = 12, IsActive = true },
                    new() { PermissionCode = "LEAVE_CONFIG_TYPES", ModuleName = "Leave Management", DisplayName = "Configure Leave Policies", Description = "Create leave types, annual quotas, and carry-forward rules", SortOrder = 13, IsActive = true },

                    // Payroll Module
                    new() { PermissionCode = "PAYROLL_VIEW_SELF", ModuleName = "Payroll", DisplayName = "View Personal Payslips", Description = "Download monthly salary slips and tax summaries", SortOrder = 20, IsActive = true },
                    new() { PermissionCode = "PAYROLL_VIEW_ALL", ModuleName = "Payroll", DisplayName = "View Company Payroll", Description = "Access salary register, CTC breakdowns, and disbursements", SortOrder = 21, IsActive = true },
                    new() { PermissionCode = "PAYROLL_PROCESS", ModuleName = "Payroll", DisplayName = "Process Monthly Salary", Description = "Calculate LOP deductions, finalize payroll batches, and mark credited", SortOrder = 22, IsActive = true },
                    new() { PermissionCode = "PAYROLL_CONFIG_RULES", ModuleName = "Payroll", DisplayName = "Configure Payroll Formulas", Description = "Define LOP deduction basis, allowance brackets, and pay cycles", SortOrder = 23, IsActive = true },

                    // Timesheet Module
                    new() { PermissionCode = "TIMESHEET_LOG_SELF", ModuleName = "Timesheets", DisplayName = "Submit Timesheets", Description = "Log project work hours against shifts and tasks", SortOrder = 30, IsActive = true },
                    new() { PermissionCode = "TIMESHEET_VIEW_ALL", ModuleName = "Timesheets", DisplayName = "View Workforce Timesheets", Description = "Audit billable hours, project timesheet logs, and utilization", SortOrder = 31, IsActive = true },
                    new() { PermissionCode = "TIMESHEET_APPROVE", ModuleName = "Timesheets", DisplayName = "Approve Timesheets", Description = "Approve or reject weekly/monthly project timesheets", SortOrder = 32, IsActive = true },

                    // Workforce & Employees Module
                    new() { PermissionCode = "EMPLOYEE_VIEW_DIRECTORY", ModuleName = "Workforce", DisplayName = "View Workforce Directory", Description = "Browse organization employee profiles and contact details", SortOrder = 40, IsActive = true },
                    new() { PermissionCode = "EMPLOYEE_MANAGE", ModuleName = "Workforce", DisplayName = "Manage Employee Profiles", Description = "Create, edit, onboard, and offboard employee profiles", SortOrder = 41, IsActive = true },
                    new() { PermissionCode = "EMPLOYEE_DOCUMENTS_MANAGE", ModuleName = "Workforce", DisplayName = "Verify Documents & KYC", Description = "Upload and verify government IDs, degrees, and certificates", SortOrder = 42, IsActive = true },
                    new() { PermissionCode = "EMPLOYEE_PAYMENT_MANAGE", ModuleName = "Workforce", DisplayName = "Manage Bank & Payment Info", Description = "Update employee bank account, IFSC, and payment profiles", SortOrder = 43, IsActive = true },

                    // Security & Role Governance Module
                    new() { PermissionCode = "ROLE_VIEW", ModuleName = "Security & Governance", DisplayName = "View Security Roles", Description = "View security tiers, active roles, and personnel mapping", SortOrder = 50, IsActive = true },
                    new() { PermissionCode = "ROLE_MANAGE", ModuleName = "Security & Governance", DisplayName = "Configure Roles & Permissions", Description = "Create roles and toggle permission matrix access rights", SortOrder = 51, IsActive = true },
                    new() { PermissionCode = "ROLE_ASSIGN", ModuleName = "Security & Governance", DisplayName = "Assign Roles to Workforce", Description = "Map employees to security roles and access tiers", SortOrder = 52, IsActive = true },

                    // Assets Module
                    new() { PermissionCode = "ASSET_REQUEST_SELF", ModuleName = "Assets & Equipment", DisplayName = "Request Office Equipment", Description = "Submit asset procurement or replacement requests", SortOrder = 60, IsActive = true },
                    new() { PermissionCode = "ASSET_VIEW_ALL", ModuleName = "Assets & Equipment", DisplayName = "View Company Inventory", Description = "View office assets, serial numbers, and equipment inventory", SortOrder = 61, IsActive = true },
                    new() { PermissionCode = "ASSET_APPROVE", ModuleName = "Assets & Equipment", DisplayName = "Allocate & Approve Assets", Description = "Approve hardware/laptop requisitions and assign inventory", SortOrder = 62, IsActive = true },
                    new() { PermissionCode = "ASSET_MANAGE", ModuleName = "Assets & Equipment", DisplayName = "Manage Asset Inventory", Description = "Register new hardware, track depreciations, and maintenance", SortOrder = 63, IsActive = true },

                    // Operations & Settings Module
                    new() { PermissionCode = "SHIFT_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Shifts & Schedules", Description = "Configure day/night shifts, timings, and grace windows", SortOrder = 70, IsActive = true },
                    new() { PermissionCode = "HOLIDAY_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Holiday Calendar", Description = "Configure national, regional, and company holidays", SortOrder = 71, IsActive = true },
                    new() { PermissionCode = "EMAIL_TEMPLATE_MANAGE", ModuleName = "System Operations", DisplayName = "Manage Email Templates", Description = "Configure notification triggers and automated email templates", SortOrder = 72, IsActive = true }
                };

                foreach (var p in defaultPerms)
                {
                    p.CreatedDate = DateTime.UtcNow;
                    p.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.PermissionMaster.Add(p);
                }

                _unitOfWork.Save();

                // Seed Default Role Mappings for base roles
                var savedPerms = (await _unitOfWork.PermissionMaster.GetAll()).ToList();
                var roles = (await _unitOfWork.RoleMaster.GetAll()).ToList();

                var hrRole = roles.FirstOrDefault(r => string.Equals(r.RoleName, "HR", StringComparison.OrdinalIgnoreCase) || string.Equals(r.RoleName, "HR Operations", StringComparison.OrdinalIgnoreCase));
                var mgrRole = roles.FirstOrDefault(r => string.Equals(r.RoleName, "Manager", StringComparison.OrdinalIgnoreCase) || string.Equals(r.RoleName, "Management", StringComparison.OrdinalIgnoreCase));
                var empRole = roles.FirstOrDefault(r => string.Equals(r.RoleName, "Employee", StringComparison.OrdinalIgnoreCase));

                if (hrRole != null)
                {
                    var hrPermCodes = new[] { "ATTENDANCE_VIEW_ALL", "ATTENDANCE_VIEW_SELF", "ATTENDANCE_PUNCH", "ATTENDANCE_REGULARIZE_APPROVE", "LEAVE_VIEW_ALL", "LEAVE_APPROVE", "LEAVE_CONFIG_TYPES", "PAYROLL_VIEW_ALL", "PAYROLL_PROCESS", "EMPLOYEE_VIEW_DIRECTORY", "EMPLOYEE_MANAGE", "EMPLOYEE_DOCUMENTS_MANAGE", "ROLE_VIEW", "HOLIDAY_MANAGE", "EMAIL_TEMPLATE_MANAGE" };
                    foreach (var p in savedPerms.Where(x => hrPermCodes.Contains(x.PermissionCode)))
                    {
                        await _unitOfWork.RolePermissionMappings.Add(new RolePermissionMapping { RoleId = hrRole.Id, PermissionId = p.Id, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow });
                    }
                }

                if (mgrRole != null)
                {
                    var mgrPermCodes = new[] { "ATTENDANCE_VIEW_ALL", "ATTENDANCE_VIEW_SELF", "ATTENDANCE_PUNCH", "ATTENDANCE_REGULARIZE_APPROVE", "LEAVE_VIEW_ALL", "LEAVE_APPROVE", "TIMESHEET_VIEW_ALL", "TIMESHEET_APPROVE", "EMPLOYEE_VIEW_DIRECTORY", "ASSET_APPROVE" };
                    foreach (var p in savedPerms.Where(x => mgrPermCodes.Contains(x.PermissionCode)))
                    {
                        await _unitOfWork.RolePermissionMappings.Add(new RolePermissionMapping { RoleId = mgrRole.Id, PermissionId = p.Id, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow });
                    }
                }

                if (empRole != null)
                {
                    var empPermCodes = new[] { "ATTENDANCE_VIEW_SELF", "ATTENDANCE_PUNCH", "ATTENDANCE_REGULARIZE_APPLY", "LEAVE_APPLY_SELF", "PAYROLL_VIEW_SELF", "TIMESHEET_LOG_SELF", "EMPLOYEE_VIEW_DIRECTORY", "ASSET_REQUEST_SELF" };
                    foreach (var p in savedPerms.Where(x => empPermCodes.Contains(x.PermissionCode)))
                    {
                        await _unitOfWork.RolePermissionMappings.Add(new RolePermissionMapping { RoleId = empRole.Id, PermissionId = p.Id, IsActive = true, CreatedDate = DateTime.UtcNow, UpdatedDate = DateTime.UtcNow });
                    }
                }

                _unitOfWork.Save();
                Log.Information("Successfully seeded default HRMS permissions and role mappings.");
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
