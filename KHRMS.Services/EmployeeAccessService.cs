using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Services.Interfaces;

namespace KHRMS.Services
{
    public class EmployeeAccessService : IEmployeeAccessService
    {
        private readonly IUserContextService _userContextService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPermissionService _permissionService;

        public EmployeeAccessService(
            IUserContextService userContextService,
            IUnitOfWork unitOfWork,
            IPermissionService permissionService)
        {
            _userContextService = userContextService;
            _unitOfWork = unitOfWork;
            _permissionService = permissionService;
        }

        public long GetCurrentEmployeeId()
        {
            return _userContextService.GetCurrentEmployeeId();
        }

        public async Task<string> GetEffectiveScopeForPermissionAsync(string permissionCode)
        {
            var currentEmpId = GetCurrentEmployeeId();
            if (currentEmpId <= 0 || string.IsNullOrWhiteSpace(permissionCode)) return "None";

            if (_userContextService.IsAdmin())
            {
                return "All";
            }

            return await _permissionService.GetEffectiveScopeAsync(currentEmpId, permissionCode);
        }

        public async Task<bool> CanAccessEmployeeWithScopeAsync(long targetEmployeeId, string permissionCode)
        {
            var currentEmpId = GetCurrentEmployeeId();
            if (currentEmpId <= 0 || targetEmployeeId <= 0)
            {
                return false;
            }

            // Viewing self is always permitted unless user has no account
            if (currentEmpId == targetEmployeeId)
            {
                return true;
            }

            if (_userContextService.IsAdmin())
            {
                return true;
            }

            var scope = await _permissionService.GetEffectiveScopeAsync(currentEmpId, permissionCode);
            if (string.Equals(scope, "None", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.Equals(scope, "All", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => !e.IsDeleted).ToList();
            var currentEmp = allEmployees.FirstOrDefault(e => e.Id == currentEmpId);
            var targetEmp = allEmployees.FirstOrDefault(e => e.Id == targetEmployeeId);

            if (currentEmp == null || targetEmp == null)
            {
                return false;
            }

            switch (scope.ToLowerInvariant())
            {
                case "branch":
                    return !string.IsNullOrWhiteSpace(currentEmp.Branch) &&
                           string.Equals(currentEmp.Branch, targetEmp.Branch, StringComparison.OrdinalIgnoreCase);

                case "department":
                    // If Department column is added later, check Department. For now, match DesignationId or Branch
                    return (currentEmp.DesignationId.HasValue && targetEmp.DesignationId.HasValue &&
                            currentEmp.DesignationId == targetEmp.DesignationId) ||
                           await IsInReportingHierarchyAsync(currentEmpId, targetEmployeeId);

                case "teamandindirect":
                    return await IsInReportingHierarchyAsync(currentEmpId, targetEmployeeId);

                case "team":
                    return targetEmp.ManagerId == currentEmpId;

                case "self":
                    return targetEmployeeId == currentEmpId;

                default:
                    return false;
            }
        }

        public async Task<List<long>> GetInScopeEmployeeIdsAsync(string permissionCode)
        {
            var currentEmpId = GetCurrentEmployeeId();
            if (currentEmpId <= 0) return new List<long>();

            if (_userContextService.IsAdmin())
            {
                var all = await _unitOfWork.Employees.GetAll();
                return all.Where(e => !e.IsDeleted).Select(e => e.Id).ToList();
            }

            var scope = await _permissionService.GetEffectiveScopeAsync(currentEmpId, permissionCode);
            if (string.Equals(scope, "None", StringComparison.OrdinalIgnoreCase))
            {
                return new List<long>();
            }

            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => !e.IsDeleted).ToList();
            var currentEmp = allEmployees.FirstOrDefault(e => e.Id == currentEmpId);
            if (currentEmp == null) return new List<long> { currentEmpId };

            switch (scope.ToLowerInvariant())
            {
                case "all":
                    return allEmployees.Select(e => e.Id).ToList();

                case "branch":
                    if (string.IsNullOrWhiteSpace(currentEmp.Branch))
                        return new List<long> { currentEmpId };
                    return allEmployees
                        .Where(e => string.Equals(e.Branch, currentEmp.Branch, StringComparison.OrdinalIgnoreCase))
                        .Select(e => e.Id)
                        .ToList();

                case "department":
                    return allEmployees
                        .Where(e => e.Id == currentEmpId ||
                                   (currentEmp.DesignationId.HasValue && e.DesignationId == currentEmp.DesignationId))
                        .Select(e => e.Id)
                        .ToList();

                case "teamandindirect":
                    var indirectReports = await GetSubordinateEmployeeIdsAsync(currentEmpId);
                    indirectReports.Add(currentEmpId);
                    return indirectReports.Distinct().ToList();

                case "team":
                    var directReports = allEmployees.Where(e => e.ManagerId == currentEmpId).Select(e => e.Id).ToList();
                    directReports.Add(currentEmpId);
                    return directReports.Distinct().ToList();

                case "self":
                default:
                    return new List<long> { currentEmpId };
            }
        }

        public async Task<bool> CanAccessEmployeeAsync(long targetEmployeeId, string? privilegedPermissionCode = null)
        {
            var currentEmpId = GetCurrentEmployeeId();
            if (currentEmpId <= 0)
            {
                return false;
            }

            // Rule 1: Self
            if (targetEmployeeId > 0 && currentEmpId == targetEmployeeId)
            {
                return true;
            }

            // Global admin / HR checks
            if (_userContextService.IsAdmin() || _userContextService.IsHR())
            {
                return true;
            }

            // Rule 2: Privileged permission check
            if (!string.IsNullOrWhiteSpace(privilegedPermissionCode))
            {
                if (await _userContextService.HasPermissionAsync(privilegedPermissionCode))
                {
                    return true;
                }
            }

            // Global admin permissions
            if (await _userContextService.HasPermissionAsync("EMPLOYEE_MANAGE") ||
                await _userContextService.HasPermissionAsync("ADMIN"))
            {
                return true;
            }

            if (targetEmployeeId <= 0)
            {
                return false;
            }

            // Rule 3: Team Scope — reporting hierarchy
            return await IsInReportingHierarchyAsync(currentEmpId, targetEmployeeId);
        }

        public async Task<bool> IsInReportingHierarchyAsync(long managerEmployeeId, long targetEmployeeId)
        {
            if (managerEmployeeId <= 0 || targetEmployeeId <= 0 || managerEmployeeId == targetEmployeeId)
            {
                return false;
            }

            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();
            var target = allEmployees.FirstOrDefault(e => e.Id == targetEmployeeId && !e.IsDeleted);
            if (target == null)
            {
                return false;
            }

            var visited = new HashSet<long> { targetEmployeeId };
            long? currentManagerId = target.ManagerId;

            while (currentManagerId.HasValue && currentManagerId.Value > 0)
            {
                if (currentManagerId.Value == managerEmployeeId)
                {
                    return true; // Target reports (directly or indirectly) to manager
                }

                if (!visited.Add(currentManagerId.Value))
                {
                    break; // Circular reference loop guard
                }

                var nextManager = allEmployees.FirstOrDefault(e => e.Id == currentManagerId.Value && !e.IsDeleted);
                currentManagerId = nextManager?.ManagerId;
            }

            return false;
        }

        public async Task<List<long>> GetSubordinateEmployeeIdsAsync(long managerEmployeeId)
        {
            if (managerEmployeeId <= 0) return new List<long>();

            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => !e.IsDeleted).ToList();
            var subordinates = new HashSet<long>();
            var queue = new Queue<long>();
            queue.Enqueue(managerEmployeeId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var directReports = allEmployees.Where(e => e.ManagerId == currentId).Select(e => e.Id).ToList();
                foreach (var reportId in directReports)
                {
                    if (subordinates.Add(reportId))
                    {
                        queue.Enqueue(reportId);
                    }
                }
            }

            return subordinates.ToList();
        }
    }
}
