using System.Collections.Generic;

namespace KHRMS.Services.Request
{
    public class RolePermissionAssignmentDto
    {
        public long PermissionId { get; set; }
        public string? Scope { get; set; } = "All"; // "Self", "Team", "TeamAndIndirect", "Department", "Branch", "All"
    }

    public class SaveRolePermissionsRequest
    {
        public long RoleId { get; set; }
        public List<RolePermissionAssignmentDto> Permissions { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
    }

    public class RolePermissionViewDto
    {
        public long PermissionId { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Scope { get; set; } = "All";
        public bool IsSensitive { get; set; }
    }
}
