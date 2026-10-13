using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("Payslips")]
    public class Payslip : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? PayRunId { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [StringLength(50)]
        public string PayslipNumber { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrossSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDeductions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }

        public decimal LopDays { get; set; }

        public decimal WorkingDays { get; set; }

        public decimal PresentDays { get; set; }

        // JSON string preserving detailed earnings & deductions breakdown
        public string? BreakdownJson { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        public string? DocumentPath { get; set; }
    }
}
