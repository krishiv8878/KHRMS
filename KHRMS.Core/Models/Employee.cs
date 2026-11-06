using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using KHRMS.Core.Models;

namespace KHRMS.Core
{
    public class Employee : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? EmployeeCode { get; set; }
        public string? ShiftIds { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        public string? ProfileImage { get; set; }

        [StringLength(300)]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [MaxLength(10)]
        public string? MobileNumber { get; set; }

        public int? DesignationId { get; set; }

        public DateTime? DateOfJoining { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }

        [StringLength(100)]
        public string? CurrentAddress { get; set; }

        [StringLength(100)]
        public string? PermanentAddress { get; set; }

        public long? ManagerId { get; set; }

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

        public bool ProfileCompleted { get; set; } = false;

        public List<long>? SkillIds { get; set; }
        public List<long>? ProjectIds { get; set; }

        // Probation & Employment Lifecycle Tracking
        public DateTime? ProbationEndDate { get; set; }

        [StringLength(50)]
        public string? ProbationStatus { get; set; } = "Confirmed"; // UnderProbation, Confirmed, Extended, Terminated

        [StringLength(50)]
        public string? EmploymentStatus { get; set; } = "Active"; // Active, Probation, NoticePeriod, Exited

        [StringLength(50)]
        public string? EmploymentType { get; set; } = "FullTime"; // FullTime, Contract, Intern, PartTime

        public DateTime? RehireDate { get; set; }

        public DateTime? ExitDate { get; set; }

        // Tenures & Universal Identity Tracking
        public long? PreviousEmployeeId { get; set; }

        [StringLength(100)]
        public string? PersonId { get; set; }

        // Notice Period Overrides & Identifiers
        public int? NoticePeriodOverrideDays { get; set; }

        [StringLength(500)]
        public string? NoticePeriodOverrideReason { get; set; }

        [StringLength(50)]
        public string? PanNumber { get; set; }

        [StringLength(50)]
        public string? UanNumber { get; set; }

        [StringLength(300)]
        public string? PersonalEmail { get; set; }

        public long? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department? DepartmentNavigation { get; set; }

        [NotMapped]
        public string? Department
        {
            get => DepartmentNavigation?.Name ?? Branch ?? Designation ?? "General";
            set => Branch = value;
        }

        [NotMapped]
        public string? WorkEmail
        {
            get => EmailAddress ?? PrimaryEmailAddress;
            set => EmailAddress = value;
        }
    }
}
