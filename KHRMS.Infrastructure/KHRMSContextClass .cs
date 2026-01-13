using System.Text.Json;
using KHRMS.Core;
using KHRMS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Infrastructure
{
    public class KHRMSContextClass : DbContext
    {
        public KHRMSContextClass(DbContextOptions<KHRMSContextClass> contextOptions) : base(contextOptions)
        {
        }

        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Holiday> Holidays { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<AssetsMaster> AssetsMasters { get; set; }
        public DbSet<LeaveType> LeaveType { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }

        public DbSet<Resignation> Resignations { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set UserLogin ID to start from 1001
            modelBuilder.Entity<UserLogin>(entity =>
            {
                entity.Property(e => e.Id)
                      .UseIdentityColumn(seed: 1001, increment: 1);
            });
            modelBuilder.Entity<Employee>()
                .Property(e => e.SkillIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<long>>(v, (JsonSerializerOptions)null)
                 ).HasColumnType("nvarchar(max)");
        }
        public DbSet<ProjectMaster> ProjectMasters { get; set; }
        public DbSet<UserRegistration> UserRegistrations { get; set; }
        public DbSet<RoleMaster> RoleMasters { get; set; }
        public DbSet<EmployeeRoleMapping> EmployeeRoleMappings { get; set; }
        public DbSet<AttendanceRequest> AttendanceRequests { get; set; }
        public DbSet<EmployeeAttendance> EmployeeAttendances { get; set; }
        public DbSet<EmployeePaymentInfo> EmployeePaymentInfos { get; set; }
        public DbSet<EmployeeDocumentInfo> Employeementdocument { get; set; }
        public DbSet<ShiftMaster> ShiftMasters { get; set; } 
        public DbSet<EmailTemplateTypeMaster> EmailTemplateTypes { get; set; }
        public DbSet<EmailTemplatesMaster> EmailTemplatesMasters { get; set;}
        public DbSet<Email> Emails { get; set; }
        public DbSet<LeaveRequest> leaveRequests { get; set; }
        public DbSet<Resignation> Resignation { get; set; }
        public DbSet<AttendanceLog> AttendanceLog { get; set; }


    }
}
