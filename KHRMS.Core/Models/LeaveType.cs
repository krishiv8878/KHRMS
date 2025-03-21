using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace KHRMS.Core
{

    public class LeaveType : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "LeaveName is required")]
        public string? LeaveName { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "LeaveType is required")]
        public string? Type { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage = "Leave Description is required")]
        public string? Description { get; set; }

    }
}
