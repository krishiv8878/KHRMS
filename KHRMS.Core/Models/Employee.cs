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

        [StringLength(20)]
        [Required(ErrorMessage = "Gender is required")]
        public string? Gender { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = " CurrentAddress is required")]
        public string? CurrentAddress { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "PermanentAddress is required")]
        public string? PermanentAddress { get; set; }

        public long ManagerId { get; set; }

        //Primary Email Address Field
        [StringLength(300)]
        [EmailAddress(ErrorMessage = "Primary Email Address required")]
        public string? PrimaryEmailAddress { get; set; }

        // ====================== Additional Fields ======================

        // Primary Contact
        [StringLength(100)]
        public string? PrimaryContactName { get; set; }

        [StringLength(50)]
        public string? PrimaryContactRelationship { get; set; }

        [StringLength(15)]
        public string? PrimaryContactPhone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? PrimaryContactEmail { get; set; }

        [StringLength(200)]
        public string? PrimaryContactAddress { get; set; }

        // Secondary Contact
        [StringLength(100)]
        public string? SecondaryContactName { get; set; }

        [StringLength(50)]
        public string? SecondaryContactRelationship { get; set; }

        [StringLength(15)]
        public string? SecondaryContactPhone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? SecondaryContactEmail { get; set; }

        [StringLength(200)]
        public string? SecondaryContactAddress { get; set; }

        // Education Qualification
        [StringLength(100)]
        public string? Degree { get; set; }

        [StringLength(100)]
        public string? University { get; set; }

        public int? YearOfPassing { get; set; }

        public float? Percentage { get; set; }

        // Experience
        [StringLength(100)]
        public string? CompanyName { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(50)]
        public string? ExperienceDuration { get; set; }

        [StringLength(100)]
        public string? ExperienceLocation { get; set; }

        public string? Responsibilities { get; set; }

        // Passport Information
        [StringLength(20)]
        public string? PassportNumber { get; set; }

        [StringLength(50)]
        public string? Nationality { get; set; }

        public DateTime? PassportIssueDate { get; set; }

        public DateTime? PassportExpiryDate { get; set; }

        [StringLength(200)]
        public string? PassportScanCopy { get; set; }

        // Branch
        [StringLength(100)]
        public string? Branch { get; set; }
        public DateTime? DateOfBirth { get; set; }


    }

}

