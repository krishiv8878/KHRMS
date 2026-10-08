using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("ReimbursementClaims")]
    public class ReimbursementClaim : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // e.g. "Fuel", "Internet / Phone", "Medical", "Travel", "Books & Periodicals"

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime BillDate { get; set; }

        [StringLength(100)]
        public string? BillNumber { get; set; }

        [StringLength(150)]
        public string? MerchantName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ReceiptUrl { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected", "Paid"

        [Column(TypeName = "decimal(18,2)")]
        public decimal ApprovedAmount { get; set; }

        public int? ReviewedByEmployeeId { get; set; }

        public DateTime? ReviewedDate { get; set; }

        [StringLength(500)]
        public string? ReviewRemarks { get; set; }

        public long? PayRunId { get; set; }
    }
}
