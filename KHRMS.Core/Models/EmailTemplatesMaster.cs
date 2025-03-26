using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class EmailTemplatesMaster : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required(ErrorMessage = "EmailTemplateTypeId is required")]

        [ForeignKey("EmailTemplateTypeMaster")]
        public long EmailTemplateTypeId { get; set; }
        public virtual EmailTemplateTypeMaster EmailTemplateTypeMaster { get; set; }


        [Required(ErrorMessage = "TemplateHtml is required")]
        public string TemplateHtml { get; set; }
    }
}