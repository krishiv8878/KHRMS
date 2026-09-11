using KHRMS.Core;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Services.Request
{
    //insert employees with roles
    public class EmployeeRequestModel : KHRMSBase
    {
        public long Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfileImage { get; set; }
        public string? EmailAddress { get; set; }
        public string? MobileNumber { get; set; }
        public int? DesignationId { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public string? Gender { get; set; }
        public string? CurrentAddress { get; set; }

        public string? PermanentAddress { get; set; }
        public long? EmployeeCode { get; set; }

        public List<long>? RoleIds { get; set; }
        public List<string?>? rolenames { get; set; }
        public string? ShiftId { get; set; }

        public long? ManagerId { get; set; }
        public string? ManagerName { get; set; }
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

        public bool? ProfileCompleted { get; set; } = true;
        public string? ClientUrl {  get; set; }

        public List<long>? SkillIds { get; set; }
        public List<String?>? Skills { get; set; }
        public List<long>? ProjectIds { get; set; }
        public List<String?>? Projects { get; set; }
    }
}