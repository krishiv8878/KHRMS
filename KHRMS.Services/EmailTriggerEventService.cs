using System.Text.Json;
using KHRMS.Core;
using KHRMS.Core.Constants;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using Serilog;

namespace KHRMS.Services
{
    public class EmailTriggerEventService : IEmailTriggerEventService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmailTriggerEventService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private static readonly List<EmailTriggerEvent> DefaultEvents = new()
        {
            new EmailTriggerEvent
            {
                Id = 1,
                EventCode = EmailEventConstants.OnboardingCredentials,
                EventName = "Employee Onboarding & Credentials",
                Category = "Authentication & Onboarding",
                Description = "Sent when a candidate is hired or a new employee account is created with login credentials.",
                DefaultSubject = "Welcome to KHRMS – Your Account Access Details",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Full Name", sample = "Alex Morgan" },
                    new { key = "USERNAME", description = "Login Username / Email", sample = "alex.morgan@company.com" },
                    new { key = "TEMP_PASSWORD", description = "Initial Temporary Password", sample = "Pass#2026!" },
                    new { key = "URL", description = "HRMS Portal Login URL", sample = "https://khrms.internal/login" },
                    new { key = "COMPANY_NAME", description = "Company Name", sample = "KHRMS Enterprise" },
                    new { key = "SUPPORT_EMAIL", description = "IT / HR Support Email", sample = "support@company.com" },
                    new { key = "HR_OR_IT_TEAM_NAME", description = "Support Team Name", sample = "People Operations" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 2,
                EventCode = EmailEventConstants.ForgotPassword,
                EventName = "Password Reset Link",
                Category = "Authentication & Onboarding",
                Description = "Sent when a user requests a password recovery link.",
                DefaultSubject = "Reset Password Request for KHRMS Account",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "URL", description = "Password Reset Link", sample = "https://khrms.internal/reset-password?token=xyz" },
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "SUPPORT_EMAIL", description = "Support Email", sample = "support@company.com" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 3,
                EventCode = EmailEventConstants.LeaveRequestSubmitted,
                EventName = "New Leave Application (To Manager)",
                Category = "Leave & Attendance",
                Description = "Sent to reporting manager when an employee applies for time off.",
                DefaultSubject = "New Leave Request from {{EmployeeName}}",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "ManagerName", description = "Reporting Manager Name", sample = "Sarah Jenkins" },
                    new { key = "EmployeeName", description = "Requester Employee Name", sample = "Alex Morgan" },
                    new { key = "LeaveType", description = "Leave Category (e.g. Sick, Casual)", sample = "Casual Leave" },
                    new { key = "StartDate", description = "First Day of Leave", sample = "25 Sep 2026" },
                    new { key = "EndDate", description = "Last Day of Leave", sample = "27 Sep 2026" },
                    new { key = "LeaveDescription", description = "Reason Submitted", sample = "Attending family event" },
                    new { key = "ActionUrl", description = "Review in Approvals Portal", sample = "https://khrms.internal/index/request" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 4,
                EventCode = EmailEventConstants.LeaveApproved,
                EventName = "Leave Request Approved (To Employee)",
                Category = "Leave & Attendance",
                Description = "Sent to employee when their time-off request is approved by manager.",
                DefaultSubject = "Your Leave Request Has Been Approved",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "ManagerName", description = "Approving Manager Name", sample = "Sarah Jenkins" },
                    new { key = "LeaveType", description = "Leave Type", sample = "Casual Leave" },
                    new { key = "StartDate", description = "Start Date", sample = "25 Sep 2026" },
                    new { key = "EndDate", description = "End Date", sample = "27 Sep 2026" },
                    new { key = "Duration", description = "Duration / Total Days", sample = "3 Days" },
                    new { key = "Reason", description = "Approval Note / Reason", sample = "Approved by manager" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 5,
                EventCode = EmailEventConstants.LeaveRejected,
                EventName = "Leave Request Declined (To Employee)",
                Category = "Leave & Attendance",
                Description = "Sent to employee when their time-off request is rejected.",
                DefaultSubject = "Your Leave Request Has Been Declined",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "ManagerName", description = "Manager Name", sample = "Sarah Jenkins" },
                    new { key = "LeaveType", description = "Leave Type", sample = "Casual Leave" },
                    new { key = "StartDate", description = "Start Date", sample = "25 Sep 2026" },
                    new { key = "EndDate", description = "End Date", sample = "27 Sep 2026" },
                    new { key = "Reason", description = "Rejection Reason", sample = "High project delivery volume during this period" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 6,
                EventCode = EmailEventConstants.RegularizationApproved,
                EventName = "Attendance Adjustment Approved",
                Category = "Leave & Attendance",
                Description = "Sent when employee attendance clock-in adjustment is approved.",
                DefaultSubject = "Your Attendance Adjustment Request Has Been Approved",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "ManagerName", description = "Manager Name", sample = "Sarah Jenkins" },
                    new { key = "Date", description = "Regularization Date", sample = "2026-09-20" },
                    new { key = "InTime", description = "Adjusted Clock-In Time", sample = "09:00 AM" },
                    new { key = "OutTime", description = "Adjusted Clock-Out Time", sample = "06:00 PM" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 7,
                EventCode = EmailEventConstants.RegularizationRejected,
                EventName = "Attendance Adjustment Declined",
                Category = "Leave & Attendance",
                Description = "Sent when employee attendance regularization request is rejected.",
                DefaultSubject = "Your Attendance Adjustment Request Was Declined",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "ManagerName", description = "Manager Name", sample = "Sarah Jenkins" },
                    new { key = "Date", description = "Adjustment Date", sample = "2026-09-20" },
                    new { key = "Reason", description = "Declined Reason", sample = "Discrepancy with entry badge records" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 8,
                EventCode = EmailEventConstants.TimesheetSubmitted,
                EventName = "Timesheet Submitted for Review",
                Category = "Timesheets",
                Description = "Sent to manager when employee submits their weekly log of hours.",
                DefaultSubject = "Weekly Timesheet Submitted by {{EmployeeName}}",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "ManagerName", description = "Manager Name", sample = "Sarah Jenkins" },
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "Period", description = "Work Week Period", sample = "15 Sep - 21 Sep 2026" },
                    new { key = "TotalHours", description = "Total Logged Hours", sample = "40.0 Hrs" },
                    new { key = "ActionUrl", description = "Review Timesheet Link", sample = "https://khrms.internal/index/request" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 9,
                EventCode = EmailEventConstants.TimesheetApproved,
                EventName = "Timesheet Approved",
                Category = "Timesheets",
                Description = "Sent to employee when weekly timesheet is approved by manager.",
                DefaultSubject = "Your Timesheet Has Been Approved",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "ManagerName", description = "Manager Name", sample = "Sarah Jenkins" },
                    new { key = "Period", description = "Timesheet Period", sample = "15 Sep - 21 Sep 2026" },
                    new { key = "TotalHours", description = "Total Approved Hours", sample = "40.0 Hrs" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 10,
                EventCode = EmailEventConstants.TimesheetRejected,
                EventName = "Timesheet Revision Requested",
                Category = "Timesheets",
                Description = "Sent to employee when weekly timesheet is rejected with revision note.",
                DefaultSubject = "Timesheet Revision Required",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "ManagerName", description = "Manager Name", sample = "Sarah Jenkins" },
                    new { key = "Period", description = "Timesheet Period", sample = "15 Sep - 21 Sep 2026" },
                    new { key = "Reason", description = "Revision Comments", sample = "Please update project task allocation on Wednesday" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 11,
                EventCode = EmailEventConstants.AssetAllocated,
                EventName = "Corporate Asset Allocated",
                Category = "Hardware Assets",
                Description = "Sent to employee upon hardware laptop/device assignment.",
                DefaultSubject = "Corporate Hardware Asset Allocated",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "EmployeeId", description = "Employee ID", sample = "EMP-018" },
                    new { key = "Department", description = "Department", sample = "Engineering" },
                    new { key = "AssetName", description = "Asset Model", sample = "Dell Latitude 5540" },
                    new { key = "SerialNumber", description = "Asset Tag / S/N", sample = "DL-98234-K" },
                    new { key = "ActionUrl", description = "Acknowledge in Portal", sample = "https://khrms.internal/index/assets" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 12,
                EventCode = EmailEventConstants.PayslipPublished,
                EventName = "Monthly Payslip Generated",
                Category = "Payroll & Banking",
                Description = "Sent when monthly salary statement is finalized and ready for download.",
                DefaultSubject = "Your Monthly Salary Statement is Available",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "EmployeeId", description = "Employee ID", sample = "EMP-018" },
                    new { key = "Designation", description = "Job Title", sample = "Senior Software Engineer" },
                    new { key = "MonthYear", description = "Payroll Cycle", sample = "September 2026" },
                    new { key = "ActionUrl", description = "Download Payslip Link", sample = "https://khrms.internal/index/paymentinfo" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 13,
                EventCode = EmailEventConstants.ResignationSubmitted,
                EventName = "Resignation Notice Submitted",
                Category = "Resignation & Exit",
                Description = "Sent to manager and HR when an employee submits separation notice.",
                DefaultSubject = "Resignation Notice Submitted by {{EmployeeName}}",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "ManagerName", description = "Manager Name", sample = "Sarah Jenkins" },
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "ResignationDate", description = "Intended Last Date", sample = "31 Oct 2026" },
                    new { key = "NoticePeriod", description = "Agreed Notice Period", sample = "60 Days" },
                    new { key = "Reason", description = "Resignation Reason", sample = "Pursuing external opportunities" }
                }),
                IsEnabled = true
            },
            new EmailTriggerEvent
            {
                Id = 14,
                EventCode = EmailEventConstants.ResignationApproved,
                EventName = "Resignation Notice Accepted",
                Category = "Resignation & Exit",
                Description = "Sent to employee upon formal acceptance of resignation by management.",
                DefaultSubject = "Formal Acceptance of Resignation",
                AvailableVariablesJson = JsonSerializer.Serialize(new[]
                {
                    new { key = "EmployeeName", description = "Employee Name", sample = "Alex Morgan" },
                    new { key = "ManagerName", description = "Manager Name", sample = "Sarah Jenkins" },
                    new { key = "ResignationDate", description = "Relieving Date", sample = "31 Oct 2026" },
                    new { key = "NoticePeriod", description = "Notice Period", sample = "60 Days" }
                }),
                IsEnabled = true
            }
        };

        public async Task<IEnumerable<EmailTriggerEvent>> GetAllAsync()
        {
            try
            {
                var dbEvents = (await _unitOfWork.EmailTriggerEvents.GetAll()).ToList();
                if (dbEvents.Any())
                {
                    return dbEvents;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not query EmailTriggerEvents table (table may not be migrated yet). Falling back to in-memory registry.");
            }

            return DefaultEvents;
        }

        public async Task<EmailTriggerEvent?> GetByEventCodeAsync(string eventCode)
        {
            try
            {
                var list = await GetAllAsync();
                return list.FirstOrDefault(e => string.Equals(e.EventCode, eventCode, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return DefaultEvents.FirstOrDefault(e => string.Equals(e.EventCode, eventCode, StringComparison.OrdinalIgnoreCase));
            }
        }

        public async Task<bool> UpdateBindingAsync(long id, long? activeTemplateId, bool isEnabled, string? defaultSubject = null)
        {
            try
            {
                var existing = await _unitOfWork.EmailTriggerEvents.GetById(id);
                if (existing != null)
                {
                    existing.ActiveTemplateId = activeTemplateId;
                    existing.IsEnabled = isEnabled;
                    if (!string.IsNullOrWhiteSpace(defaultSubject))
                    {
                        existing.DefaultSubject = defaultSubject;
                    }
                    existing.UpdatedDate = DateTime.Now;

                    _unitOfWork.EmailTriggerEvents.Update(existing);
                    return _unitOfWork.Save() > 0;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not update EmailTriggerEvent in database.");
            }

            // In-memory fallback if not yet migrated
            var inMem = DefaultEvents.FirstOrDefault(e => e.Id == id);
            if (inMem != null)
            {
                inMem.ActiveTemplateId = activeTemplateId;
                inMem.IsEnabled = isEnabled;
                if (!string.IsNullOrWhiteSpace(defaultSubject))
                {
                    inMem.DefaultSubject = defaultSubject;
                }
                return true;
            }

            return false;
        }

        public async Task<EmailTemplatesMaster?> GetActiveTemplateForEventAsync(string eventCodeOrLegacyName)
        {
            try
            {
                var allEvents = await GetAllAsync();
                var matched = allEvents.FirstOrDefault(e =>
                    string.Equals(e.EventCode, eventCodeOrLegacyName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(e.EventName, eventCodeOrLegacyName, StringComparison.OrdinalIgnoreCase));

                if (matched != null && matched.ActiveTemplateId.HasValue && matched.ActiveTemplateId.Value > 0)
                {
                    var template = await _unitOfWork.EmailTemplateMaster.GetById(matched.ActiveTemplateId.Value);
                    if (template != null) return template;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error getting active template for event {Event}", eventCodeOrLegacyName);
            }

            // Fallback: search by template type name
            try
            {
                var allTypes = await _unitOfWork.EmailTemplateTypeMaster.GetAll();
                var matchedType = allTypes.FirstOrDefault(t =>
                    string.Equals(t.TemplateType, eventCodeOrLegacyName, StringComparison.OrdinalIgnoreCase) ||
                    t.TemplateType.Contains(eventCodeOrLegacyName, StringComparison.OrdinalIgnoreCase));

                if (matchedType != null)
                {
                    var templates = await _unitOfWork.EmailTemplateMaster.GetAll();
                    return templates.FirstOrDefault(t => t.EmailTemplateTypeId == matchedType.Id);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error getting template by legacy type name");
            }

            return null;
        }

        public async Task<bool> SeedDefaultEventsAsync()
        {
            try
            {
                var existing = (await _unitOfWork.EmailTriggerEvents.GetAll()).ToList();
                if (!existing.Any())
                {
                    foreach (var evt in DefaultEvents)
                    {
                        var newEvt = new EmailTriggerEvent
                        {
                            EventCode = evt.EventCode,
                            EventName = evt.EventName,
                            Category = evt.Category,
                            Description = evt.Description,
                            AvailableVariablesJson = evt.AvailableVariablesJson,
                            DefaultSubject = evt.DefaultSubject,
                            IsEnabled = evt.IsEnabled,
                            CreatedBy = 1,
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            IsActive = true
                        };
                        await _unitOfWork.EmailTriggerEvents.Add(newEvt);
                    }
                    return _unitOfWork.Save() > 0;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not seed default events (table might not exist yet).");
            }
            return false;
        }
    }
}
