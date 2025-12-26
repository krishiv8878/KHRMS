using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHRMS.Core.Models
{
    [Table("AttendanceLog")]
    public class AttendanceLog
    {
        [Key]
        public long Id { get; set; }
        [ForeignKey("employee")]
        public long EmployeeId { get; set; }
        public virtual Employee? employee { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set;}
        public decimal Duration { get; set; }
    }
}
