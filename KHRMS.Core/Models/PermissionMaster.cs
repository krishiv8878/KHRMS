using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("PermissionMaster")]
    public class PermissionMaster : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PermissionCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;
    }
}
