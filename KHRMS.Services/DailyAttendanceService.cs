using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using Serilog;

namespace KHRMS.Services
{
    public class DailyAttendanceService : IDailyAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAttendanceLockService _lockService;

        public DailyAttendanceService(IUnitOfWork unitOfWork, IAttendanceLockService lockService)
        {
            _unitOfWork = unitOfWork;
            _lockService = lockService;
        }

        public async Task<int> ComputeDailyAttendanceAsync(DateOnly targetDate, long? specificEmployeeId = null)
        {
            if (await _lockService.IsMonthLockedAsync(targetDate.Month, targetDate.Year))
            {
                Log.Warning("Skipping attendance computation for {Date:yyyy-MM-dd} as the month is locked.", targetDate);
                return 0;
            }

            var allEmployees = (await _unitOfWork.Employees.GetAll())
                .Where(e => !e.IsDeleted && e.IsActive)
                .ToList();

            if (specificEmployeeId.HasValue && specificEmployeeId.Value > 0)
            {
                allEmployees = allEmployees.Where(e => e.Id == specificEmployeeId.Value).ToList();
            }

            if (!allEmployees.Any()) return 0;

            var empIds = allEmployees.Select(e => e.Id).ToHashSet();

            // Preload shifts and assignments
            var shifts = (await _unitOfWork.ShiftRepository.GetAll()).Where(s => !s.IsDeleted).ToList();
            var defaultShift = shifts.FirstOrDefault() ?? new ShiftMaster
            {
                Id = 1,
                ShiftName = "General Shift",
                StartTime = "09:00",
                EndTime = "18:00",
                GracePeriodMinutes = 15,
                FullDayMinimumMinutes = 480,
                HalfDayMinimumMinutes = 240,
                PresentMinimumMinutes = 120,
                ScheduledBreakMinutes = 60,
                MaxLateMarksPerMonth = 3,
                AutoClockOutBufferHours = 4
            };

            var shiftAssignments = (await _unitOfWork.EmployeeShiftAssignments.GetAll())
                .Where(a => !a.IsDeleted && a.IsActive && empIds.Contains(a.EmployeeId) &&
                            a.EffectiveFromDate <= targetDate &&
                            (!a.EffectiveToDate.HasValue || a.EffectiveToDate.Value >= targetDate))
                .ToList();

            // Preload holidays
            var holiday = (await _unitOfWork.Holidays.GetAll())
                .FirstOrDefault(h => !h.IsDeleted && h.HolidayDate == targetDate);

            // Preload approved leaves
            var targetDateTime = targetDate.ToDateTime(TimeOnly.MinValue);
            var approvedLeaves = (await _unitOfWork.LeaveRequest.GetAll())
                .Where(l => !l.IsDeleted &&
                            string.Equals(l.Status, "Approved", StringComparison.OrdinalIgnoreCase) &&
                            empIds.Contains(l.EmployeeId) &&
                            targetDateTime.Date >= l.StartDate.Date &&
                            targetDateTime.Date <= l.EndDate.Date)
                .ToDictionary(l => l.EmployeeId, l => l);

            // Preload work modes
            var workModes = (await _unitOfWork.EmployeeDailyWorkModes.GetAll())
                .Where(w => !w.IsDeleted && empIds.Contains(w.EmployeeId) && w.WorkDate == targetDate)
                .ToDictionary(w => w.EmployeeId, w => w.WorkMode);

            // Preload raw attendance punches
            var rawAttendances = (await _unitOfWork.EmployeeAttendance.GetAll())
                .Where(a => !a.IsDeleted && empIds.Contains(a.EmployeeId) && a.AttendanceDate == targetDate)
                .ToDictionary(a => a.EmployeeId, a => a);

            // Preload raw attendance logs
            var attendanceLogs = (await _unitOfWork.AttendanceLog.GetAll())
                .Where(l => empIds.Contains(l.EmployeeId) && l.AttendanceDate == targetDate)
                .GroupBy(l => l.EmployeeId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.InTime).ToList());

