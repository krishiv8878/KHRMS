using KHRMS.Core;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    //insert employees with roles
    public class EmployeeRequestModel : KHRMSBase
    {
        public long Id { get; set; }
        [Required(ErrorMessage = "FirstName is required")]
        public string? FirstName { get; set; }
        [Required(ErrorMessage = "LastName is required")]
        public string? LastName { get; set; }
        [Required(ErrorMessage = "EmailAddress is required")]
        public string? EmailAddress { get; set; }
        [Required(ErrorMessage = "MobileNumber is required")]
        public string? MobileNumber { get; set; }
        public int DesignationId { get; set; }
        [Required(ErrorMessage = "Date Of Joining is required")]
        public DateTime? DateOfJoining { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        public string? Gender { get; set; }
        [Required(ErrorMessage = "CurrentAddress is required")]
        public string? CurrentAddress { get; set; }
        [Required(ErrorMessage = "PermanentAddress is required")]
        public string? PermanentAddress { get; set; }
        [Required(ErrorMessage = "Code is required")]
        public long EmployeeCode { get; set; }
        [Required(ErrorMessage = "Role id is required")]
        public List<long>? RoleIds { get; set; }
        [Required(ErrorMessage = "Shift id is required")]
        public string? ShiftId { get; set; }
    }
}