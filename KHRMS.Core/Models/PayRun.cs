using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("PayRuns")]
    public class PayRun : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public int Month { get; set; } // 1 to 12

        [Required]
        public int Year { get; set; }

        public DateTime PayPeriodStart { get; set; }

        public DateTime PayPeriodEnd { get; set; }

        public int TotalEmployees { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGrossPay { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalNetPay { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Draft"; // "Draft", "Submitted", "Approved", "Disbursed"

        public int? ProcessedByEmployeeId { get; set; }

        public int? ApprovedByEmployeeId { get; set; }

        public DateTime? ApprovedDate { get; set; }

        public DateTime? DisbursedDate { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; } = "Bank Transfer";

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
