namespace KHRMS.Services.Request
{
    public class TimesheetTaskItemDTO
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public decimal Hours { get; set; }
    }

    public class TimesheetDayViewDTO
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public DateTime? ClockIn { get; set; }
        public DateTime? ClockOut { get; set; }
        public decimal AttendanceHours { get; set; } = 0;
        public bool IsRegularized { get; set; } = false;
        public bool IsShiftInProgress { get; set; } = false;
        public List<TimesheetTaskItemDTO> Tasks { get; set; } = new();
        public decimal DayTotalHours => Tasks.Sum(t => t.Hours);
    }

    public class TimesheetViewDTO
    {
        public long? TimesheetId { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string PeriodType { get; set; } = "Weekly";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalTimesheetHours { get; set; } = 0;
        public decimal TotalAttendanceHours { get; set; } = 0;
        public string Status { get; set; } = "Draft";
        public DateTime? SubmittedDate { get; set; }
        public long? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? RejectionReason { get; set; }
        public List<TimesheetDayViewDTO> Days { get; set; } = new();
    }
}
