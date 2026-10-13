using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("SalaryStructures")]
    public class SalaryStructure : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        // Comma-separated or JSON list of component codes/IDs included in this structure
        public string? ComponentConfigurationJson { get; set; }
    }
}
