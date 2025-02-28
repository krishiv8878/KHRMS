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
        IDocumentMasterRepository DocumentMasters { get; }

        int Save();
    }
}
