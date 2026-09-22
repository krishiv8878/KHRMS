using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("UserPermissionMapping")]
    public class UserPermissionMapping : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        public long PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public virtual PermissionMaster? Permission { get; set; }

        public bool IsGranted { get; set; } = true;
    }
}
