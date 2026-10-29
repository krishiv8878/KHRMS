using System;

namespace KHRMS.Services.Request
{
    public class DailyWorkModeDTO
    {
        public long EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string WorkMode { get; set; } = "WFO"; // WFO, WFH, OD
        public string? Reason { get; set; }
    }

    public class EmployeeWorkModeDTO
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty; // yyyy-MM-dd
        public string WorkMode { get; set; } = "WFO"; // WFO, WFH, OD
        public string RequestType { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string Status { get; set; } = "Approved";
        public string? Reason { get; set; }
    }

    public class SetWorkModeDTO
    {
        public long EmployeeId { get; set; }
        public string? WorkDate { get; set; } // yyyy-MM-dd
        public DateTime? Date { get; set; }
        public string WorkMode { get; set; } = "WFO"; // WFO, WFH, OD
        public string? SelectedBy { get; set; } // Employee, Manager, Admin, Policy
        public string? Notes { get; set; }
    }

    public class WorkModeResponseDTO
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string WorkDate { get; set; } = string.Empty; // yyyy-MM-dd
        public string WorkMode { get; set; } = "WFO"; // WFO, WFH, OD
        public string SelectedBy { get; set; } = "Employee";
        public string? Notes { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
