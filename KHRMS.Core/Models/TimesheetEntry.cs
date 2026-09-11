using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("TimesheetEntries")]
    public class TimesheetEntry : KHRMSBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? TimesheetId { get; set; }

        [Required]
        public long EmployeeId { get; set; }

        [Required]
        public long ProjectId { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        [Required]
        [StringLength(1000)]
        public string TaskDescription { get; set; } = string.Empty;

        [Column(TypeName = "decimal(4,2)")]
        public decimal Hours { get; set; } = 0;

        public DateTime? ClockInRef { get; set; }

        public DateTime? ClockOutRef { get; set; }

        public bool IsRegularized { get; set; } = false;
    }
}
