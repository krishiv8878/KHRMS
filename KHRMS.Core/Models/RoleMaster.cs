using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core
{
    [Table("RoleMasters")]
    public class RoleMaster : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "RoleName is required")]
        public string? RoleName { get; set; }
    }
}
