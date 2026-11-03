using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("HelpdeskTickets")]
    public class HelpdeskTicket : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        [Required]
        public long RequesterEmployeeId { get; set; }

        [Required]
        [StringLength(50)]
        public string DepartmentCategory { get; set; } = "General"; // 'Human Resources', 'IT Support', 'Payroll & Finance', 'General'

        [Required]
        [StringLength(250)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium"; // 'Low', 'Medium', 'High', 'Critical'

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Open"; // 'Open', 'In Progress', 'Pending User', 'Resolved', 'Closed'

        public long? AssignedToEmployeeId { get; set; }

        public DateTime? SlaDueTime { get; set; }

        public DateTime? ResolvedDate { get; set; }

        public string? ResolutionNotes { get; set; }

        [ForeignKey("RequesterEmployeeId")]
        public virtual Employee? Requester { get; set; }

        [ForeignKey("AssignedToEmployeeId")]
        public virtual Employee? Assignee { get; set; }
    }
}
