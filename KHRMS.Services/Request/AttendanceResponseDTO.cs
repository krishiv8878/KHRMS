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
    public class AttendanceResponseDTO
    {
        public string RequestedBy { get; set; }
        public string? RequestType { get; set; }
        public DateTime RequestedDate { get; set; }
        public string? Reason { get; set; }
        public string? Status { get; set; }
        public DateTime ClockInTime { get; set; }

        public DateTime ClockOutTime { get; set; }
    }
}
