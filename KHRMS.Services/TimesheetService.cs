using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Services
{
    public class TimesheetService : ITimesheetService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TimesheetService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TimesheetViewDTO> GetPeriodTimesheetAsync(long employeeId, DateTime startDate, DateTime endDate, long? timesheetId = null)
        {
            var start = startDate.Date;
            var end = endDate.Date;

            var employee = await _unitOfWork.Employees.GetById(employeeId);
            var employeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : "Employee";
            var managerId = employee?.ManagerId;
            string? managerName = null;
            if (managerId.HasValue && managerId.Value > 0)
            {
                var mgr = await _unitOfWork.Employees.GetById(managerId.Value);
                managerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : null;
            }

            // Find Timesheet for this period
            Timesheet? timesheet = null;
            if (timesheetId.HasValue && timesheetId.Value > 0)
            {
                timesheet = await _unitOfWork.Timesheets.Query()
                    .FirstOrDefaultAsync(t => !t.IsDeleted && t.Id == timesheetId.Value);
            }
            if (timesheet == null)
            {
                timesheet = await _unitOfWork.Timesheets.Query()
                    .Where(t => !t.IsDeleted && t.EmployeeId == employeeId && t.StartDate.Date == start && t.EndDate.Date == end)
                    .OrderByDescending(t => t.Id)
                    .FirstOrDefaultAsync();
            }

            // Find all Entries for this employee in date range
            var allEntries = await _unitOfWork.TimesheetEntries.Query()
                .Where(e => e.EmployeeId == employeeId && ((e.EntryDate.Date >= start && e.EntryDate.Date <= end) || (timesheet != null && e.TimesheetId == timesheet.Id)))
                .ToListAsync();

            List<TimesheetEntry> entries;
            if (timesheet != null && (timesheetId.HasValue || timesheet.Status == "Approved" || timesheet.Status == "Submitted"))
            {
                // Viewing a specific timesheet instance (e.g. from manager approval list or review modal)
                // OR an approved/submitted locked timesheet
                if (timesheet.Status == "Rejected")
                {
                    // For a REJECTED timesheet:
                    // Any entry tagged with this timesheet's ID belonged to it when submitted/rejected,
                    // EVEN IF the employee later soft-deleted it while editing their draft for resubmission!
                    var tagged = allEntries.Where(e => e.TimesheetId == timesheet.Id).ToList();
                    if (tagged.Any())
                    {
                        entries = tagged;
                    }
                    else
                    {
                        // Fallback for older historical records before TimesheetId tagging:
                        var cutoff = timesheet.ActionDate ?? timesheet.SubmittedDate ?? timesheet.UpdatedDate;
                        entries = allEntries
                            .Where(e => e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end)
                            .Where(e => (!e.CreatedDate.HasValue || e.CreatedDate.Value <= cutoff.AddMinutes(2)) &&
                                        (!e.IsDeleted || e.UpdatedDate > cutoff.AddMinutes(2)))
                            .ToList();
                    }
                }
                else
                {
                    // Approved or Submitted: take active entries linked to this timesheet
                    var tagged = allEntries.Where(e => !e.IsDeleted && e.TimesheetId == timesheet.Id).ToList();
                    entries = tagged.Any() ? tagged : allEntries.Where(e => !e.IsDeleted && e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end).ToList();
                }
            }
            else if (timesheet != null && timesheet.Status == "Rejected" && !timesheetId.HasValue)
            {
                // Employee is visiting their own timesheet page to edit after rejection.
                // Check if working draft copies (TimesheetId == null) exist:
                var workingDrafts = allEntries
                    .Where(e => !e.IsDeleted && e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end && e.TimesheetId == null)
                    .ToList();

                if (workingDrafts.Any())
                {
                    entries = workingDrafts;
                }
                else
                {
                    // Clones don't exist yet for this rejected timesheet - clone the rejected entries into working draft now!
                    var historical = allEntries.Where(e => e.TimesheetId == timesheet.Id && !e.IsDeleted).ToList();
                    if (!historical.Any())
                    {
                        historical = allEntries
                            .Where(e => !e.IsDeleted && e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end)
                            .ToList();
                    }

                    var newWorkingList = new List<TimesheetEntry>();
                    foreach (var h in historical)
                    {
                        var clone = new TimesheetEntry
                        {
                            TimesheetId = null, // working draft
                            EmployeeId = h.EmployeeId,
                            ProjectId = h.ProjectId,
                            EntryDate = h.EntryDate,
                            TaskDescription = h.TaskDescription,
                            Hours = h.Hours,
                            ClockInRef = h.ClockInRef,
                            ClockOutRef = h.ClockOutRef,
                            IsRegularized = h.IsRegularized,
                            CreatedBy = h.CreatedBy,
                            CreatedDate = h.CreatedDate,
                            UpdatedBy = (int)employeeId,
                            UpdatedDate = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false
                        };
                        await _unitOfWork.TimesheetEntries.Add(clone);
                        newWorkingList.Add(clone);
                    }
                    if (newWorkingList.Any())
                    {
                        _unitOfWork.Save();
                    }
                    entries = newWorkingList;
                }
            }
            else
            {
                // Draft / unsubmitted period
                entries = allEntries
                    .Where(e => !e.IsDeleted && e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end && e.TimesheetId == null)
                    .ToList();
            }

            // Projects map
            var allProjects = await _unitOfWork.ProjectMasters.GetAll();
            var projectDict = allProjects.ToDictionary(p => p.Id, p => p.ProjectName ?? "Project");

            // Attendance records in range
            var attendanceList = await _unitOfWork.EmployeeAttendance.Query()
                .Where(a => !a.IsDeleted && a.EmployeeId == employeeId)
                .ToListAsync();

            // Attendance requests (regularizations) in range
            var empAttRequests = await _unitOfWork.AttendanceRequests.Query()
                .Where(r => !r.IsDeleted && r.EmployeeId == employeeId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            // Holidays
            var holidaysList = await _unitOfWork.Holidays.Query()
                .Where(h => !h.IsDeleted && h.IsActive)
                .ToListAsync();

            // Approved Leaves
            var allLeaves = await _unitOfWork.LeaveRequest.GetAll();
            var approvedLeaves = allLeaves
                .Where(l => !l.IsDeleted && l.EmployeeId == employeeId && l.Status != null && l.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var allLeaveTypes = await _unitOfWork.LeaveType.GetAll();
            var leaveTypeDict = allLeaveTypes.ToDictionary(lt => lt.Id, lt => lt.Type ?? "Leave");

            // Build Day-by-Day view
            var days = new List<TimesheetDayViewDTO>();
            for (var dt = start; dt <= end; dt = dt.AddDays(1))
            {
                var curDate = dt.Date;
                var dayAttendance = attendanceList.FirstOrDefault(a => a.AttendanceDate.ToDateTime(TimeOnly.MinValue).Date == curDate);

                DateTime? clockIn = dayAttendance?.ClockIn;
                DateTime? clockOut = dayAttendance?.ClockOut;
                decimal attHours = dayAttendance?.TotalHours ?? dayAttendance?.EffectiveHours ?? 0m;

                var matchReg = empAttRequests.FirstOrDefault(r => r.RequestedDate.Date == curDate);
                string? regStatus = matchReg?.Status;
                bool isRegularized = matchReg != null && string.Equals(matchReg.Status, "Approved", StringComparison.OrdinalIgnoreCase);
                bool isShiftInProgress = clockIn.HasValue && !clockOut.HasValue && curDate == DateTime.UtcNow.Date;

                // Holiday check
                var curDateOnly = DateOnly.FromDateTime(curDate);
                var holidayMatch = holidaysList.FirstOrDefault(h => 
                    h.HolidayDate == curDateOnly || 
                    (h.HolidayDate.Year == curDate.Year && h.HolidayDate.Month == curDate.Month && h.HolidayDate.Day == curDate.Day) ||
                    h.HolidayDate.ToDateTime(TimeOnly.MinValue).Date == curDate);
                bool isHoliday = holidayMatch != null;
                string? holidayName = holidayMatch?.HolidayName;
                bool isOptionalHoliday = holidayMatch?.IsOptional ?? false;

                // Approved Leave check
                var leaveMatch = approvedLeaves.FirstOrDefault(l => 
                    (curDate >= l.StartDate.Date && curDate <= l.EndDate.Date) ||
                    (curDateOnly >= DateOnly.FromDateTime(l.StartDate) && curDateOnly <= DateOnly.FromDateTime(l.EndDate)));
                bool isLeave = leaveMatch != null;
                string? leaveTypeName = null;
                if (leaveMatch != null)
                {
                    leaveTypeName = leaveTypeDict.ContainsKey(leaveMatch.LeaveTypeId) ? leaveTypeDict[leaveMatch.LeaveTypeId] : (leaveMatch.LeaveType?.Type ?? "On Leave");
                }

                bool isWeekend = curDate.DayOfWeek == DayOfWeek.Saturday || curDate.DayOfWeek == DayOfWeek.Sunday;
                bool isPriorToJoining = employee?.DateOfJoining.HasValue == true && curDate < employee.DateOfJoining.Value.Date;
                bool hasPunch = clockIn.HasValue || attHours > 0;
                bool isFuture = curDate > DateTime.UtcNow.Date;
                bool isAbsent = !isFuture && !isPriorToJoining && !isLeave && !isHoliday && !isWeekend && !hasPunch && !isShiftInProgress && !isRegularized;

                var dayTasks = entries
                    .Where(e => e.EntryDate.Date == curDate)
                    .Select(e => new TimesheetTaskItemDTO
                    {
                        Id = e.Id,
                        ProjectId = e.ProjectId,
                        ProjectName = projectDict.ContainsKey(e.ProjectId) ? projectDict[e.ProjectId] : "Project",
                        TaskDescription = e.TaskDescription,
                        Hours = e.Hours
                    })
                    .ToList();

                days.Add(new TimesheetDayViewDTO
                {
                    Date = curDate,
                    DayName = curDate.ToString("ddd"),
                    ClockIn = clockIn,
                    ClockOut = clockOut,
                    AttendanceHours = attHours,
                    IsRegularized = isRegularized,
                    RegularizationStatus = regStatus,
                    IsAbsent = isAbsent,
                    IsShiftInProgress = isShiftInProgress,
                    IsHoliday = isHoliday,
                    HolidayName = holidayName,
                    IsOptionalHoliday = isOptionalHoliday,
                    IsLeave = isLeave,
                    LeaveTypeName = leaveTypeName,
                    IsWeekend = isWeekend,
                    IsPriorToJoining = isPriorToJoining,
                    Tasks = dayTasks
                });
            }

            var calculatedHours = days.Sum(d => d.DayTotalHours);
            var totalTimesheetHours = (timesheet != null && timesheet.Status == "Rejected" && calculatedHours > 0)
                ? calculatedHours
                : (timesheet != null && timesheet.Status != "Draft" && timesheet.TotalHours > 0 && calculatedHours == 0
                    ? timesheet.TotalHours
                    : calculatedHours);
            var totalAttendanceHours = days.Sum(d => d.AttendanceHours);

            return new TimesheetViewDTO
            {
                TimesheetId = timesheet?.Id,
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                PeriodType = timesheet?.PeriodType ?? (end.Subtract(start).TotalDays > 14 ? "Monthly" : "Weekly"),
                StartDate = start,
                EndDate = end,
                TotalTimesheetHours = totalTimesheetHours,
                TotalAttendanceHours = totalAttendanceHours,
                Status = timesheet?.Status ?? "Draft",
                SubmittedDate = timesheet?.SubmittedDate,
                ManagerId = managerId,
                ManagerName = managerName,
                RejectionReason = timesheet?.RejectionReason,
                Days = days
            };
        }

        public async Task<TimesheetEntryDTO> SaveEntryAsync(TimesheetEntryDTO dto, long employeeId)
        {
            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee?.DateOfJoining.HasValue == true && dto.EntryDate.Date < employee.DateOfJoining.Value.Date)
            {
                throw new InvalidOperationException($"Cannot log timesheet tasks for {dto.EntryDate:yyyy-MM-dd} prior to your official joining date ({employee.DateOfJoining.Value:yyyy-MM-dd}).");
            }

            var entryDate = dto.EntryDate.Date;
            if (entryDate > DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException($"Cannot log timesheet tasks for future dates ({dto.EntryDate:yyyy-MM-dd}).");
            }

            // Check if user has an approved leave on this entry date
            var allLeaves = await _unitOfWork.LeaveRequest.GetAll();
            var leaveOnDate = allLeaves.FirstOrDefault(l => !l.IsDeleted && l.EmployeeId == employeeId && 
                l.Status != null && l.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                entryDate >= l.StartDate.Date && entryDate <= l.EndDate.Date);
            if (leaveOnDate != null)
            {
                throw new InvalidOperationException($"Cannot log timesheet tasks on an approved leave date ({dto.EntryDate:yyyy-MM-dd}).");
            }

            // Check if it is a weekend or company holiday
            bool isWeekend = entryDate.DayOfWeek == DayOfWeek.Saturday || entryDate.DayOfWeek == DayOfWeek.Sunday;
            var allHolidays = await _unitOfWork.Holidays.GetAll();
            var entryDateOnly = DateOnly.FromDateTime(entryDate);
            bool isHoliday = allHolidays.Any(h => !h.IsDeleted && h.IsActive && 
                (h.HolidayDate == entryDateOnly || 
                 (h.HolidayDate.Year == entryDate.Year && h.HolidayDate.Month == entryDate.Month && h.HolidayDate.Day == entryDate.Day) ||
                 h.HolidayDate.ToDateTime(TimeOnly.MinValue).Date == entryDate));

            // On regular working days (not weekend, not holiday, not approved leave):
            // The employee MUST have recorded an attendance punch OR have an APPROVED regularization request!
            if (!isWeekend && !isHoliday)
            {
                var allAttendance = await _unitOfWork.EmployeeAttendance.GetAll();
                var hasPunch = allAttendance.Any(a => !a.IsDeleted && a.EmployeeId == employeeId &&
                    a.AttendanceDate == entryDateOnly &&
                    (a.ClockIn != default || a.TotalHours > 0 || a.EffectiveHours > 0));

                var allAttRequests = await _unitOfWork.AttendanceRequests.GetAll();
                var hasApprovedReg = allAttRequests.Any(r => !r.IsDeleted && r.EmployeeId == employeeId &&
                    r.RequestedDate.Date == entryDate &&
                    r.Status != null && r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));

                if (!hasPunch && !hasApprovedReg)
                {
                    var isRegPending = allAttRequests.Any(r => !r.IsDeleted && r.EmployeeId == employeeId &&
                        r.RequestedDate.Date == entryDate &&
                        r.Status != null && r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));

                    if (isRegPending)
                    {
                        throw new InvalidOperationException($"Cannot log tasks for {dto.EntryDate:yyyy-MM-dd}. Your attendance regularization request for this date is currently awaiting manager approval.");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Cannot log tasks for {dto.EntryDate:yyyy-MM-dd} because you were marked absent (no attendance punch recorded). Please request attendance regularization first; once approved by your manager, task logging will be enabled.");
                    }
                }
            }

            var project = await _unitOfWork.ProjectMasters.GetById(dto.ProjectId);
            var projectName = project?.ProjectName ?? "Project";

            if (dto.Id > 0)
            {
                var existing = await _unitOfWork.TimesheetEntries.GetById(dto.Id);
                if (existing != null && existing.EmployeeId == employeeId && !existing.IsDeleted)
                {
                    // If existing entry is already locked to a historical submitted/rejected/approved timesheet,
                    // DO NOT mutate the historical entry! Clone it as a working draft instead!
                    if (existing.TimesheetId.HasValue && existing.TimesheetId.Value > 0)
                    {
                        var workingCopy = new TimesheetEntry
                        {
                            TimesheetId = null,
                            EmployeeId = employeeId,
                            ProjectId = dto.ProjectId,
                            EntryDate = dto.EntryDate.Date,
                            TaskDescription = dto.TaskDescription,
                            Hours = dto.Hours,
                            CreatedBy = (int)employeeId,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedBy = (int)employeeId,
                            UpdatedDate = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false
                        };
                        await _unitOfWork.TimesheetEntries.Add(workingCopy);
                        _unitOfWork.Save();
                        dto.Id = workingCopy.Id;
                        dto.ProjectName = projectName;
                        return dto;
                    }

                    existing.ProjectId = dto.ProjectId;
                    existing.EntryDate = dto.EntryDate.Date;
                    existing.TaskDescription = dto.TaskDescription;
                    existing.Hours = dto.Hours;
                    existing.UpdatedBy = (int)employeeId;
                    existing.UpdatedDate = DateTime.UtcNow;

                    _unitOfWork.TimesheetEntries.Update(existing);
                    _unitOfWork.Save();

                    dto.ProjectName = projectName;
                    return dto;
                }
            }

            var newEntry = new TimesheetEntry
            {
                EmployeeId = employeeId,
                ProjectId = dto.ProjectId,
                EntryDate = dto.EntryDate.Date,
                TaskDescription = dto.TaskDescription,
                Hours = dto.Hours,
                CreatedBy = (int)employeeId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = (int)employeeId,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.TimesheetEntries.Add(newEntry);
            _unitOfWork.Save();

            dto.Id = newEntry.Id;
            dto.ProjectName = projectName;
            return dto;
        }

        public async Task<bool> DeleteEntryAsync(long entryId, long employeeId)
        {
            if (entryId <= 0) return false;

            var entry = await _unitOfWork.TimesheetEntries.GetById(entryId);
            if (entry == null || entry.EmployeeId != employeeId || entry.IsDeleted) return false;

            // If the entry belongs to a historical submitted/rejected/approved timesheet,
            // we do NOT soft-delete the historical entry from the old timesheet so its audit trail stays intact!
            if (!entry.TimesheetId.HasValue || entry.TimesheetId.Value == 0)
            {
                entry.IsDeleted = true;
                entry.IsActive = false;
                entry.UpdatedBy = (int)employeeId;
                entry.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.TimesheetEntries.Update(entry);
                var result = _unitOfWork.Save();
                return result > 0;
            }

            return true;
        }

        public async Task<TimesheetViewDTO> SubmitPeriodAsync(TimesheetSubmitDTO dto, long employeeId)
        {
            var start = dto.StartDate.Date;
            var end = dto.EndDate.Date;

            var employee = await _unitOfWork.Employees.GetById(employeeId);
            var managerId = employee?.ManagerId;

            var allTimesheets = await _unitOfWork.Timesheets.GetAll();
            var existingTimesheets = allTimesheets
                .Where(t => !t.IsDeleted && t.EmployeeId == employeeId && t.StartDate.Date == start && t.EndDate.Date == end)
                .OrderByDescending(t => t.Id)
                .ToList();
            var latestTimesheet = existingTimesheets.FirstOrDefault();

            var allEntries = await _unitOfWork.TimesheetEntries.GetAll();

            // Candidate entries for this new submission:
            // 1) Active entries with TimesheetId == null (working draft)
            var workingEntries = allEntries
                .Where(e => !e.IsDeleted && e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end && (e.TimesheetId == null || e.TimesheetId == 0))
                .ToList();

            List<TimesheetEntry> entriesToSubmit;
            if (workingEntries.Any())
            {
                entriesToSubmit = workingEntries;
            }
            else if (latestTimesheet != null && latestTimesheet.Status == "Rejected")
            {
                // Fallback: If no working entries were created yet, clone active entries from the rejected timesheet
                var historical = allEntries
                    .Where(e => !e.IsDeleted && e.TimesheetId == latestTimesheet.Id)
                    .ToList();
                entriesToSubmit = new List<TimesheetEntry>();
                foreach (var h in historical)
                {
                    var clone = new TimesheetEntry
                    {
                        TimesheetId = null,
                        EmployeeId = h.EmployeeId,
                        ProjectId = h.ProjectId,
                        EntryDate = h.EntryDate,
                        TaskDescription = h.TaskDescription,
                        Hours = h.Hours,
                        ClockInRef = h.ClockInRef,
                        ClockOutRef = h.ClockOutRef,
                        IsRegularized = h.IsRegularized,
                        CreatedBy = h.CreatedBy,
                        CreatedDate = h.CreatedDate,
                        UpdatedBy = (int)employeeId,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };
                    await _unitOfWork.TimesheetEntries.Add(clone);
                    entriesToSubmit.Add(clone);
                }
                if (entriesToSubmit.Any())
                {
                    _unitOfWork.Save();
                }
            }
            else
            {
                entriesToSubmit = allEntries
                    .Where(e => !e.IsDeleted && e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end && (e.TimesheetId == null || (latestTimesheet != null && e.TimesheetId == latestTimesheet.Id)))
                    .ToList();
            }

            // Validate that no tasks in entriesToSubmit are logged on absent unregularized dates
            var allAttendanceRecords = await _unitOfWork.EmployeeAttendance.GetAll();
            var allAttRequestsForSubmit = await _unitOfWork.AttendanceRequests.GetAll();
            var allHolidaysForSubmit = await _unitOfWork.Holidays.GetAll();
            var allLeavesForSubmit = await _unitOfWork.LeaveRequest.GetAll();

            foreach (var item in entriesToSubmit)
            {
                var curDate = item.EntryDate.Date;
                bool isWeekend = curDate.DayOfWeek == DayOfWeek.Saturday || curDate.DayOfWeek == DayOfWeek.Sunday;
                var curDateOnly = DateOnly.FromDateTime(curDate);
                bool isHoliday = allHolidaysForSubmit.Any(h => !h.IsDeleted && h.IsActive &&
                    (h.HolidayDate == curDateOnly ||
                     (h.HolidayDate.Year == curDate.Year && h.HolidayDate.Month == curDate.Month && h.HolidayDate.Day == curDate.Day) ||
                     h.HolidayDate.ToDateTime(TimeOnly.MinValue).Date == curDate));
                bool isLeave = allLeavesForSubmit.Any(l => !l.IsDeleted && l.EmployeeId == employeeId &&
                    l.Status != null && l.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
                    curDate >= l.StartDate.Date && curDate <= l.EndDate.Date);

                if (!isWeekend && !isHoliday && !isLeave)
                {
                    bool hasPunch = allAttendanceRecords.Any(a => !a.IsDeleted && a.EmployeeId == employeeId &&
                        a.AttendanceDate == curDateOnly &&
                        (a.ClockIn != default || a.TotalHours > 0 || a.EffectiveHours > 0));

                    bool hasApprovedReg = allAttRequestsForSubmit.Any(r => !r.IsDeleted && r.EmployeeId == employeeId &&
                        r.RequestedDate.Date == curDate &&
                        r.Status != null && r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));

                    if (!hasPunch && !hasApprovedReg)
                    {
                        throw new InvalidOperationException($"Timesheet contains tasks logged on {curDate:yyyy-MM-dd}, a day you were marked absent. Please regularize attendance for this date before submitting.");
                    }
                }
            }

            var totalHours = entriesToSubmit.Sum(e => e.Hours);

            Timesheet timesheet;
            // If timesheet was never created OR was previously Rejected, create a NEW submission record
            // so that the previous Rejected record remains intact in history for the manager!
            if (latestTimesheet == null || latestTimesheet.Status == "Rejected")
            {
                var newTimesheet = new Timesheet
                {
                    EmployeeId = employeeId,
                    PeriodType = dto.PeriodType ?? "Weekly",
                    StartDate = start,
                    EndDate = end,
                    TotalHours = totalHours,
                    Status = "Submitted",
                    SubmittedDate = DateTime.UtcNow,
                    ManagerId = managerId,
                    CreatedBy = (int)employeeId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = (int)employeeId,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.Timesheets.Add(newTimesheet);
                _unitOfWork.Save();
                timesheet = newTimesheet;
            }
            else
            {
                latestTimesheet.Status = "Submitted";
                latestTimesheet.SubmittedDate = DateTime.UtcNow;
                latestTimesheet.ManagerId = managerId;
                latestTimesheet.TotalHours = totalHours;
                latestTimesheet.RejectionReason = null;
                latestTimesheet.UpdatedBy = (int)employeeId;
                latestTimesheet.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Timesheets.Update(latestTimesheet);
                _unitOfWork.Save();
                timesheet = latestTimesheet;
            }

            // Link all submitted entries to this timesheet.
            foreach (var entry in entriesToSubmit)
            {
                entry.TimesheetId = timesheet.Id;
                _unitOfWork.TimesheetEntries.Update(entry);
            }
            _unitOfWork.Save();

            try
            {
                var empName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : $"Employee #{employeeId}";
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = 0,
                    Title = "Timesheet Awaiting Review",
                    Message = $"{empName} submitted {timesheet.PeriodType} timesheet ({timesheet.TotalHours} hrs) for {timesheet.StartDate:yyyy-MM-dd} to {timesheet.EndDate:yyyy-MM-dd}.",
                    Category = "Timesheet",
                    Type = "request",
                    Icon = "more_time",
                    IconBg = "#f3e8ff",
                    IconColor = "#9333ea",
                    Route = "/index/request",
                    QueryParams = "tab=timesheet",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch
            {
            }

            return await GetPeriodTimesheetAsync(employeeId, start, end, timesheet.Id);
        }

        public async Task<List<TimesheetViewDTO>> GetPendingApprovalsForManagerAsync(long managerId, bool isAdminOrHR = false)
        {
            var allTimesheets = await _unitOfWork.Timesheets.GetAll();
            var pendingTimesheets = allTimesheets
                .Where(t => !t.IsDeleted && t.Status != "Draft" && (isAdminOrHR || t.ManagerId == managerId))
                .OrderByDescending(t => t.ActionDate ?? t.SubmittedDate ?? t.UpdatedDate)
                .ToList();

            var resultList = new List<TimesheetViewDTO>();
            foreach (var ts in pendingTimesheets)
            {
                var view = await GetPeriodTimesheetAsync(ts.EmployeeId, ts.StartDate, ts.EndDate, ts.Id);
                resultList.Add(view);
            }

            return resultList;
        }

        public async Task<bool> ApproveOrRejectAsync(TimesheetApprovalDTO dto, long managerId)
        {
            if (dto.TimesheetId <= 0) return false;

            var timesheet = await _unitOfWork.Timesheets.GetById(dto.TimesheetId);
            if (timesheet == null || timesheet.IsDeleted) return false;

            var isApproved = dto.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase);
            timesheet.Status = isApproved ? "Approved" : "Rejected";
            timesheet.ActionBy = managerId;
            timesheet.ActionDate = DateTime.UtcNow;
            timesheet.RejectionReason = isApproved ? null : dto.RejectionReason;
            timesheet.UpdatedBy = (int)managerId;
            timesheet.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Timesheets.Update(timesheet);
            var result = _unitOfWork.Save();

            if (result > 0)
            {
                if (!isApproved)
                {
                    try
                    {
                        var allEntries = await _unitOfWork.TimesheetEntries.GetAll();
                        var existingDrafts = allEntries.Where(e => !e.IsDeleted && e.EmployeeId == timesheet.EmployeeId &&
                            e.EntryDate.Date >= timesheet.StartDate.Date && e.EntryDate.Date <= timesheet.EndDate.Date &&
                            e.TimesheetId == null).ToList();

                        if (!existingDrafts.Any())
                        {
                            var rejectedEntries = allEntries.Where(e => !e.IsDeleted && e.TimesheetId == timesheet.Id).ToList();
                            foreach (var re in rejectedEntries)
                            {
                                var clone = new TimesheetEntry
                                {
                                    TimesheetId = null,
                                    EmployeeId = re.EmployeeId,
                                    ProjectId = re.ProjectId,
                                    EntryDate = re.EntryDate,
                                    TaskDescription = re.TaskDescription,
                                    Hours = re.Hours,
                                    ClockInRef = re.ClockInRef,
                                    ClockOutRef = re.ClockOutRef,
                                    IsRegularized = re.IsRegularized,
                                    CreatedBy = re.CreatedBy,
                                    CreatedDate = re.CreatedDate,
                                    UpdatedBy = (int)timesheet.EmployeeId,
                                    UpdatedDate = DateTime.UtcNow,
                                    IsActive = true,
                                    IsDeleted = false
                                };
                                await _unitOfWork.TimesheetEntries.Add(clone);
                            }
                            _unitOfWork.Save();
                        }
                    }
                    catch
                    {
                    }
                }

                try
                {
                    await _unitOfWork.Notifications.Add(new Notification
                    {
                        EmployeeId = timesheet.EmployeeId,
                        Title = isApproved ? "Timesheet Approved" : "Timesheet Rejected",
                        Message = isApproved 
                            ? $"Your timesheet for {timesheet.StartDate:yyyy-MM-dd} to {timesheet.EndDate:yyyy-MM-dd} ({timesheet.TotalHours} hrs) has been approved."
                            : $"Your timesheet for {timesheet.StartDate:yyyy-MM-dd} to {timesheet.EndDate:yyyy-MM-dd} was rejected. Reason: {dto.RejectionReason ?? "No reason provided"}",
                        Category = "Timesheet",
                        Type = isApproved ? "approval" : "alert",
                        Icon = isApproved ? "check_circle" : "cancel",
                        IconBg = isApproved ? "#f0fdf4" : "#fef2f2",
                        IconColor = isApproved ? "#16a34a" : "#dc2626",
                        Route = "/index/timesheet",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    });
                    _unitOfWork.Save();
                }
                catch
                {
                }
            }

            return result > 0;
        }
    }
}
