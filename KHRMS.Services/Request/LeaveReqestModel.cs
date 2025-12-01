using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.Services.Request
{
    public class LeaveReqestModel
    {
        public long Id { get; set; }
        public long LeaveTypeId { get; set; }
        public long ApprovedBy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveTypeName { get; set; }
        public bool IsApproved { get; set; }   
        public string LeaveMode { get; set; }

        public string LeaveReason { get; set; }
    }
}
