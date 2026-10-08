using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("FullAndFinalSettlements")]
    public class FullAndFinalSettlement : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        public long? ResignationId { get; set; }

        public DateTime RelievingDate { get; set; }

        public decimal TenureYears { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LastDrawnBasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LastDrawnGrossSalary { get; set; }

        public decimal UnutilizedLeaveDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LeaveEncashmentAmount { get; set; }

        public bool IsGratuityEligible { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GratuityAmount { get; set; }

        public int NoticePeriodDays { get; set; }

        public int NoticePeriodShortfallDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NoticeRecoveryAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PendingSalaryDays { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PendingSalaryAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ApprovedReimbursements { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherAllowances { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OtherDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSettlementAmount { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Draft"; // "Draft", "Approved", "Settled"

        public int? ApprovedByEmployeeId { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? SettledDate { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }
    }
}
