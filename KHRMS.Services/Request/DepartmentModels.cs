using System;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    public class DepartmentDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? CostCenter { get; set; }
        public long? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? Description { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class CreateDepartmentRequest
    {
        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100, ErrorMessage = "Department Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
        public string? Code { get; set; }

        [StringLength(50, ErrorMessage = "Cost Center cannot exceed 50 characters")]
        public string? CostCenter { get; set; }

        public long? ManagerId { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
        public string? Description { get; set; }
    }

    public class UpdateDepartmentRequest
    {
        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100, ErrorMessage = "Department Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Code cannot exceed 20 characters")]
        public string? Code { get; set; }

        [StringLength(50, ErrorMessage = "Cost Center cannot exceed 50 characters")]
        public string? CostCenter { get; set; }

        public long? ManagerId { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
