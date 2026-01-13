using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KHRMS.Core.Models;

namespace KHRMS.Core
{
    public class Skill : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(100)]
        [Required(ErrorMessage ="SKillName is required")]
        public string? SkillName { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDeleted { get; set; }

    }
}
