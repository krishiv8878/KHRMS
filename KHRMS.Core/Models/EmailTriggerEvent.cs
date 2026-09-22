using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    public class EmailTriggerEvent : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EventName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string AvailableVariablesJson { get; set; } = "[]";

        [MaxLength(250)]
        public string DefaultSubject { get; set; } = string.Empty;

        [ForeignKey("EmailTemplatesMaster")]
        public long? ActiveTemplateId { get; set; }
        public virtual EmailTemplatesMaster? EmailTemplate { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
