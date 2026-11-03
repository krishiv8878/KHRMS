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

            modelBuilder.Entity<EmployeeDailyWorkMode>()
                .HasIndex(e => new { e.EmployeeId, e.WorkDate })
                .IsUnique();

            // Workflow relationships and concurrency configurations
            modelBuilder.Entity<WorkflowDelegation>()
                .HasOne(d => d.FromEmployee)
                .WithMany()
                .HasForeignKey(d => d.FromEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowDelegation>()
                .HasOne(d => d.ToEmployee)
                .WithMany()
                .HasForeignKey(d => d.ToEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowInstance>()
                .HasOne(i => i.RequesterEmployee)
                .WithMany()
                .HasForeignKey(i => i.RequesterEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowInstance>()
                .HasOne(i => i.WorkflowDefinition)
                .WithMany(d => d.Instances)
                .HasForeignKey(i => i.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowInstanceStep>()
                .HasOne(s => s.ActionByEmployee)
                .WithMany()
                .HasForeignKey(s => s.ActionByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowInstanceStep>()
                .HasOne(s => s.WorkflowInstance)
                .WithMany(i => i.InstanceSteps)
                .HasForeignKey(s => s.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkflowAuditLog>()
                .HasOne(a => a.PerformedByEmployee)
                .WithMany()
                .HasForeignKey(a => a.PerformedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowAuditLog>()
                .HasOne(a => a.WorkflowInstance)
                .WithMany(i => i.AuditLogs)
                .HasForeignKey(a => a.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(e => new { e.ModuleName, e.EntityName, e.EntityId });
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.ActorEmployeeId);
            });

            // Checklist relationships
            modelBuilder.Entity<ChecklistTemplateTask>()
                .HasOne(t => t.Template)
                .WithMany(tpl => tpl.TemplateTasks)
                .HasForeignKey(t => t.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeChecklist>()
                .HasOne(c => c.Employee)
                .WithMany()
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChecklistTask>()
                .HasOne(t => t.EmployeeChecklist)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.EmployeeChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Time & Attendance Rules & Constraints (Phase 4)
            modelBuilder.Entity<EmployeeShiftAssignment>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeShiftAssignment>()
                .HasOne(s => s.Shift)
                .WithMany()
                .HasForeignKey(s => s.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeShiftAssignment>()
                .HasIndex(s => new { s.EmployeeId, s.EffectiveFromDate });

            modelBuilder.Entity<DailyAttendanceRecord>()
                .HasOne(d => d.Employee)
                .WithMany()
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyAttendanceRecord>()
                .HasOne(d => d.Shift)
                .WithMany()
                .HasForeignKey(d => d.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DailyAttendanceRecord>()
                .HasIndex(d => new { d.EmployeeId, d.AttendanceDate })
                .IsUnique();

            modelBuilder.Entity<DailyAttendanceRecord>()
                .HasIndex(d => new { d.AttendanceDate, d.Status });

            modelBuilder.Entity<AttendanceLock>()
                .HasIndex(a => new { a.Month, a.Year })
                .IsUnique();

            // Employee Tenures & Universal Identity Tracking
            modelBuilder.Entity<Employee>()
                .HasOne<Employee>()
                .WithMany()
                .HasForeignKey(e => e.PreviousEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.PreviousEmployeeId);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => new { e.PanNumber, e.UanNumber, e.PersonalEmail });

            modelBuilder.Entity<SystemSetting>()
                .HasIndex(s => s.SettingKey)
                .IsUnique();

            // Payroll Unique Indexes & Integrity (P1: DAT-02)
            modelBuilder.Entity<PayRun>()
                .HasIndex(p => new { p.Month, p.Year })
                .IsUnique();

            modelBuilder.Entity<Payslip>()
                .HasIndex(p => new { p.EmployeeId, p.Month, p.Year })
                .IsUnique();

            modelBuilder.Entity<SystemSetting>().HasData(new SystemSetting
            {
                Id = 1,
                SettingKey = "ReusePreviousEmployeeCodeOnRehire",
                SettingValue = "false",
                Description = "Determines whether rehired employees retain their previous employee code or receive a new one.",
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // Phase 5: Leave Ledger & Entitlement Configurations
            modelBuilder.Entity<LeaveLedger>(entity =>
            {
                entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId });
                entity.HasIndex(e => new { e.EmployeeId, e.Period });
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.LeaveType).WithMany().HasForeignKey(e => e.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.LeaveRequest).WithMany().HasForeignKey(e => e.LeaveRequestId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<LeaveEntitlement>(entity =>
            {
                entity.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.IsExpired });
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.LeaveType).WithMany().HasForeignKey(e => e.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LopConversionRequest>(entity =>
            {
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.OriginalLeaveRequest).WithMany().HasForeignKey(e => e.OriginalLeaveRequestId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TargetLeaveType).WithMany().HasForeignKey(e => e.TargetLeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LeaveEncashmentRequest>(entity =>
            {
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.LeaveType).WithMany().HasForeignKey(e => e.LeaveTypeId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CompOffGrantRequest>(entity =>
            {
                entity.HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            });

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
        public DbSet<EmployeeLoan> EmployeeLoans { get; set; }
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

        // Work Mode & Environment
        public DbSet<EmployeeDailyWorkMode> EmployeeDailyWorkModes { get; set; }

        // Configurable Approval Workflow System
        public DbSet<WorkflowDefinition> WorkflowDefinitions { get; set; }
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }
        public DbSet<WorkflowRoutingRule> WorkflowRoutingRules { get; set; }
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; }
        public DbSet<WorkflowInstanceStep> WorkflowInstanceSteps { get; set; }
        public DbSet<WorkflowDelegation> WorkflowDelegations { get; set; }
        public DbSet<WorkflowAuditLog> WorkflowAuditLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Employee Lifecycle & Checklists
        public DbSet<ChecklistTemplate> ChecklistTemplates { get; set; }
        public DbSet<ChecklistTemplateTask> ChecklistTemplateTasks { get; set; }
        public DbSet<EmployeeChecklist> EmployeeChecklists { get; set; }
        public DbSet<ChecklistTask> ChecklistTasks { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        // Time & Attendance Rules & Computation (Phase 4)
        public DbSet<EmployeeShiftAssignment> EmployeeShiftAssignments { get; set; }
        public DbSet<DailyAttendanceRecord> DailyAttendanceRecords { get; set; }
        public DbSet<AttendanceLock> AttendanceLocks { get; set; }

        // Leave Management, Ledger, Accrual & Rollover (Phase 5)
        public DbSet<LeaveLedger> LeaveLedgers { get; set; }
        public DbSet<LeaveEntitlement> LeaveEntitlements { get; set; }
        public DbSet<LopConversionRequest> LopConversionRequests { get; set; }
        public DbSet<LeaveEncashmentRequest> LeaveEncashmentRequests { get; set; }
        public DbSet<CompOffGrantRequest> CompOffGrantRequests { get; set; }

        // Company Policy Engine & Helpdesk (Phase 7)
        public DbSet<CompanyPolicy> CompanyPolicies { get; set; }
        public DbSet<PolicyAcknowledgement> PolicyAcknowledgements { get; set; }
        public DbSet<HelpdeskTicket> HelpdeskTickets { get; set; }
    }
}
