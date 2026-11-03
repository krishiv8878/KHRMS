using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KHRMS.Services
{
    public class LwdReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12);

        public LwdReminderBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("LwdReminderBackgroundService started. Monitoring Last Working Day readiness reminders.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessLwdRemindersAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred during LWD reminder background processing.");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            Log.Information("LwdReminderBackgroundService stopped.");
        }

        private async Task ProcessLwdRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var emailService = scope.ServiceProvider.GetService<ISendEmailService>();
            var checklistService = scope.ServiceProvider.GetService<IChecklistService>();

            var today = DateTime.Today;

            // Fetch active resignations that have not been finalized
            var activeResignations = (await unitOfWork.Resignation.GetAll())
                .Where(r => !r.IsDeleted && r.IsActive &&
                            r.ResignationStatus != "Exited" &&
                            r.ResignationStatus != "Withdrawn" &&
                            r.ResignationStatus != "Rejected")
                .ToList();

            if (!activeResignations.Any()) return;

            var allEmployees = (await unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);

            // Find HR / Admin email recipients
            var hrEmployees = allEmployees.Values
                .Where(e => !e.IsDeleted && e.IsActive && !string.IsNullOrWhiteSpace(e.EmailAddress))
                .Where(e => e.Designation?.Contains("HR", StringComparison.OrdinalIgnoreCase) == true ||
                            e.Branch?.Contains("Human Resources", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            var hrEmails = hrEmployees.Select(h => h.EmailAddress!).Distinct().ToList();
            if (!hrEmails.Any())
            {
                // Fallback to primary admin
                var admin = allEmployees.Values.FirstOrDefault(e => e.IsActive && !e.IsDeleted && !string.IsNullOrWhiteSpace(e.EmailAddress));
                if (admin?.EmailAddress != null) hrEmails.Add(admin.EmailAddress);
            }

            foreach (var res in activeResignations)
            {
                var targetLwd = res.ApprovedLwd ?? res.ProposedLwd ?? res.RequestedLwd;
                if (!targetLwd.HasValue) continue;

                var lwdDate = targetLwd.Value.Date;
                var daysRemaining = (int)(lwdDate - today).TotalDays;

                if (!allEmployees.TryGetValue(res.EmployeeId, out var emp)) continue;

                string employeeName = $"{emp.FirstName} {emp.LastName}".Trim();
                string formattedLwd = lwdDate.ToString("dd MMM yyyy");

                // Collect readiness metrics
                int pendingAssets = 0;
                if (checklistService != null)
                {
                    try
                    {
                        var assetReport = await checklistService.VerifyAssetReturnsAsync(emp.Id);
                        pendingAssets = assetReport.PendingAssets;
                    }
                    catch { }
                }

                int pendingTasks = 0;
                var checklist = (await unitOfWork.EmployeeChecklists.GetAll())
                    .FirstOrDefault(c => c.EmployeeId == emp.Id && c.ChecklistType == "Offboarding" && !c.IsDeleted);
                if (checklist != null)
                {
                    var tasks = (await unitOfWork.ChecklistTasks.GetAll())
                        .Where(t => t.EmployeeChecklistId == checklist.Id && !t.IsDeleted && t.IsMandatory)
                        .ToList();
                    pendingTasks = tasks.Count(t => t.Status != "Completed" && t.Status != "Waived");
                }

                string fnfStatus = res.FnFStatus ?? "Pending";

                string subject = string.Empty;
                string urgencyNotice = string.Empty;

                if (daysRemaining == 7)
                {
                    subject = $"[HR Action: T-7 Days] Exit Readiness Summary for {employeeName} (LWD: {formattedLwd})";
                    urgencyNotice = "This is a 7-day advance notice to review the employee's offboarding readiness.";
                }
                else if (daysRemaining == 1)
                {
                    subject = $"[HR URGENT: T-1 Day] Exit Readiness Check for {employeeName} (LWD: Tomorrow, {formattedLwd})";
                    urgencyNotice = "Final working day is tomorrow. Please ensure all assets, clearances, and FnF calculations are ready.";
                }
                else if (daysRemaining == 0)
                {
                    subject = $"[HR ACTION TODAY] Last Working Day for {employeeName} ({formattedLwd})";
                    urgencyNotice = "Today is the employee's Last Working Day. Complete clearances and finalize exit when ready.";
                }
                else if (daysRemaining < 0)
                {
                    int overdueDays = Math.Abs(daysRemaining);
                    subject = $"[HR OVERDUE: {overdueDays}d] Exit Past LWD Not Finalized: {employeeName} (LWD was {formattedLwd})";
                    urgencyNotice = $"This exit is {overdueDays} day(s) past the approved Last Working Day and has NOT been finalized by HR.";
                }
                else
                {
                    continue; // Not a notification milestone
                }

                if (emailService != null && hrEmails.Any())
                {
                    var bodyDict = new Dictionary<string, string>
                    {
                        { "EmployeeName", employeeName },
                        { "Designation", emp.Designation ?? "N/A" },
                        { "Department", emp.Branch ?? "N/A" },
                        { "LwdDate", formattedLwd },
                        { "PendingAssetsCount", pendingAssets.ToString() },
                        { "PendingTasksCount", pendingTasks.ToString() },
                        { "FnFStatus", fnfStatus },
                        { "UrgencyNotice", urgencyNotice }
                    };

                    foreach (var hrEmail in hrEmails)
                    {
                        try
                        {
                            await emailService.SendTemplateEmailAsync(hrEmail, subject, bodyDict, "lwd_readiness_reminder");
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Failed sending LWD reminder email to {HrEmail} for employee #{EmpId}", hrEmail, emp.Id);
                        }
                    }
                }
            }
        }
    }
}
