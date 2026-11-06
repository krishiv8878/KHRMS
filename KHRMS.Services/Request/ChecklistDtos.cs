namespace KHRMS.Services.Request
{
    public class ChecklistTemplateDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ChecklistType { get; set; } = "Onboarding";
        public string? Description { get; set; }
        public string? Department { get; set; }
        public List<ChecklistTemplateTaskDto> Tasks { get; set; } = new();
    }

    public class ChecklistTemplateTaskDto
    {
        public long Id { get; set; }
        public long TemplateId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "Employee";
        public string? DefaultAssigneeRole { get; set; }
        public long? DefaultAssigneeEmployeeId { get; set; }
        public int DueDays { get; set; } = 3;
        public bool IsMandatory { get; set; } = true;
        public int DisplayOrder { get; set; } = 1;
    }

    public class ChecklistTaskUpdateDto
    {
        public string Status { get; set; } = "Completed"; // "Completed", "Waived", "InProgress"
        public string? Remarks { get; set; }
        public string? WaiveReason { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public class ChecklistTemplateCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string ChecklistType { get; set; } = "Onboarding"; // "Onboarding", "Offboarding"
        public string? Description { get; set; }
        public string? Department { get; set; }
        public List<ChecklistTemplateTaskCreateDto> Tasks { get; set; } = new();
    }

    public class ChecklistTemplateTaskCreateDto
    {
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "Employee"; // "Employee", "HR", "IT", "Admin", "Finance", "Manager", "Facilities"
        public string? DefaultAssigneeRole { get; set; }
        public long? DefaultAssigneeEmployeeId { get; set; }
        public int DueDays { get; set; } = 3;
        public bool IsMandatory { get; set; } = true;
        public int DisplayOrder { get; set; } = 1;
    }

    public class EmployeeChecklistDto
    {
        public long Id { get; set; }
        public long EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeEmail { get; set; }
        public string? Department { get; set; }
        public long? TemplateId { get; set; }
        public string ChecklistType { get; set; } = "Onboarding";
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = "InProgress";
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public double CompletionPercentage => TotalTasks == 0 ? 0 : Math.Round(((double)CompletedTasks / TotalTasks) * 100, 1);
        public DateTime InitiatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Remarks { get; set; }
        public List<ChecklistTaskDto> Tasks { get; set; } = new();
    }

    public class ChecklistTaskDto
    {
        public long Id { get; set; }
        public long EmployeeChecklistId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "Employee";
        public string Status { get; set; } = "Pending";
        public DateTime? DueDate { get; set; }
        public long? AssignedEmployeeId { get; set; }
        public string? AssignedEmployeeName { get; set; }
        public string? AssignedRole { get; set; }
        public bool IsMandatory { get; set; }
        public DateTime? ClearedDate { get; set; }
        public long? ClearedByEmployeeId { get; set; }
        public string? ClearedByEmployeeName { get; set; }
        public string? Remarks { get; set; }
        public string? WaiveReason { get; set; }
        public long? WaivedByEmployeeId { get; set; }
        public string? WaivedByEmployeeName { get; set; }
        public string? AttachmentUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool CanClear { get; set; }
        public bool CanWaive { get; set; }
    }

    public class TaskClearanceDto
    {
        public string? Remarks { get; set; }
        public string? AttachmentUrl { get; set; }
    }

    public class TaskWaiveDto
    {
        public string WaiveReason { get; set; } = string.Empty;
    }

    public class TaskAssigneeUpdateDto
    {
        public long? AssignedEmployeeId { get; set; }
        public string? AssignedRole { get; set; }
    }

    public class ProbationReminderDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime DateOfJoining { get; set; }
        public DateTime ProbationEndDate { get; set; }
        public int DaysRemaining { get; set; }
        public string ProbationStatus { get; set; } = "UnderProbation";
        public long? ManagerId { get; set; }
        public string ManagerName { get; set; } = string.Empty;
    }

    public class ProbationDecisionDto
    {
        public string Decision { get; set; } = "Confirm"; // "Confirm", "Extend", "Terminate"
        public DateTime? NewEndDate { get; set; } // Required if Extend
        public string? Remarks { get; set; }
    }

    public class ManagerResignationReviewDto
    {
        public bool Accept { get; set; } = true;
        public DateTime? ProposedLwd { get; set; }
        public string? Remarks { get; set; }
    }

    public class HrResignationApprovalDto
    {
        public DateTime ApprovedLwd { get; set; }
        public string? NoticeHandling { get; set; } = "Standard"; // Standard, Waived, ShortfallRecovery
        public string? Remarks { get; set; }
    }

    public class FinalizeExitDto
    {
        public string? Remarks { get; set; }
        public string? OverrideReason { get; set; }
    }

    public class RehireEmployeeDto
    {
        public DateTime NewJoiningDate { get; set; } = DateTime.Today;
        public int? DesignationId { get; set; }
        public string? Designation { get; set; }
        public long? ManagerId { get; set; }
        public string? ShiftId { get; set; }
        public decimal? AnnualCtc { get; set; }
        public int? ProbationDays { get; set; } = 90;
        public string? Remarks { get; set; }
        public bool? ReusePreviousEmployeeCode { get; set; }
    }

    public class MatchingFormerEmployeeDto
    {
        public long EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string? PersonalEmail { get; set; }
        public string? PanNumber { get; set; }
        public string? UanNumber { get; set; }
        public string? Designation { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public DateTime? ExitDate { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public string MatchReason { get; set; } = string.Empty; // e.g. "Matched by PAN", "Matched by Personal Email"
    }

    public class HistoricalRehireReportItemDto
    {
        public long EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public DateTime? RehireDate { get; set; }
        public DateTime? CurrentDateOfJoining { get; set; }
        public string Designation { get; set; } = string.Empty;
    }

    public class PreviousEmploymentHistoryDto
    {
        public long EmployeeId { get; set; }
        public long? EmployeeCode { get; set; }
        public string Designation { get; set; } = string.Empty;
        public DateTime? DateOfJoining { get; set; }
        public DateTime? ExitDate { get; set; }
        public string EmploymentStatus { get; set; } = string.Empty;
        public List<PreviousPayslipSummaryDto> Payslips { get; set; } = new();
        public List<PreviousDocumentSummaryDto> Documents { get; set; } = new();
    }

    public class PreviousPayslipSummaryDto
    {
        public long Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal NetPay { get; set; }
        public string? PayslipUrl { get; set; }
    }

    public class PreviousDocumentSummaryDto
    {
        public long Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentName { get; set; } = string.Empty;
        public string? DocumentUrl { get; set; }
        public DateTime? UploadedDate { get; set; }
    }

    public class NoticePeriodOverrideDto
    {
        public int NoticePeriodDays { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class FnFStatusUpdateDto
    {
        public string Status { get; set; } = "Prepared"; // "Prepared", "NotApplicable"
        public string? Remarks { get; set; }
        public string? OverrideReason { get; set; }
    }

    public class AssetReturnVerificationDto
    {
        public bool AllCleared { get; set; }
        public int TotalAssets { get; set; }
        public int ReturnedAssets { get; set; }
        public int PendingAssets { get; set; }
        public List<string> PendingAssetDetails { get; set; } = new();
    }
}
