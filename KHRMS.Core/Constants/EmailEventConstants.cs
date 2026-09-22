namespace KHRMS.Core.Constants
{
    public static class EmailEventConstants
    {
        // Authentication & Onboarding
        public const string OnboardingCredentials = "ONBOARDING_CREDENTIALS";
        public const string ForgotPassword = "FORGOT_PASSWORD";
        public const string PasswordChanged = "PASSWORD_CHANGED";

        // Leave & Attendance
        public const string LeaveRequestSubmitted = "LEAVE_REQUEST_SUBMITTED";
        public const string LeaveApproved = "LEAVE_APPROVED";
        public const string LeaveRejected = "LEAVE_REJECTED";
        public const string RegularizationApproved = "REGULARIZATION_APPROVED";
        public const string RegularizationRejected = "REGULARIZATION_REJECTED";

        // Timesheet
        public const string TimesheetSubmitted = "TIMESHEET_SUBMITTED";
        public const string TimesheetApproved = "TIMESHEET_APPROVED";
        public const string TimesheetRejected = "TIMESHEET_REJECTED";
        public const string TimesheetReminder = "TIMESHEET_REMINDER";

        // Hardware & Assets
        public const string AssetRequested = "ASSET_REQUESTED";
        public const string AssetAllocated = "ASSET_ALLOCATED";
        public const string AssetTicketCreated = "ASSET_TICKET_CREATED";
        public const string AssetTicketUpdated = "ASSET_TICKET_UPDATED";

        // Payroll & Finance
        public const string PayslipPublished = "PAYSLIP_PUBLISHED";
        public const string BankingDetailsApproved = "BANKING_DETAILS_APPROVED";

        // Resignation & Offboarding
        public const string ResignationSubmitted = "RESIGNATION_SUBMITTED";
        public const string ResignationApproved = "RESIGNATION_APPROVED";
        public const string ResignationRejected = "RESIGNATION_REJECTED";
    }
}
