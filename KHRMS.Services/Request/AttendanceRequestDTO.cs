using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KHRMS.Core;

namespace KHRMS.Services.Request
{
    public class AttendanceRequestDTO : KHRMSBase
    {
        public long? EmployeeId { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Request type is required")]
        public string? RequestType { get; set; }

        [Required(ErrorMessage = "Request Date type is required")]
        public DateTime RequestedDate { get; set; }

        public long? RequestedBy { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string? Reason { get; set; }

        public string? Status { get; set; }

        public long? LastActionBy { get; set; }

        public long? ActionBy { get; set; }

        public DateTime? ActionDate { get; set; }

        public string? RejectionReason { get; set; }

        [Required(ErrorMessage = "Clock In Time is required")]
        public DateTime clockIn { get; set; }

        public DateTime? clockOut { get; set; }

        public long? ManagerId { get; set; }
    }
}
