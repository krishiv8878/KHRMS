using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace KHRMS.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KHRMSContextClass _dbContext;
        public ICandidateRepository Candidates { get; }
        public ISkillRepository Skills { get; }
        public IDesignationRepository Designations { get; }

        public IHolidayRepository Holidays { get; }

        public IEmployeeRepository Employees { get; }

        public ILeaveRepository LeaveType { get; }

        public IAssetsMasterRepository AssetsMasters { get; }

        public IUserLoginRepository UserLogins { get; }

        public IProjectMasterRepository ProjectMasters { get; }
        public IUserRegistrationRepository UserRegistrations { get; }
        
        public IRoleMasterRepository RoleMaster {  get; }
        public IEmployeeRoleMappingRepository EmployeeRoleMappings { get; }

        public IAttendanceRequestRepository AttendanceRequests { get; }

        public IEmployeeAttendanceRepository EmployeeAttendance  { get; }

        public IEmployeePaymentInfoRepository EmployeePaymentInfo { get; }

        public IEmployeeDocumentRepository EmployeementDocument { get; }

        public IShiftRepository ShiftRepository { get; }

        public IEmailTemplateMasterRepository EmailTemplateMaster { get; }

        public IEmailTemplateTypeMasterRepository EmailTemplateTypeMaster {  get; }

        public IEmailRepository Email {  get; }

        public ILeaveRequestRepository LeaveRequest { get; }
        public IResignationRepository Resignation { get; }
        public IAttendanceLogRepository AttendanceLog { get; }
        public ITimesheetRepository Timesheets { get; }
        public ITimesheetEntryRepository TimesheetEntries { get; }
        public IAssetRequestRepository AssetRequests { get; }
        public IAssetRequestLogRepository AssetRequestLogs { get; }
        public INotificationRepository Notifications { get; }
        public IEmailTriggerEventRepository EmailTriggerEvents { get; }
        public IPermissionMasterRepository PermissionMaster { get; }
        public IRolePermissionMappingRepository RolePermissionMappings { get; }
        public IUserPermissionMappingRepository UserPermissionMappings { get; }

        public UnitOfWork(KHRMSContextClass dbContext,
                            ICandidateRepository candidateRepository,
                            ISkillRepository skillRepository,
                            IDesignationRepository designationRepository,
                            IHolidayRepository holidayRepository,
                            IEmployeeRepository employeesRepository,
                            IAssetsMasterRepository assetsMasterRepository,
                            ILeaveRepository leaveRepository,
                            IUserLoginRepository userLoginRepository,
                            IProjectMasterRepository projectMasters,
                            IUserRegistrationRepository registrationRepository,
                            IRoleMasterRepository roleMaster,
                            IEmployeeRoleMappingRepository employeeRoleMappings,
                            IAttendanceRequestRepository attendanceRequestRepository,
                            IEmployeeAttendanceRepository employeeAttendanceRepository,
                            IEmployeePaymentInfoRepository employeePaymentInfo,
                            IEmployeeDocumentRepository employeementDocument,
                            IShiftRepository shiftRepository,
                            IEmailTemplateTypeMasterRepository emailTemplateTypeMaster,
                            IEmailTemplateMasterRepository emailTemplateMaster,
                            IEmailRepository emails,
                            ILeaveRequestRepository leaveRequest,
                            IResignationRepository resignation,
                            IAttendanceLogRepository attendanceLog,
                            ITimesheetRepository? timesheets = null,
                            ITimesheetEntryRepository? timesheetEntries = null,
                            IAssetRequestRepository? assetRequests = null,
                            IAssetRequestLogRepository? assetRequestLogs = null,
                            IEmailTriggerEventRepository? emailTriggerEvents = null,
                            IPermissionMasterRepository? permissionMaster = null,
                            IRolePermissionMappingRepository? rolePermissionMappings = null,
                            IUserPermissionMappingRepository? userPermissionMappings = null)
        {
            _dbContext = dbContext;
            Candidates = candidateRepository;
            Skills = skillRepository;
            Designations = designationRepository;
            Employees = employeesRepository;
            Holidays = holidayRepository;
            AssetsMasters = assetsMasterRepository;
            LeaveType = leaveRepository;
            UserLogins = userLoginRepository;
            ProjectMasters = projectMasters;
            UserRegistrations = registrationRepository;
            RoleMaster = roleMaster;
            EmployeeRoleMappings = employeeRoleMappings;
            AttendanceRequests = attendanceRequestRepository;
            EmployeeAttendance = employeeAttendanceRepository;
            EmployeePaymentInfo = employeePaymentInfo;
            EmployeementDocument = employeementDocument;
            ShiftRepository = shiftRepository;
            EmailTemplateTypeMaster = emailTemplateTypeMaster;
            EmailTemplateMaster = emailTemplateMaster;
            Email = emails;
            LeaveRequest = leaveRequest;
            Resignation = resignation;
            AttendanceLog = attendanceLog;
            Timesheets = timesheets ?? new TimesheetRepository(_dbContext);
            TimesheetEntries = timesheetEntries ?? new TimesheetEntryRepository(_dbContext);
            AssetRequests = assetRequests ?? new AssetRequestRepository(_dbContext);
            AssetRequestLogs = assetRequestLogs ?? new AssetRequestLogRepository(_dbContext);
            Notifications = new NotificationRepository(_dbContext);
            EmailTriggerEvents = emailTriggerEvents ?? new EmailTriggerEventRepository(_dbContext);
            PermissionMaster = permissionMaster ?? new PermissionMasterRepository(_dbContext);
            RolePermissionMappings = rolePermissionMappings ?? new RolePermissionMappingRepository(_dbContext);
            UserPermissionMappings = userPermissionMappings ?? new UserPermissionMappingRepository(_dbContext);
        }

        public int Save()
        {
            return _dbContext.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }

    }
}
