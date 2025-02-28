using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Core
{
    public class Employee : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long EmployeeCode { get; set; }
        public string? ShiftIds { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "FirstName is required")]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "LastName is required")]
        public string? LastName { get; set; }

        [StringLength(300)]
        [Required(ErrorMessage = "EmailAddress is required")]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [MaxLength(10)]
        [Required(ErrorMessage = "MobileNumber is required")]
        public string? MobileNumber { get; set; }

        public int DesignationId { get; set; }

        [Required(ErrorMessage = "Date Of Joining is required")]
        public DateTime? DateOfJoining { get; set; }

        [StringLength(10)]
        [Required(ErrorMessage = "Gender is required")]
        public string? Gender { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = " CurrentAddress is required")]
        public string? CurrentAddress { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "PermanentAddress is required")]
        public string? PermanentAddress { get; set; }



    }

}

