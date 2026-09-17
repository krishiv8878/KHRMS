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
            var allTimesheets = await _unitOfWork.Timesheets.GetAll();
            Timesheet? timesheet = null;
            if (timesheetId.HasValue && timesheetId.Value > 0)
            {
                timesheet = allTimesheets.FirstOrDefault(t => !t.IsDeleted && t.Id == timesheetId.Value);
            }
            if (timesheet == null)
            {
                timesheet = allTimesheets
                    .Where(t => !t.IsDeleted && t.EmployeeId == employeeId && t.StartDate.Date == start && t.EndDate.Date == end)
                    .OrderByDescending(t => t.Id)
                    .FirstOrDefault();
            }

            // Find all Entries for this employee in date range
            var allEntries = await _unitOfWork.TimesheetEntries.GetAll();
            var entries = allEntries
                .Where(e => !e.IsDeleted && e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end)
                .ToList();

            // Projects map
            var allProjects = await _unitOfWork.ProjectMasters.GetAll();
            var projectDict = allProjects.ToDictionary(p => p.Id, p => p.ProjectName ?? "Project");

            // Attendance records in range
            var allAttendance = await _unitOfWork.EmployeeAttendance.GetAll();
            var attendanceList = allAttendance
                .Where(a => !a.IsDeleted && a.EmployeeId == employeeId)
                .ToList();

            // Attendance requests (regularizations) in range
            var allAttRequests = await _unitOfWork.AttendanceRequests.GetAll();
            var regularizedDates = allAttRequests
                .Where(r => !r.IsDeleted && r.EmployeeId == employeeId && r.Status != null && r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.RequestedDate.Date)
                .ToHashSet();

            // Build Day-by-Day view
            var days = new List<TimesheetDayViewDTO>();
            for (var dt = start; dt <= end; dt = dt.AddDays(1))
            {
                var curDate = dt.Date;
                var dayAttendance = attendanceList.FirstOrDefault(a => a.AttendanceDate.ToDateTime(TimeOnly.MinValue).Date == curDate);

                DateTime? clockIn = dayAttendance?.ClockIn;
                DateTime? clockOut = dayAttendance?.ClockOut;
                decimal attHours = dayAttendance?.TotalHours ?? dayAttendance?.EffectiveHours ?? 0m;
                bool isRegularized = regularizedDates.Contains(curDate);
                bool isShiftInProgress = clockIn.HasValue && !clockOut.HasValue && curDate == DateTime.UtcNow.Date;

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
                    IsShiftInProgress = isShiftInProgress,
                    Tasks = dayTasks
                });
            }

            var totalTimesheetHours = days.Sum(d => d.DayTotalHours);
            var totalAttendanceHours = days.Sum(d => d.AttendanceHours);

            return new TimesheetViewDTO
            {
                TimesheetId = timesheet?.Id,
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                PeriodType = timesheet?.PeriodType ?? "Weekly",
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
            var project = await _unitOfWork.ProjectMasters.GetById(dto.ProjectId);
            var projectName = project?.ProjectName ?? "Project";

            if (dto.Id > 0)
            {
                var existing = await _unitOfWork.TimesheetEntries.GetById(dto.Id);
                if (existing != null && existing.EmployeeId == employeeId && !existing.IsDeleted)
                {
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

            entry.IsDeleted = true;
            entry.IsActive = false;
            entry.UpdatedBy = (int)employeeId;
            entry.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.TimesheetEntries.Update(entry);
            var result = _unitOfWork.Save();
            return result > 0;
        }

        public async Task<TimesheetViewDTO> SubmitPeriodAsync(TimesheetSubmitDTO dto, long employeeId)
        {
            var start = dto.StartDate.Date;
            var end = dto.EndDate.Date;

            var employee = await _unitOfWork.Employees.GetById(employeeId);
            var managerId = employee?.ManagerId;

            var allTimesheets = await _unitOfWork.Timesheets.GetAll();
            var timesheet = allTimesheets
                .Where(t => !t.IsDeleted && t.EmployeeId == employeeId && t.StartDate.Date == start && t.EndDate.Date == end)
                .OrderByDescending(t => t.Id)
                .FirstOrDefault();

            var allEntries = await _unitOfWork.TimesheetEntries.GetAll();
            var entries = allEntries
                .Where(e => !e.IsDeleted && e.EmployeeId == employeeId && e.EntryDate.Date >= start && e.EntryDate.Date <= end)
                .ToList();

            var totalHours = entries.Sum(e => e.Hours);

            // If timesheet was never created OR was previously Rejected, create a NEW submission record
            // so that the previous Rejected record remains intact in history for the manager!
            if (timesheet == null || timesheet.Status == "Rejected")
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
                timesheet.Status = "Submitted";
                timesheet.SubmittedDate = DateTime.UtcNow;
                timesheet.ManagerId = managerId;
                timesheet.TotalHours = totalHours;
                timesheet.RejectionReason = null;
                timesheet.UpdatedBy = (int)employeeId;
                timesheet.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.Timesheets.Update(timesheet);
                _unitOfWork.Save();
            }

            // Link all entries to this timesheet
            foreach (var entry in entries)
            {
                if (entry.TimesheetId != timesheet.Id)
                {
                    entry.TimesheetId = timesheet.Id;
                    _unitOfWork.TimesheetEntries.Update(entry);
                }
            }
            _unitOfWork.Save();

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
            return result > 0;
        }
    }
}
