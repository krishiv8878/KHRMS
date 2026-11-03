using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("SystemSettings")]
    public class SystemSetting : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string SettingValue { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }
    }
}
