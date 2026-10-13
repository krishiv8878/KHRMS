using KHRMS.Core.Interfaces;

namespace KHRMS.Core
{
    public interface IUnitOfWork : IDisposable
    {
        ICandidateRepository Candidates { get; }
        ISkillRepository Skills { get; }
        IDesignationRepository Designations { get; }
        IEmployeeRepository Employees { get; }
        IHolidayRepository Holidays { get; }
        ILeaveRepository LeaveType { get; }
        IAssetsMasterRepository AssetsMasters { get; }
        IUserLoginRepository UserLogins { get; }
        IProjectMasterRepository ProjectMasters { get; }
        IUserRegistrationRepository UserRegistrations { get; }

        IEmployeeRoleMappingRepository EmployeeRoleMappings { get; }
        IRoleMasterRepository RoleMaster { get; }

        IAttendanceRequestRepository AttendanceRequests { get; }
        IEmployeeAttendanceRepository EmployeeAttendance { get; }

        IEmployeePaymentInfoRepository EmployeePaymentInfo { get; }
        IEmployeeDocumentRepository  EmployeementDocument { get; }

        IShiftRepository ShiftRepository { get; }

        IEmailTemplateTypeMasterRepository EmailTemplateTypeMaster { get; }

        IEmailTemplateMasterRepository EmailTemplateMaster { get; }
        IEmailRepository Email {  get; }
        ILeaveRequestRepository LeaveRequest { get; }
        IResignationRepository Resignation { get; }
        IAttendanceLogRepository AttendanceLog { get; }
        ITimesheetRepository Timesheets { get; }
        ITimesheetEntryRepository TimesheetEntries { get; }
        IAssetRequestRepository AssetRequests { get; }
        IAssetRequestLogRepository AssetRequestLogs { get; }
        INotificationRepository Notifications { get; }
        IEmailTriggerEventRepository EmailTriggerEvents { get; }
        IPermissionMasterRepository PermissionMaster { get; }
        IRolePermissionMappingRepository RolePermissionMappings { get; }
        IUserPermissionMappingRepository UserPermissionMappings { get; }

        // Payroll Repositories
        ISalaryComponentRepository SalaryComponents { get; }
        ISalaryStructureRepository SalaryStructures { get; }
        IEmployeeSalaryAssignmentRepository EmployeeSalaryAssignments { get; }
        IPayRunRepository PayRuns { get; }
        IPayRunEmployeeDetailRepository PayRunEmployeeDetails { get; }
        IPayslipRepository Payslips { get; }
        IReimbursementClaimRepository ReimbursementClaims { get; }
        IFullAndFinalSettlementRepository FullAndFinalSettlements { get; }

        // PMS Repositories
        IPmsGoalRepository PmsGoals { get; }
        IPmsKeyResultRepository PmsKeyResults { get; }
        IPmsGoalCheckInRepository PmsGoalCheckIns { get; }
        IPmsReviewCycleRepository PmsReviewCycles { get; }
        IPmsAppraisalRepository PmsAppraisals { get; }
        IPmsFeedbackRepository PmsFeedbacks { get; }
        IPmsOneOnOneRepository PmsOneOnOnes { get; }
        IPmsPipRepository PmsPips { get; }

        IExpenseCategoryRepository ExpenseCategories { get; }
        IExpenseReportRepository ExpenseReports { get; }
        IExpenseItemRepository ExpenseItems { get; }
        IExpenseMileageRepository ExpenseMileages { get; }
        IExpenseAdvanceRepository ExpenseAdvances { get; }

        // Recruitment & ATS Repositories
        IJobRequisitionRepository JobRequisitions { get; }
        IJobInterviewRepository JobInterviews { get; }
        IJobOfferRepository JobOffers { get; }

        int Save();
    }
}
