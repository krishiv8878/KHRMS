using KHRMS.Core;
using KHRMS.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public class RoleMasterService : IRoleMasterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;

        private static readonly HashSet<string> ProtectedRoleNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Admin", "System Admin", "Employee"
        };

        public RoleMasterService(IUnitOfWork unitOfWork, IAuditService auditService)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<bool> AddRoleMaster(RoleMaster roleMaster)
        {
            if (roleMaster == null || string.IsNullOrWhiteSpace(roleMaster.RoleName))
            {
                return false;
            }

            var trimmedName = roleMaster.RoleName.Trim();
            var allExisting = await _unitOfWork.RoleMaster.GetAll();
            if (allExisting.Any(r => !r.IsDeleted && string.Equals(r.RoleName?.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"A role with the name '{trimmedName}' already exists.");
            }

            roleMaster.RoleName = trimmedName;
            roleMaster.CreatedDate = DateTime.UtcNow;
            roleMaster.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.RoleMaster.Add(roleMaster);
            var result = _unitOfWork.Save();

            if (result > 0)
            {
                await _auditService.LogAsync(
                    moduleName: "Security & Governance",
                    entityName: "RoleMaster",
                    entityId: roleMaster.Id.ToString(),
                    action: "CREATE",
                    beforeValues: null,
                    afterValues: new { roleMaster.Id, roleMaster.RoleName, roleMaster.IsActive },
                    reason: "New security role created"
                );
                return true;
            }

            return false;
        }

        public async Task<bool> DeleteRoleMaster(long roleMasterId)
        {
            if (roleMasterId <= 0) return false;

            var roleMasterDetails = await _unitOfWork.RoleMaster.GetById(roleMasterId);
            if (roleMasterDetails == null || roleMasterDetails.IsDeleted)
            {
                return false;
            }

            // Safeguard 1: Block deleting System Admin / Admin / core roles
            if (ProtectedRoleNames.Contains(roleMasterDetails.RoleName?.Trim() ?? string.Empty))
            {
                throw new InvalidOperationException($"System core role '{roleMasterDetails.RoleName}' is protected and cannot be deleted.");
            }

            // Safeguard 2: Block deleting roles that are currently assigned to active employees
            var activeMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => m.RoleId == roleMasterId && m.IsActive && !m.IsDeleted)
                .ToList();

            if (activeMappings.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete role '{roleMasterDetails.RoleName}' because it is currently assigned to {activeMappings.Count} active employee(s). Please reassign them before deleting this role.");
            }

            var beforeSnapshot = new
            {
                roleMasterDetails.Id,
                roleMasterDetails.RoleName,
                roleMasterDetails.IsActive
            };

            roleMasterDetails.IsDeleted = true;
            roleMasterDetails.IsActive = false;
            roleMasterDetails.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.RoleMaster.Update(roleMasterDetails);
            var result = _unitOfWork.Save();

            if (result > 0)
            {
                await _auditService.LogAsync(
                    moduleName: "Security & Governance",
                    entityName: "RoleMaster",
                    entityId: roleMasterId.ToString(),
                    action: "DELETE",
                    beforeValues: beforeSnapshot,
                    afterValues: null,
                    reason: "Role deleted by Administrator"
                );
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<RoleMaster>> GetAllRoleMaster()
        {
            var roleMaster = await _unitOfWork.RoleMaster.GetAll();
            return roleMaster.Where(r => !r.IsDeleted).ToList();
        }

        public async Task<RoleMaster?> GetRoleMasterById(long roleMasterId)
        {
            if (roleMasterId <= 0) return null;
            var details = await _unitOfWork.RoleMaster.GetById(roleMasterId);
            return details is { IsDeleted: false } ? details : null;
        }

        public async Task<bool> UpdateRoleMaster(RoleMaster roleMaster)
        {
            if (roleMaster == null || roleMaster.Id <= 0) return false;

            var existingRole = await _unitOfWork.RoleMaster.GetById(roleMaster.Id);
            if (existingRole == null || existingRole.IsDeleted) return false;

            var isProtected = ProtectedRoleNames.Contains(existingRole.RoleName?.Trim() ?? string.Empty);

            // Safeguard: Cannot deactivate a protected system core role
            if (isProtected && !roleMaster.IsActive)
            {
                throw new InvalidOperationException($"System core role '{existingRole.RoleName}' is protected and cannot be deactivated.");
            }

            var beforeSnapshot = new
            {
                existingRole.Id,
                existingRole.RoleName,
                existingRole.IsActive
            };

            existingRole.RoleName = roleMaster.RoleName?.Trim() ?? existingRole.RoleName;
            existingRole.IsActive = roleMaster.IsActive;
            existingRole.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.RoleMaster.Update(existingRole);
            var result = _unitOfWork.Save();

            if (result > 0)
            {
                await _auditService.LogAsync(
                    moduleName: "Security & Governance",
                    entityName: "RoleMaster",
                    entityId: existingRole.Id.ToString(),
                    action: "UPDATE",
                    beforeValues: beforeSnapshot,
                    afterValues: new { existingRole.Id, existingRole.RoleName, existingRole.IsActive },
                    reason: "Role updated"
                );
                return true;
            }

            return false;
        }
    }
}
