using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("SalaryComponents")]
    public class SalaryComponent : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = "Earning"; // Earning or Deduction

        [Required]
        [StringLength(30)]
        public string CalculationType { get; set; } = "PercentageOfBasic"; // Flat, PercentageOfBasic, PercentageOfGross

        public decimal DefaultPercentage { get; set; } = 0;

        public decimal DefaultAmount { get; set; } = 0;

        public bool IsTaxable { get; set; } = true;

        public bool IsStatutory { get; set; } = false;

        [StringLength(250)]
        public string? Description { get; set; }
    }
}
