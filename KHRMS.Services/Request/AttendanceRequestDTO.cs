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
        [Required(ErrorMessage = "Employee is required")]
        public long EmployeeId { get; set; }

        [StringLength(50)]
        [Required(ErrorMessage = "Request type is required")]
        public string? RequestType { get; set; }

        [Required(ErrorMessage = "Request Date type is required")]
        public DateTime RequestedDate { get; set; }

        [Required(ErrorMessage = "Request id type is required")]
        public long RequestedBy { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string? Reason { get; set; }

        public string? Status { get; set; }

        public long? LastActionBy { get; set; }

        [Required(ErrorMessage = "Clock In Time is required")]
        public DateTime clockIn { get; set; }

        public DateTime? clockOut { get; set; }

        [Required(ErrorMessage = "Manager Id is required")]
        public long ManagerId { get; set; }
    }
}
