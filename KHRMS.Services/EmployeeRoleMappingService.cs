using KHRMS.Core;
using KHRMS.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public class EmployeeRoleMappingService : IEmployeeRoleMappingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IUserContextService _userContextService;

        public EmployeeRoleMappingService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _userContextService = userContextService;
        }

        public async Task<bool> CreateEmployeeRoleMapping(EmployeeRoleMapping employeeRoleMapping)
        {
            if (employeeRoleMapping == null || employeeRoleMapping.EmployeeId <= 0 || employeeRoleMapping.RoleId <= 0)
            {
                return false;
            }

            var currentEmpId = _userContextService.GetCurrentEmployeeId();

            // Safeguard 3: Block self-role assignment
            if (currentEmpId > 0 && currentEmpId == employeeRoleMapping.EmployeeId)
            {
                throw new InvalidOperationException("Users cannot modify their own security roles. Role assignments must be performed by another authorized administrator.");
            }

            var employeeroleMapping = new EmployeeRoleMapping
            {
                RoleId = employeeRoleMapping.RoleId,
                EmployeeId = employeeRoleMapping.EmployeeId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
            };

            await _unitOfWork.EmployeeRoleMappings.Add(employeeroleMapping);
            var result = _unitOfWork.Save();

            if (result > 0)
            {
                await _auditService.LogAsync(
                    moduleName: "Security & Governance",
                    entityName: "EmployeeRoleMapping",
                    entityId: employeeroleMapping.EmployeeId.ToString(),
                    action: "ASSIGN_ROLE",
                    beforeValues: null,
                    afterValues: new { employeeroleMapping.RoleId, employeeroleMapping.EmployeeId },
                    reason: "Role assigned to employee"
                );
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteEmployeeRoleMapping(long employeeRoleMappingId)
        {
            if (employeeRoleMappingId <= 0) return false;

            var mapping = await _unitOfWork.EmployeeRoleMappings.GetById(employeeRoleMappingId);
            if (mapping == null || mapping.IsDeleted) return false;

            var currentEmpId = _userContextService.GetCurrentEmployeeId();

            // Safeguard 3: Block self-role modification
            if (currentEmpId > 0 && currentEmpId == mapping.EmployeeId)
            {
                throw new InvalidOperationException("Users cannot modify their own security roles. Role assignments must be performed by another authorized administrator.");
            }

            // Safeguard 1: Block removing last Administrator
            await AssertNotRemovingLastAdminAsync(mapping.EmployeeId, new List<long>());

            var beforeSnapshot = new { mapping.Id, mapping.EmployeeId, mapping.RoleId, mapping.IsActive };

            mapping.IsActive = false;
            mapping.IsDeleted = true;
            mapping.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.EmployeeRoleMappings.Update(mapping);
            var result = _unitOfWork.Save();

            if (result > 0)
            {
                await _auditService.LogAsync(
                    moduleName: "Security & Governance",
                    entityName: "EmployeeRoleMapping",
                    entityId: mapping.EmployeeId.ToString(),
                    action: "REVOKE_ROLE",
                    beforeValues: beforeSnapshot,
                    afterValues: null,
                    reason: "Role revoked from employee"
                );
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<EmployeeRoleMapping>> GetAllEmployeeRoleMapping()
        {
            var mappings = await _unitOfWork.EmployeeRoleMappings.GetAll();
            return mappings.Where(m => !m.IsDeleted).ToList();
        }

        public async Task<EmployeeRoleMapping?> GetRoleMappingById(long employeeRoleMappingId)
        {
            if (employeeRoleMappingId <= 0) return null;
            var mapping = await _unitOfWork.EmployeeRoleMappings.GetById(employeeRoleMappingId);
            return mapping is { IsDeleted: false } ? mapping : null;
        }

        public async Task<bool> UpdateEmployeeRoleMapping(EmployeeRoleMapping employeeRoleMapping)
        {
            if (employeeRoleMapping == null || employeeRoleMapping.Id <= 0) return false;

            var existing = await _unitOfWork.EmployeeRoleMappings.GetById(employeeRoleMapping.Id);
            if (existing == null || existing.IsDeleted) return false;

            var currentEmpId = _userContextService.GetCurrentEmployeeId();

            // Safeguard 3: Block self-role modification
            if (currentEmpId > 0 && currentEmpId == existing.EmployeeId)
            {
                throw new InvalidOperationException("Users cannot modify their own security roles. Role assignments must be performed by another authorized administrator.");
            }

            existing.RoleId = employeeRoleMapping.RoleId;
            existing.EmployeeId = employeeRoleMapping.EmployeeId;
            existing.IsActive = employeeRoleMapping.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.EmployeeRoleMappings.Update(existing);
            return _unitOfWork.Save() > 0;
        }

        public async Task<bool> AssignRolesToEmployeeAsync(long targetEmployeeId, List<long> roleIds, string reason)
        {
            if (targetEmployeeId <= 0)
            {
                throw new ArgumentException("Valid target employee ID is required.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("A reason is mandatory for modifying employee role assignments.");
            }

            var currentEmpId = _userContextService.GetCurrentEmployeeId();

            // Safeguard 3: Block self-role modification
            if (currentEmpId > 0 && currentEmpId == targetEmployeeId)
            {
                throw new InvalidOperationException("Users cannot modify their own security roles. Role assignments must be performed by another authorized administrator.");
            }

            // Safeguard 1: Block removing the last Administrator
            await AssertNotRemovingLastAdminAsync(targetEmployeeId, roleIds);

            var existingMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => m.EmployeeId == targetEmployeeId && !m.IsDeleted)
                .ToList();

            var beforeRoleIds = existingMappings.Where(m => m.IsActive).Select(m => m.RoleId).ToList();
            var targetRoleSet = (roleIds ?? new List<long>()).Distinct().ToHashSet();

            // Deactivate removed mappings
            foreach (var m in existingMappings)
            {
                if (!targetRoleSet.Contains(m.RoleId))
                {
                    m.IsActive = false;
                    m.IsDeleted = true;
                    m.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.EmployeeRoleMappings.Update(m);
                }
            }

            // Add or reactivate target mappings
            foreach (var rId in targetRoleSet)
            {
                var existing = existingMappings.FirstOrDefault(m => m.RoleId == rId);
                if (existing != null)
                {
                    existing.IsActive = true;
                    existing.IsDeleted = false;
                    existing.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.EmployeeRoleMappings.Update(existing);
                }
                else
                {
                    await _unitOfWork.EmployeeRoleMappings.Add(new EmployeeRoleMapping
                    {
                        EmployeeId = targetEmployeeId,
                        RoleId = rId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow
                    });
                }
            }

            _unitOfWork.Save();

            // Audit Trail
            await _auditService.LogAsync(
                moduleName: "Security & Governance",
                entityName: "EmployeeRoleMapping",
                entityId: targetEmployeeId.ToString(),
                action: "ASSIGN_ROLES",
                beforeValues: new { TargetEmployeeId = targetEmployeeId, AssignedRoleIds = beforeRoleIds },
                afterValues: new { TargetEmployeeId = targetEmployeeId, AssignedRoleIds = targetRoleSet.ToList() },
                reason: reason
            );

            return true;
        }

        private async Task AssertNotRemovingLastAdminAsync(long targetEmployeeId, List<long> newRoleIds)
        {
            var allRoles = (await _unitOfWork.RoleMaster.GetAll()).ToList();
            var adminRoleIds = allRoles
                .Where(r => string.Equals(r.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(r.RoleName, "System Admin", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Id)
                .ToHashSet();

            var allMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => m.IsActive && !m.IsDeleted)
                .ToList();

            var currentTargetRoleIds = allMappings
                .Where(m => m.EmployeeId == targetEmployeeId)
                .Select(m => m.RoleId)
                .ToHashSet();

            bool targetCurrentlyHasAdmin = currentTargetRoleIds.Any(id => adminRoleIds.Contains(id));
            bool targetWillHaveAdmin = (newRoleIds ?? new List<long>()).Any(id => adminRoleIds.Contains(id));

            if (targetCurrentlyHasAdmin && !targetWillHaveAdmin)
            {
                // Count other active administrators
                var otherActiveAdmins = allMappings
                    .Where(m => m.EmployeeId != targetEmployeeId && adminRoleIds.Contains(m.RoleId))
                    .Select(m => m.EmployeeId)
                    .Distinct()
                    .ToList();

                if (!otherActiveAdmins.Any())
                {
                    throw new InvalidOperationException("Cannot revoke Admin role from the last active Administrator. At least one active System Administrator must exist in the organization.");
                }
            }
        }
    }
}
