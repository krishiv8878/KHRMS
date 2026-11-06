namespace KHRMS.Services.Request
{
    // ================= 1. COMPANY POLICY DTOS =================
    public class CreateCompanyPolicyRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "General"; // 'Leave', 'Conduct', 'Attendance', 'IT & Security', 'Compensation', 'General'
        public string ContentHtml { get; set; } = string.Empty;
        public bool IsMandatoryAcknowledgement { get; set; } = true;
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow.Date;
    }

    public class UpdateCompanyPolicyRequest
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string ContentHtml { get; set; } = string.Empty;
        public bool IsMandatoryAcknowledgement { get; set; } = true;
        public DateTime EffectiveDate { get; set; }
        public bool IncrementVersion { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class AcknowledgePolicyRequest
    {
        public long PolicyId { get; set; }
        public int PolicyVersion { get; set; }
    }

    public class PolicyDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public int Version { get; set; }
        public bool IsMandatoryAcknowledgement { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsAcknowledgedByCurrentUser { get; set; }
        public DateTime? UserAcknowledgedDate { get; set; }
        public int TotalAcknowledgementsCount { get; set; }
    }

    public class PolicyComplianceSummaryDto
    {
        public int TotalActivePolicies { get; set; }
        public int MandatoryPoliciesCount { get; set; }
        public int TotalEmployeesCount { get; set; }
        public int FullyCompliantEmployeesCount { get; set; }
        public decimal OverallComplianceRatePercent { get; set; }
        public List<PolicyCompliancePerPolicyDto> PoliciesSummary { get; set; } = new();
    }

    public class PolicyCompliancePerPolicyDto
    {
        public long PolicyId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Version { get; set; }
        public int AcknowledgedCount { get; set; }
        public int PendingCount { get; set; }
        public decimal CompliancePercent { get; set; }
    }

    // ================= 2. HELPDESK TICKET DTOS =================
    public class CreateHelpdeskTicketRequest
    {
        public string DepartmentCategory { get; set; } = "General"; // 'Human Resources', 'IT Support', 'Payroll & Finance', 'General'
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // 'Low', 'Medium', 'High', 'Critical'
        public long? AssignedToEmployeeId { get; set; }
    }

    public class UpdateHelpdeskTicketStatusRequest
    {
        public long TicketId { get; set; }
        public string Status { get; set; } = "In Progress"; // 'Open', 'In Progress', 'Pending User', 'Resolved', 'Closed'
        public string? ResolutionNotes { get; set; }
        public long? AssignedToEmployeeId { get; set; }
    }

    public class AssignTicketRequest
    {
        public long TicketId { get; set; }
        public long AssignToEmployeeId { get; set; }
        public string? AssignmentNotes { get; set; }
    }

    public class AddTicketCommentRequest
    {
        public long TicketId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class HelpdeskTicketActivityDto
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public string ActivityType { get; set; } = "Comment"; // "Created", "StatusChange", "Assigned", "Comment", "Resolution"
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public long? AssignedToEmployeeId { get; set; }
        public string? AssigneeName { get; set; }
        public long PerformedByEmployeeId { get; set; }
        public string PerformedByName { get; set; } = string.Empty;
        public string PerformedByEmail { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class HelpdeskTicketDto
    {
        public long Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public long RequesterEmployeeId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string RequesterCode { get; set; } = string.Empty;
        public string RequesterEmail { get; set; } = string.Empty;
        public string RequesterDepartment { get; set; } = string.Empty;
        public string DepartmentCategory { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long? AssignedToEmployeeId { get; set; }
        public string AssigneeName { get; set; } = string.Empty;
        public DateTime? SlaDueTime { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolutionNotes { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SlaStatus { get; set; } = "On Track"; // 'On Track', 'Overdue', 'Resolved in SLA', 'Resolved Past SLA'
        public List<HelpdeskTicketActivityDto> Activities { get; set; } = new();
        public bool CanCurrentUserUpdateStatus { get; set; }
        public bool CanCurrentUserAssign { get; set; }
        public bool CanCurrentUserClaim { get; set; }
        public bool CanCurrentUserComment { get; set; } = true;
        public bool CanRequesterCloseOrReopen { get; set; }
        public bool CanRequesterCancel { get; set; }
    }

    public class HelpdeskMetricsDto
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int CriticalTicketsCount { get; set; }
        public decimal SlaCompliancePercent { get; set; }
        public decimal AverageResolutionHours { get; set; }
    }
    // ================= 3. REPORTING DTOS =================
    public class AttendanceReportItemDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalCalendarDays { get; set; }
        public int ExpectedWorkingDays { get; set; }
        public decimal PresentDays { get; set; }
        public decimal PaidLeaveDays { get; set; }
        public decimal UnpaidLopDays { get; set; }
        public int LateArrivalsCount { get; set; }
        public decimal TotalWorkHours { get; set; }
        public decimal PunctualityRatePercent { get; set; }
    }

    public class LeaveBalanceReportItemDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string LeaveType { get; set; } = string.Empty;
        public decimal AnnualQuota { get; set; }
        public decimal AccruedToDate { get; set; }
        public decimal CarriedForward { get; set; }
        public decimal ConsumedDays { get; set; }
        public decimal PendingReservedDays { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal UnpaidLopDays { get; set; }
    }

    public class PayrollReconciliationReportItemDto
    {
        public long EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public decimal BasicSalary { get; set; }
        public decimal Hra { get; set; }
        public decimal SpecialAllowance { get; set; }
        public decimal Reimbursements { get; set; }
        public decimal GrossPay { get; set; }
        public decimal EpfDeduction { get; set; }
        public decimal EsicDeduction { get; set; }
        public decimal PtDeduction { get; set; }
        public decimal TdsDeduction { get; set; }
        public decimal LopDeduction { get; set; }
        public decimal LoanEmiDeduction { get; set; }
        public decimal ManualAdjustments { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsOnHold { get; set; }
        public string? HoldReason { get; set; }
    }

    public class HeadcountDemographicsReportDto
    {
        public int TotalActiveEmployees { get; set; }
        public int TotalOnProbation { get; set; }
        public int TotalExitedYearToDate { get; set; }
        public Dictionary<string, int> HeadcountByDepartment { get; set; } = new();
        public Dictionary<string, int> HeadcountByEmploymentType { get; set; } = new();
        public decimal AverageTenureYears { get; set; }
        public decimal AnnualTurnoverRatePercent { get; set; }
    }

    // ================= 4. PMS CALIBRATION DTOS =================
    public class PmsCalibrationDataDto
    {
        public long ReviewCycleId { get; set; }
        public string CycleName { get; set; } = string.Empty;
        public int TotalAppraisals { get; set; }
        public int CalibratedAppraisalsCount { get; set; }
        public decimal AverageManagerRating { get; set; }
        public decimal AverageFinalRating { get; set; }
        public PmsBellCurveDistributionDto BellCurve { get; set; } = new();
        public List<PmsCalibrationAppraisalItemDto> Appraisals { get; set; } = new();
    }

    public class PmsBellCurveDistributionDto
    {
        // 5 Tiers: Unsatisfactory (1), Needs Improvement (2), Meets Expectations (3), Exceeds (4), Outstanding (5)
        public int Tier1Count { get; set; } // < 2.0
        public int Tier2Count { get; set; } // 2.0 - 2.99
        public int Tier3Count { get; set; } // 3.0 - 3.99
        public int Tier4Count { get; set; } // 4.0 - 4.49
        public int Tier5Count { get; set; } // >= 4.5

        public decimal Tier1Percent { get; set; }
        public decimal Tier2Percent { get; set; }
        public decimal Tier3Percent { get; set; }
        public decimal Tier4Percent { get; set; }
        public decimal Tier5Percent { get; set; }

        // Recommended Gaussian Targets
        public decimal TargetTier1Percent => 10.0m;
        public decimal TargetTier2Percent => 20.0m;
        public decimal TargetTier3Percent => 40.0m;
        public decimal TargetTier4Percent => 20.0m;
        public decimal TargetTier5Percent => 10.0m;
    }

    public class PmsCalibrationAppraisalItemDto
    {
        public long AppraisalId { get; set; }
        public long EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public decimal? SelfRating { get; set; }
        public decimal? ManagerRating { get; set; }
        public decimal? FinalRating { get; set; }
        public decimal? CalibratedRating { get; set; }
        public string? CalibrationRemarks { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CalibrateAppraisalRequest
    {
        public long AppraisalId { get; set; }
        public decimal CalibratedRating { get; set; }
        public string? Remarks { get; set; }
    }

    public class EmployeeAppraisalHistoryDto
    {
        public long AppraisalId { get; set; }
        public long ReviewCycleId { get; set; }
        public string CycleName { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? SelfRating { get; set; }
        public decimal? ManagerRating { get; set; }
        public decimal? FinalRating { get; set; }
        public decimal? CalibratedRating { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ManagerComments { get; set; }
        public string? HrComments { get; set; }
    }
}
