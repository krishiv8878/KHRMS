using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("RolePermissionMapping")]
    public class RolePermissionMapping : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual RoleMaster? Role { get; set; }

        public long PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public virtual PermissionMaster? Permission { get; set; }
    }
}