            // Preload existing daily computed records
            var existingRecords = (await _unitOfWork.DailyAttendanceRecords.GetAll())
                .Where(r => !r.IsDeleted && empIds.Contains(r.EmployeeId) && r.AttendanceDate == targetDate)
                .ToDictionary(r => r.EmployeeId, r => r);

            int computedCount = 0;

            foreach (var emp in allEmployees)
            {
                // Verify employment tenure bounds
                if (emp.DateOfJoining.HasValue && targetDateTime.Date < emp.DateOfJoining.Value.Date)
                {
                    continue; // Employee had not yet joined
                }

                // Resolve Shift
                var assignment = shiftAssignments.FirstOrDefault(a => a.EmployeeId == emp.Id);
                var shift = assignment != null && assignment.ShiftId > 0
                    ? shifts.FirstOrDefault(s => s.Id == assignment.ShiftId) ?? defaultShift
                    : defaultShift;

                // Week-off determination
                string weekOffPattern = assignment?.WeekOffPattern ?? "Sunday,Saturday";
                bool isWeekOff = IsDateWeekOff(targetDate, weekOffPattern);

                // Holiday determination
                bool isHoliday = holiday != null;

                // Leave determination
                bool isOnLeave = approvedLeaves.TryGetValue(emp.Id, out var leaveReq);

                // Work mode determination
                bool isWfh = workModes.TryGetValue(emp.Id, out var modeStr) &&
                             (string.Equals(modeStr, "WFH", StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(modeStr, "Remote", StringComparison.OrdinalIgnoreCase));

                // Punch analysis
                rawAttendances.TryGetValue(emp.Id, out var rawAtt);
                attendanceLogs.TryGetValue(emp.Id, out var logs);

                DateTime? firstIn = rawAtt?.ClockIn != default ? rawAtt?.ClockIn : logs?.FirstOrDefault()?.InTime;
                DateTime? lastOut = rawAtt?.ClockOut != default ? rawAtt?.ClockOut : logs?.LastOrDefault()?.OutTime;

                existingRecords.TryGetValue(emp.Id, out var existingRec);

                bool isAlreadyRegularized = existingRec != null && existingRec.IsRegularized;

                int totalWorkMins = 0;
                int totalBreakMins = 0;
                int effectiveMins = 0;
                int lateArrivalMins = 0;
                int earlyExitMins = 0;
                int overtimeMins = 0;
                string status = "Absent";
                string? remarks = null;

                if (firstIn.HasValue && firstIn.Value != DateTime.MinValue)
                {
                    // Check auto clock-out if clock out is missing
                    if (!lastOut.HasValue || lastOut.Value == DateTime.MinValue)
                    {
                        var shiftEndTime = ParseShiftTime(shift.EndTime ?? "18:00", targetDate);
                        var bufferLimit = shiftEndTime.AddHours(shift.AutoClockOutBufferHours > 0 ? shift.AutoClockOutBufferHours : 4);

                        if (DateTime.Now > bufferLimit)
                        {
                            // Stale open punch past buffer: auto clock-out
                            lastOut = shiftEndTime;
                            remarks = "Auto clocked out by system buffer";
                            status = "MissedPunch";
                        }
                    }

                    if (lastOut.HasValue && lastOut.Value > firstIn.Value)
                    {
                        totalWorkMins = (int)(lastOut.Value - firstIn.Value).TotalMinutes;

                        // Calculate break minutes from interval gaps between punches
                        if (logs != null && logs.Count > 1)
                        {
                            for (int i = 0; i < logs.Count - 1; i++)
                            {
                                var currentOut = logs[i].OutTime;
                                var nextIn = logs[i + 1].InTime;
                                if (currentOut.HasValue && nextIn.HasValue && nextIn.Value > currentOut.Value)
                                {
                                    totalBreakMins += (int)(nextIn.Value - currentOut.Value).TotalMinutes;
                                }
                            }
                        }

                        if (totalBreakMins <= 0 && totalWorkMins >= 300)
                        {
                            totalBreakMins = shift.ScheduledBreakMinutes > 0 ? shift.ScheduledBreakMinutes : 60;
                        }

                        effectiveMins = Math.Max(0, totalWorkMins - totalBreakMins);

                        // Late Arrival calculation
                        var shiftStartTime = ParseShiftTime(shift.StartTime ?? "09:00", targetDate);
                        var graceThreshold = shiftStartTime.AddMinutes(shift.GracePeriodMinutes > 0 ? shift.GracePeriodMinutes : 15);
                        if (firstIn.Value > graceThreshold)
                        {
                            lateArrivalMins = (int)(firstIn.Value - shiftStartTime).TotalMinutes;
                        }

                        // Early Exit calculation
                        var shiftEndTime = ParseShiftTime(shift.EndTime ?? "18:00", targetDate);
                        if (lastOut.Value < shiftEndTime)
                        {
                            earlyExitMins = (int)(shiftEndTime - lastOut.Value).TotalMinutes;
                        }

                        // Overtime calculation
                        int fullDayThreshold = shift.FullDayMinimumMinutes > 0 ? shift.FullDayMinimumMinutes : 480;
                        if (effectiveMins > fullDayThreshold)
                        {
                            overtimeMins = effectiveMins - fullDayThreshold;
                        }

                        // Status evaluation
                        if (isAlreadyRegularized)
                        {
                            status = existingRec!.Status;
                        }
                        else if (status == "MissedPunch")
                        {
                            // Kept as MissedPunch
                        }
                        else if (effectiveMins >= fullDayThreshold)
                        {
                            if (lateArrivalMins > 0)
                            {
                                status = "Late";
                                remarks = remarks ?? $"Late arrival by {lateArrivalMins} mins";
                            }
                            else if (isWfh)
                            {
                                status = "WFH";
                            }
                            else
                            {
                                status = "Present";
                            }
                        }
                        else if (effectiveMins >= (shift.HalfDayMinimumMinutes > 0 ? shift.HalfDayMinimumMinutes : 240))
                        {
                            status = "HalfDay";
                            remarks = remarks ?? $"Short hours ({effectiveMins / 60}h {effectiveMins % 60}m worked)";
                        }
                        else
                        {
                            status = "Absent";
                            remarks = remarks ?? "Insufficient work duration";
                        }
                    }
                }
                else
                {
                    // No punches logged
                    if (isAlreadyRegularized)
                    {
                        status = existingRec!.Status;
                    }
                    else if (isOnLeave)
                    {
                        status = "OnLeave";
                        remarks = $"Leave: {leaveReq?.LeaveReason ?? "Approved Leave"}";
                    }
                    else if (isHoliday)
                    {
                        status = "Holiday";
                        remarks = holiday?.HolidayName ?? "Company Holiday";
                    }
                    else if (isWeekOff)
                    {
                        status = "WeekOff";
                    }
                    else
                    {
                        status = "Absent";
                    }
                }

                // Upsert into DailyAttendanceRecords
                if (existingRec != null)
                {
                    existingRec.ShiftId = shift.Id;
                    existingRec.FirstClockIn = firstIn;
                    existingRec.LastClockOut = lastOut;
                    existingRec.TotalWorkMinutes = totalWorkMins;
                    existingRec.TotalBreakMinutes = totalBreakMins;
                    existingRec.EffectiveWorkMinutes = effectiveMins;
                    existingRec.OvertimeMinutes = overtimeMins;
                    existingRec.LateArrivalMinutes = lateArrivalMins;
                    existingRec.EarlyExitMinutes = earlyExitMins;
                    if (!existingRec.IsRegularized)
                    {
                        existingRec.Status = status;
                    }
                    existingRec.LeaveRequestId = leaveReq?.Id;
                    existingRec.Remarks = remarks ?? existingRec.Remarks;
                    existingRec.UpdatedDate = DateTime.UtcNow;

                    _unitOfWork.DailyAttendanceRecords.Update(existingRec);
                }
                else
                {
                    var newRecord = new DailyAttendanceRecord
                    {
                        EmployeeId = emp.Id,
                        AttendanceDate = targetDate,
                        ShiftId = shift.Id,
                        FirstClockIn = firstIn,
                        LastClockOut = lastOut,
                        TotalWorkMinutes = totalWorkMins,
                        TotalBreakMinutes = totalBreakMins,
                        EffectiveWorkMinutes = effectiveMins,
                        OvertimeMinutes = overtimeMins,
                        LateArrivalMinutes = lateArrivalMins,
                        EarlyExitMinutes = earlyExitMins,
                        Status = status,
                        LeaveRequestId = leaveReq?.Id,
                        IsRegularized = false,
                        Remarks = remarks,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    };

                    await _unitOfWork.DailyAttendanceRecords.Add(newRecord);
                }

                computedCount++;
            }

            _unitOfWork.Save();
            return computedCount;
        }

