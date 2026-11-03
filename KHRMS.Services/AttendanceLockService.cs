using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using Serilog;

namespace KHRMS.Services
{
    public class AttendanceLockService : IAttendanceLockService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService? _auditService;

        public AttendanceLockService(IUnitOfWork unitOfWork, IAuditService? auditService = null)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
        }

        public async Task<IEnumerable<AttendanceLockDTO>> GetAllLocksAsync()
        {
            var locks = (await _unitOfWork.AttendanceLocks.GetAll())
                .Where(l => !l.IsDeleted)
                .OrderByDescending(l => l.Year)
                .ThenByDescending(l => l.Month)
                .ToList();

            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            return locks.Select(l => new AttendanceLockDTO
            {
                Id = l.Id,
                Month = l.Month,
                Year = l.Year,
                TotalEmployeesLocked = l.TotalEmployeesLocked,
                TotalPresentDays = l.TotalPresentDays,
                TotalLopDays = l.TotalLopDays,
                TotalOvertimeHours = l.TotalOvertimeHours,
                IsLocked = l.IsLocked,
                LockedByEmployeeId = l.LockedByEmployeeId,
                LockedByEmployeeName = employees.TryGetValue(l.LockedByEmployeeId, out var lockedByName) ? lockedByName : $"Employee #{l.LockedByEmployeeId}",
                LockedDate = l.LockedDate,
                UnlockedReason = l.UnlockedReason,
                UnlockedByEmployeeId = l.UnlockedByEmployeeId,
                UnlockedByEmployeeName = l.UnlockedByEmployeeId.HasValue && employees.TryGetValue(l.UnlockedByEmployeeId.Value, out var unlockedByName) ? unlockedByName : null,
                UnlockedDate = l.UnlockedDate
            });
        }

        public async Task<AttendanceLockDTO?> GetLockForMonthAsync(int month, int year)
        {
            var lockRecord = (await _unitOfWork.AttendanceLocks.GetAll())
                .FirstOrDefault(l => l.Month == month && l.Year == year && !l.IsDeleted);

            if (lockRecord == null) return null;

            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            return new AttendanceLockDTO
            {
                Id = lockRecord.Id,
                Month = lockRecord.Month,
                Year = lockRecord.Year,
                TotalEmployeesLocked = lockRecord.TotalEmployeesLocked,
                TotalPresentDays = lockRecord.TotalPresentDays,
                TotalLopDays = lockRecord.TotalLopDays,
                TotalOvertimeHours = lockRecord.TotalOvertimeHours,
                IsLocked = lockRecord.IsLocked,
                LockedByEmployeeId = lockRecord.LockedByEmployeeId,
                LockedByEmployeeName = employees.TryGetValue(lockRecord.LockedByEmployeeId, out var lockedByName) ? lockedByName : $"Employee #{lockRecord.LockedByEmployeeId}",
                LockedDate = lockRecord.LockedDate,
                UnlockedReason = lockRecord.UnlockedReason,
                UnlockedByEmployeeId = lockRecord.UnlockedByEmployeeId,
                UnlockedByEmployeeName = lockRecord.UnlockedByEmployeeId.HasValue && employees.TryGetValue(lockRecord.UnlockedByEmployeeId.Value, out var unlockedByName) ? unlockedByName : null,
                UnlockedDate = lockRecord.UnlockedDate
            };
        }

        public async Task<bool> IsMonthLockedAsync(int month, int year)
        {
            var lockRecord = (await _unitOfWork.AttendanceLocks.GetAll())
                .FirstOrDefault(l => l.Month == month && l.Year == year && !l.IsDeleted);

            return lockRecord != null && lockRecord.IsLocked;
        }

        public async Task<AttendanceLockDTO> LockMonthAsync(int month, int year, long lockedByEmployeeId)
        {
            if (month < 1 || month > 12)
                throw new ArgumentException("Month must be between 1 and 12.");

            if (year < 2000 || year > 2100)
                throw new ArgumentException("Invalid year provided.");

            var existingLock = (await _unitOfWork.AttendanceLocks.GetAll())
                .FirstOrDefault(l => l.Month == month && l.Year == year && !l.IsDeleted);

            if (existingLock != null && existingLock.IsLocked)
            {
                throw new InvalidOperationException($"Attendance for {month:D2}/{year} is already locked.");
            }

            // Aggregate metrics from DailyAttendanceRecords
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var dailyRecords = (await _unitOfWork.DailyAttendanceRecords.GetAll())
                .Where(r => !r.IsDeleted && r.AttendanceDate >= startDate && r.AttendanceDate <= endDate)
                .ToList();

            int totalEmployees = dailyRecords.Select(r => r.EmployeeId).Distinct().Count();
            if (totalEmployees == 0)
            {
                totalEmployees = (await _unitOfWork.Employees.GetAll()).Count(e => !e.IsDeleted && e.IsActive);
            }

            decimal presentDays = dailyRecords.Count(r => string.Equals(r.Status, "Present", StringComparison.OrdinalIgnoreCase) ||
                                                          string.Equals(r.Status, "Late", StringComparison.OrdinalIgnoreCase) ||
                                                          string.Equals(r.Status, "WFH", StringComparison.OrdinalIgnoreCase))
                                  + (dailyRecords.Count(r => string.Equals(r.Status, "HalfDay", StringComparison.OrdinalIgnoreCase)) * 0.5m);

            decimal lopDays = dailyRecords.Count(r => string.Equals(r.Status, "Absent", StringComparison.OrdinalIgnoreCase));
            decimal overtimeHours = Math.Round((decimal)dailyRecords.Sum(r => r.OvertimeMinutes) / 60m, 2);

            AttendanceLock lockRecord;
            if (existingLock != null)
            {
                existingLock.IsLocked = true;
                existingLock.LockedByEmployeeId = lockedByEmployeeId;
                existingLock.LockedDate = DateTime.UtcNow;
                existingLock.TotalEmployeesLocked = totalEmployees;
                existingLock.TotalPresentDays = presentDays;
                existingLock.TotalLopDays = lopDays;
                existingLock.TotalOvertimeHours = overtimeHours;
                existingLock.UpdatedDate = DateTime.UtcNow;
                existingLock.UpdatedBy = (int)lockedByEmployeeId;
                _unitOfWork.AttendanceLocks.Update(existingLock);
                lockRecord = existingLock;
            }
            else
            {
                lockRecord = new AttendanceLock
                {
                    Month = month,
                    Year = year,
                    TotalEmployeesLocked = totalEmployees,
                    TotalPresentDays = presentDays,
                    TotalLopDays = lopDays,
                    TotalOvertimeHours = overtimeHours,
                    IsLocked = true,
                    LockedByEmployeeId = lockedByEmployeeId,
                    LockedDate = DateTime.UtcNow,
                    CreatedBy = (int)lockedByEmployeeId,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };
                await _unitOfWork.AttendanceLocks.Add(lockRecord);
            }

            _unitOfWork.Save();

            // Link lock ID to all computed records of that month
            foreach (var rec in dailyRecords)
            {
                rec.AttendanceLockId = lockRecord.Id;
                rec.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.DailyAttendanceRecords.Update(rec);
            }
            _unitOfWork.Save();

            if (_auditService != null)
            {
                await _auditService.LogAsync(
                    moduleName: "Attendance",
                    entityName: "AttendanceLock",
                    entityId: lockRecord.Id.ToString(),
                    action: "LOCK",
                    beforeValues: null,
                    afterValues: new { Month = month, Year = year, TotalEmployees = totalEmployees, PresentDays = presentDays, LopDays = lopDays },
                    reason: $"Month-end attendance for {month:D2}/{year} locked by Employee #{lockedByEmployeeId}."
                );
            }

            return (await GetLockForMonthAsync(month, year))!;
        }

        public async Task<AttendanceLockDTO> UnlockMonthAsync(int month, int year, long unlockedByEmployeeId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 5)
            {
                throw new ArgumentException("A valid justification reason of at least 5 characters is mandatory to unlock attendance.");
            }

            var lockRecord = (await _unitOfWork.AttendanceLocks.GetAll())
                .FirstOrDefault(l => l.Month == month && l.Year == year && !l.IsDeleted);

            if (lockRecord == null || !lockRecord.IsLocked)
            {
                throw new InvalidOperationException($"Attendance for {month:D2}/{year} is not currently locked.");
            }

            var beforeState = new { lockRecord.IsLocked, lockRecord.LockedByEmployeeId, lockRecord.LockedDate };

            lockRecord.IsLocked = false;
            lockRecord.UnlockedReason = reason.Trim();
            lockRecord.UnlockedByEmployeeId = unlockedByEmployeeId;
            lockRecord.UnlockedDate = DateTime.UtcNow;
            lockRecord.UpdatedDate = DateTime.UtcNow;
            lockRecord.UpdatedBy = (int)unlockedByEmployeeId;

            _unitOfWork.AttendanceLocks.Update(lockRecord);
            _unitOfWork.Save();

            if (_auditService != null)
            {
                await _auditService.LogAsync(
                    moduleName: "Attendance",
                    entityName: "AttendanceLock",
                    entityId: lockRecord.Id.ToString(),
                    action: "UNLOCK",
                    beforeValues: beforeState,
                    afterValues: new { lockRecord.IsLocked, lockRecord.UnlockedByEmployeeId, lockRecord.UnlockedDate, lockRecord.UnlockedReason },
                    reason: reason.Trim()
                );
            }

            return (await GetLockForMonthAsync(month, year))!;
        }
    }
}
