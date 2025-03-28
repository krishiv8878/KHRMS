
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class EmailTemplateTypeMaster : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }


        [Required(ErrorMessage = "TemplateType is required")]
        public string TemplateType { get; set; }


        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
    }
}
