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
        public long employee_id { get; set; }
        public virtual Employee? employee { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime? in_time { get; set; }
        public DateTime? out_time { get; set;}
        public decimal duration { get; set; }
    }
}
