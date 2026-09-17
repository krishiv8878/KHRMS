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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveTypeName { get; set; }
        public string LeaveMode { get; set; }

        public string LeaveReason { get; set; }
        public string? Status { get; set; }
        public long? ActionBy { get; set; }
        public DateTime? ActionDate { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class EmployeeLeaveBalanceDto
    {
        public long LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int TotalQuota { get; set; }
        public double UsedDays { get; set; }
        public double RemainingDays { get; set; }
        public int PendingDays { get; set; }
    }
}
