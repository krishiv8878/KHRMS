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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Skill>().HasIndex(x => x.SkillName).IsUnique();


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

            modelBuilder.Entity<Employee>()
                .Property(e => e.ProjectIds)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<long>>(v, (JsonSerializerOptions)null)
                 ).HasColumnType("nvarchar(max)");

            // Explicitly ignore IsDeleted on Email if Emails table does not have IsDeleted column
            modelBuilder.Entity<Email>().Ignore(e => e.IsDeleted);

            // Global Query Filter: Automatically exclude soft-deleted records across all KHRMSBase entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
            {
                if (entityType.ClrType != null && typeof(KHRMSBase).IsAssignableFrom(entityType.ClrType))
                {
                    // Skip entities where IsDeleted is not mapped in the database table (e.g. Email)
                    if (entityType.ClrType == typeof(Email))
                    {
                        continue;
                    }

                    var isDeletedProperty = entityType.FindProperty(nameof(KHRMSBase.IsDeleted));
                    if (isDeletedProperty != null)
                    {
                        var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                        var property = System.Linq.Expressions.Expression.Property(
                            System.Linq.Expressions.Expression.Convert(parameter, typeof(KHRMSBase)), 
                            nameof(KHRMSBase.IsDeleted)
                        );
                        var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                        var comparison = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                        var lambda = System.Linq.Expressions.Expression.Lambda(comparison, parameter);

                        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                    }
                }
            }
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
        public DbSet<Timesheet> Timesheets { get; set; }
        public DbSet<TimesheetEntry> TimesheetEntries { get; set; }
        public DbSet<AssetRequest> AssetRequests { get; set; }
        public DbSet<AssetRequestLog> AssetRequestLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EmailTriggerEvent> EmailTriggerEvents { get; set; }
        public DbSet<PermissionMaster> PermissionMasters { get; set; }
        public DbSet<RolePermissionMapping> RolePermissionMappings { get; set; }
        public DbSet<UserPermissionMapping> UserPermissionMappings { get; set; }

        // Payroll Management
        public DbSet<SalaryComponent> SalaryComponents { get; set; }
        public DbSet<SalaryStructure> SalaryStructures { get; set; }
        public DbSet<EmployeeSalaryAssignment> EmployeeSalaryAssignments { get; set; }
        public DbSet<PayRun> PayRuns { get; set; }
        public DbSet<PayRunEmployeeDetail> PayRunEmployeeDetails { get; set; }
        public DbSet<Payslip> Payslips { get; set; }
        public DbSet<ReimbursementClaim> ReimbursementClaims { get; set; }
        public DbSet<FullAndFinalSettlement> FullAndFinalSettlements { get; set; }

        // Performance Management System (PMS)
        public DbSet<PmsGoal> PmsGoals { get; set; }
        public DbSet<PmsKeyResult> PmsKeyResults { get; set; }
        public DbSet<PmsGoalCheckIn> PmsGoalCheckIns { get; set; }
        public DbSet<PmsReviewCycle> PmsReviewCycles { get; set; }
        public DbSet<PmsAppraisal> PmsAppraisals { get; set; }
        public DbSet<PmsFeedback> PmsFeedbacks { get; set; }
        public DbSet<PmsOneOnOne> PmsOneOnOnes { get; set; }
        public DbSet<PmsPip> PmsPips { get; set; }

        // Expense & Travel Management
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<ExpenseReport> ExpenseReports { get; set; }
        public DbSet<ExpenseItem> ExpenseItems { get; set; }
        public DbSet<ExpenseMileage> ExpenseMileages { get; set; }
        public DbSet<ExpenseAdvance> ExpenseAdvances { get; set; }

        // Recruitment & ATS Management
        public DbSet<JobRequisition> JobRequisitions { get; set; }
        public DbSet<JobInterview> JobInterviews { get; set; }
        public DbSet<JobOffer> JobOffers { get; set; }
    }
}
