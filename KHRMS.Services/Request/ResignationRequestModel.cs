using System;
using System.Collections.Generic;
using KHRMS.Core;

namespace KHRMS.Services.Request
{
    public class ResignationRequestModel : KHRMSBase
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeEmail { get; set; }
        public string? Department { get; set; }

        public string? Reason { get; set; }

        public bool Status { get; set; }

        public long ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public string? NoticePeriod { get; set; }

        public DateTime? Resignation_Date { get; set; }

        public List<long>? RoleId { get; set; }
        public List<string?>? RoleName { get; set; }

        // State Machine & Notice Period Fields
        public string? ResignationStatus { get; set; } = "Submitted";
        public DateTime? RequestedLwd { get; set; }
        public DateTime? ProposedLwd { get; set; }
        public DateTime? ApprovedLwd { get; set; }
        public int NoticeShortfallDays { get; set; } = 0;
        public string? NoticeHandling { get; set; } = "Standard";
        public string? ManagerRemarks { get; set; }
        public string? HrRemarks { get; set; }
        public string? ExitInterviewRemarks { get; set; }
        public bool IsLwdLocked { get; set; } = false;
        public string? AssetClearanceOverrideReason { get; set; }
    }
}
