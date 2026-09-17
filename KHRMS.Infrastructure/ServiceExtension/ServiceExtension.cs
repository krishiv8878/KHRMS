using KHRMS.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Infrastructure.Repositories;

namespace KHRMS.Infrastructure
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddDIServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<KHRMSContextClass>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("HRMSContext"));
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICandidateRepository, CandidateRepository>();
            services.AddScoped<ISkillRepository, SkillRepository>();
            services.AddScoped<IDesignationRepository, DesignationRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IHolidayRepository, HolidayRepository>();
            services.AddScoped<IAssetsMasterRepository, AssetsMasterRepository>();
            services.AddScoped<ILeaveRepository, LeaveRepository>();
            services.AddScoped<IUserLoginRepository, UserLoginRepository>();
            services.AddScoped<IProjectMasterRepository, ProjectMasterRepository>();
            services.AddScoped<IUserRegistrationRepository, UserRegistrationRepository>();
            services.AddScoped<IEmployeeRoleMappingRepository, EmployeeRoleMappingRepository>();
            services.AddScoped<IRoleMasterRepository, RoleMasterRepository>();
            services.AddScoped<IAttendanceRequestRepository, AttendanceRequestRepository>();
            services.AddScoped<IEmployeeAttendanceRepository, EmployeeAttendanceRepository>();
            services.AddScoped<IEmployeePaymentInfoRepository, EmployeePaymentInfoRepository>();
            services.AddScoped<IEmployeeDocumentRepository, EmployeeDocumentRepository>();
            services.AddScoped<IShiftRepository, ShiftRepository>();
            services.AddScoped<IEmailTemplateTypeMasterRepository, EmailTemplateTypeMasterRepository>();
            services.AddScoped<IEmailTemplateMasterRepository, EmailTemplatesMasterRepository>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
            services.AddScoped<IResignationRepository, ResignationRepository>();
            services.AddScoped<IAttendanceLogRepository, AttendanceLogRepository>();
            services.AddScoped<ITimesheetRepository, TimesheetRepository>();
            services.AddScoped<ITimesheetEntryRepository, TimesheetEntryRepository>();
            return services;
        }
    }
}
