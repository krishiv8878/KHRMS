using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("HelpdeskTicketActivities")]
    public class HelpdeskTicketActivity : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long TicketId { get; set; }

        [Required]
        public long PerformedByEmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; } = "Comment"; // "Created", "StatusChange", "Assigned", "Comment", "Resolution"

        [StringLength(50)]
        public string? OldStatus { get; set; }

        [StringLength(50)]
        public string? NewStatus { get; set; }

        public long? AssignedToEmployeeId { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        [ForeignKey("TicketId")]
        public virtual HelpdeskTicket? Ticket { get; set; }

        [ForeignKey("PerformedByEmployeeId")]
        public virtual Employee? PerformedByEmployee { get; set; }

        [ForeignKey("AssignedToEmployeeId")]
        public virtual Employee? AssignedToEmployee { get; set; }
    }
}
