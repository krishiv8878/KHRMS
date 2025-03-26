using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class Email : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }


        [Required(ErrorMessage = "EmailTemplateId is required")]
        [ForeignKey("EmailTemplatesMaster")]
        public long EmailTemplateId { get; set; }
        public virtual EmailTemplatesMaster EmailTemplatesMaster { get; set; }


        [Required(ErrorMessage = "EmailBody is required")]
        public string? EmailBody { get; set; }

        [StringLength(500)]
        public string? EmailSubject { get; set; }

        [StringLength(500)]
        public string? FromEmail { get; set; }

        [StringLength(500)]
        public string? ToEmail { get; set; }


        [Required(ErrorMessage = "is email delivered is required")]
        public bool IsDelivered { get; set; }

        [NotMapped]
        public new bool IsDeleted { get; set; }
    }
}