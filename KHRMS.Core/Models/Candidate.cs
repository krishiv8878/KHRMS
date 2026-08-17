using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace KHRMS.Core
{
    public class Candidate : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Candidate FirstName is required")]
        public string? FirstName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Candidate LastName is required")]
        public string? LastName { get; set; }

        [StringLength(300)]
        [Required(ErrorMessage = "Candidate EmailAddress is required")]
        [EmailAddress]
        public string? EmailAddress { get; set; }

        [MaxLength(10)]
        [Required(ErrorMessage = "Candidate MobileNumber is required")]
        public string? MobileNumber { get; set; }

        [Required(ErrorMessage = "Candidate TotalExperience is required")]
        public string? TotalExperience { get; set; }

        [Required(ErrorMessage = "Candidate RelevantExperience is required")]
        public string? RelevantExperience { get; set; }

        [Required(ErrorMessage = "Candidate CurrentSalary is required")]
        public long? CurrentSalary { get; set; }

        [Required(ErrorMessage = "Candidate ExpectedSalary is required")]
        public long? ExpectedSalary { get; set; }

        [Required(ErrorMessage = "Candidate NoticePeriod is required")]
        public int? NoticePeriod { get; set; }

        [Required(ErrorMessage = "Candidate AppliedRole is required")]
        public string? AppliedRole { get; set; }

        [Required(ErrorMessage = "Candidate Stage is required")]
        public string? Stage { get; set; }

        [Required(ErrorMessage = "Candidate MatchScore is required")]
        public int? MatchScore { get; set; }         
    }
}
