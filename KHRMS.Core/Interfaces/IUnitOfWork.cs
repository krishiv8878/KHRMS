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

        int Save();
    }
}