        public async Task<int> ComputeDateRangeAsync(DateOnly fromDate, DateOnly toDate, long? specificEmployeeId = null)
        {
            if (fromDate > toDate)
                throw new ArgumentException("fromDate must be on or before toDate.");

            int total = 0;
            var cur = fromDate;
            while (cur <= toDate)
            {
                total += await ComputeDailyAttendanceAsync(cur, specificEmployeeId);
                cur = cur.AddDays(1);
            }
            return total;
        }

        public async Task<IEnumerable<DailyAttendanceRecordDTO>> GetDailyRecordsAsync(long? employeeId, int month, int year)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var query = (await _unitOfWork.DailyAttendanceRecords.GetAll())
                .Where(r => !r.IsDeleted && r.AttendanceDate >= startDate && r.AttendanceDate <= endDate);

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                query = query.Where(r => r.EmployeeId == employeeId.Value);
            }

            var records = query.OrderBy(r => r.AttendanceDate).ThenBy(r => r.EmployeeId).ToList();

            var empMap = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => e);
            var shiftMap = (await _unitOfWork.ShiftRepository.GetAll()).ToDictionary(s => s.Id, s => s.ShiftName);

            return records.Select(r =>
            {
                empMap.TryGetValue(r.EmployeeId, out var emp);
                string shiftName = r.ShiftId.HasValue && shiftMap.TryGetValue(r.ShiftId.Value, out var sName) ? sName : "General";

                return new DailyAttendanceRecordDTO
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    EmployeeCode = emp?.EmployeeCode?.ToString() ?? "",
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"Employee #{r.EmployeeId}",
                    AttendanceDate = r.AttendanceDate.ToString("yyyy-MM-dd"),
                    ShiftId = r.ShiftId,
                    ShiftName = shiftName,
                    FirstClockIn = r.FirstClockIn,
                    LastClockOut = r.LastClockOut,
                    TotalWorkMinutes = r.TotalWorkMinutes,
                    TotalBreakMinutes = r.TotalBreakMinutes,
                    EffectiveWorkMinutes = r.EffectiveWorkMinutes,
                    OvertimeMinutes = r.OvertimeMinutes,
                    LateArrivalMinutes = r.LateArrivalMinutes,
                    EarlyExitMinutes = r.EarlyExitMinutes,
                    Status = r.Status,
                    LeaveRequestId = r.LeaveRequestId,
                    IsRegularized = r.IsRegularized,
                    AttendanceLockId = r.AttendanceLockId,
                    Remarks = r.Remarks
                };
            });
        }

        public async Task<AttendanceMonthlySummaryDTO> GetMonthlySummaryAsync(long employeeId, int month, int year)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            int totalDays = endDate.Day;

            var records = (await _unitOfWork.DailyAttendanceRecords.GetAll())
                .Where(r => !r.IsDeleted && r.EmployeeId == employeeId && r.AttendanceDate >= startDate && r.AttendanceDate <= endDate)
                .ToList();

            var emp = await _unitOfWork.Employees.GetById(employeeId);
            bool isLocked = await _lockService.IsMonthLockedAsync(month, year);

            decimal presentDays = records.Count(r => string.Equals(r.Status, "Present", StringComparison.OrdinalIgnoreCase) ||
                                                     string.Equals(r.Status, "WFH", StringComparison.OrdinalIgnoreCase));

            decimal halfDays = records.Count(r => string.Equals(r.Status, "HalfDay", StringComparison.OrdinalIgnoreCase));
            decimal absentDays = records.Count(r => string.Equals(r.Status, "Absent", StringComparison.OrdinalIgnoreCase) ||
                                                    string.Equals(r.Status, "MissedPunch", StringComparison.OrdinalIgnoreCase));
            int lateMarks = records.Count(r => string.Equals(r.Status, "Late", StringComparison.OrdinalIgnoreCase) || r.LateArrivalMinutes > 0);

            // 3 Late Marks = 0.5 Day deduction penalty
            decimal latePenaltyDays = (lateMarks / 3) * 0.5m;

            decimal onLeaveDays = records.Count(r => string.Equals(r.Status, "OnLeave", StringComparison.OrdinalIgnoreCase));
            decimal weekOffDays = records.Count(r => string.Equals(r.Status, "WeekOff", StringComparison.OrdinalIgnoreCase));
            decimal holidayDays = records.Count(r => string.Equals(r.Status, "Holiday", StringComparison.OrdinalIgnoreCase));

            decimal totalHours = Math.Round((decimal)records.Sum(r => r.EffectiveWorkMinutes) / 60m, 2);

            return new AttendanceMonthlySummaryDTO
            {
                EmployeeId = employeeId,
                EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : $"Employee #{employeeId}",
                Month = month,
                Year = year,
                TotalDaysInMonth = totalDays,
                PresentDays = presentDays,
                HalfDays = halfDays,
                AbsentDays = absentDays,
                LateMarksCount = lateMarks,
                LatePenaltyDays = latePenaltyDays,
                OnLeaveDays = onLeaveDays,
                WeekOffDays = weekOffDays,
                HolidayDays = holidayDays,
                TotalEffectiveHours = totalHours,
                IsMonthLocked = isLocked
            };
        }

        public async Task<int> RunAutoClockOutAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var openRecords = (await _unitOfWork.EmployeeAttendance.GetAll())
                .Where(a => !a.IsDeleted && a.ClockIn != default && (!a.ClockOut.HasValue || a.ClockOut.Value == DateTime.MinValue))
                .ToList();

            if (!openRecords.Any()) return 0;

            int count = 0;
            var shifts = (await _unitOfWork.ShiftRepository.GetAll()).ToDictionary(s => s.Id, s => s);

            foreach (var att in openRecords)
            {
                // Fetch shift or default to 18:00
                var shiftEndTime = ParseShiftTime("18:00", att.AttendanceDate);
                var bufferHours = 4;

                var bufferLimit = shiftEndTime.AddHours(bufferHours);
                if (DateTime.Now > bufferLimit)
                {
                    att.ClockOut = shiftEndTime;
                    att.TotalHours = (decimal)(att.ClockOut.Value - att.ClockIn).TotalHours;
                    att.EffectiveHours = Math.Max(0, att.TotalHours - 1);
                    att.UpdatedDate = DateTime.Now;
                    _unitOfWork.EmployeeAttendance.Update(att);
                    count++;
                }
            }

            if (count > 0)
            {
                _unitOfWork.Save();
                // Recompute daily attendance for updated records
                foreach (var att in openRecords.Where(a => a.ClockOut.HasValue))
                {
                    await ComputeDailyAttendanceAsync(att.AttendanceDate, att.EmployeeId);
                }
            }

            return count;
        }

        private static bool IsDateWeekOff(DateOnly date, string pattern)
        {
            var dayName = date.DayOfWeek.ToString();
            var parts = pattern.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return parts.Any(p => string.Equals(p, dayName, StringComparison.OrdinalIgnoreCase));
        }

        private static DateTime ParseShiftTime(string timeStr, DateOnly targetDate)
        {
            var baseDate = targetDate.ToDateTime(TimeOnly.MinValue);
            if (TimeOnly.TryParse(timeStr, CultureInfo.InvariantCulture, out var t))
            {
                return baseDate.Add(t.ToTimeSpan());
            }
            return baseDate.AddHours(18); // Default fallback 18:00
        }
    }
}
