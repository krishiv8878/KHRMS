using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("EmployeeSalaryAssignments")]
    public class EmployeeSalaryAssignment : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public long EmployeeId { get; set; }

        public long? SalaryStructureId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualCtc { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyGross { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BasicSalary { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Hra { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpecialAllowance { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EpfEmployee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EpfEmployer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EsicEmployee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal EsicEmployer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProfessionalTax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyTds { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyNetTakeHome { get; set; }

        [StringLength(20)]
        public string TaxRegime { get; set; } = "New"; // "New" or "Old"

        public DateTime EffectiveFromDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}
