using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    public class Resignation : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ForeignKey("Employee")]
        [Required(ErrorMessage = "EmployeeId is required")]
        public long EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        [Required(ErrorMessage = "Resignation Reason is required")]
        public string? Reason { get; set; }

        public bool Status { get; set; }

        public long ManagerId { get; set; }

        [Required(ErrorMessage = "NoticePeriod is required")]
        public string? NoticePeriod { get; set; }

        [Required(ErrorMessage = "ResignationDate is required")]
        public DateTime Resignation_Date { get; set; }

        [Required(ErrorMessage = "RoleId is MIssing")]
        public List<long>? RoleId { get; set; }

        public string? ResignationStatus { get; set; } = "Submitted";

        public DateTime? RequestedLwd { get; set; }

        public DateTime? ProposedLwd { get; set; }

        public DateTime? ApprovedLwd { get; set; }

        public int NoticeShortfallDays { get; set; } = 0;

        public string? NoticeHandling { get; set; } = "Standard"; // Standard, Waived, ShortfallRecovery

        public string? ManagerRemarks { get; set; }

        public string? HrRemarks { get; set; }

        public string? ExitInterviewRemarks { get; set; }

        public bool IsLwdLocked { get; set; } = false;

        public string? AssetClearanceOverrideReason { get; set; }

        // Notice Period Policy Snapshot
        public int? NoticePeriodDaysSnapshot { get; set; }

        public string? NoticePeriodSourceSnapshot { get; set; } // EmployeeOverride, DesignationProbationPolicy, DesignationPolicy, CompanyDefault

        // Full & Final Readiness Gate
        public string? FnFStatus { get; set; } = "Pending"; // Pending, Prepared, NotApplicable

        public DateTime? FnFPreparedDate { get; set; }

        public string? FnFRemarks { get; set; }
    }
}
