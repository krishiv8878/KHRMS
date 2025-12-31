using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KHRMS.Core;

namespace KHRMS.Services.Request
{
    public class AttendanceRequestUpdateDTO : KHRMSBase
    {
        public long Id { get; set; }
        
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        public string RequestType { get; set; }

        public DateTime RequestedDate { get; set; }

        public long RequestedBy { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; }

        public long LastActionBy { get; set; }

        public DateTime clockIn { get; set; }

        public DateTime clockOut { get; set; }
        public long ManagerId { get; set; }
    }
}
