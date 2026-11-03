using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KHRMS.Services
{
    public class LeaveLedgerService : ILeaveLedgerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeaveLedgerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EmployeeLeaveBalanceDto> GetBalanceSummaryAsync(long employeeId, long leaveTypeId)
        {
            var lt = await _unitOfWork.LeaveType.GetById(leaveTypeId);
            if (lt == null)
            {
                return new EmployeeLeaveBalanceDto { LeaveTypeId = leaveTypeId };
            }

            // Auto-heal any dangling reservations or workflow-cancelled requests first
            await AutoReleaseDanglingReservationsAsync(employeeId);

            // Ensure baseline ledger records exist for this employee & leave type
            await EnsureLedgerInitializedAsync(employeeId, lt);

            var allTransactions = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => l.EmployeeId == employeeId && l.LeaveTypeId == leaveTypeId && l.IsDeleted != true)
                .OrderBy(l => l.CreatedDate)
                .ToList();

            int currentYear = DateTime.UtcNow.Year;

            // Synchronize baseline opening balance if Admin adjusted AllowedDays on non-accrual policy
            var openingTx = allTransactions.FirstOrDefault(tx => tx.TransactionType == "OpeningBalance" && tx.EffectiveDate.Year == currentYear);
            if (openingTx != null && lt.AllowedDays >= 0 && !lt.AccrualEnabled && openingTx.Days != (decimal)lt.AllowedDays)
            {
                decimal diff = (decimal)lt.AllowedDays - openingTx.Days;
                openingTx.Days = (decimal)lt.AllowedDays;
                openingTx.BalanceAfter += diff;
                _unitOfWork.LeaveLedgers.Update(openingTx);

                var baselineEnt = (await _unitOfWork.LeaveEntitlements.GetAll())
                    .FirstOrDefault(e => e.EmployeeId == employeeId && e.LeaveTypeId == leaveTypeId && e.SourceType == "OpeningBalance" && e.CreditDate.Year == currentYear && e.IsDeleted != true);
                if (baselineEnt != null)
                {
                    baselineEnt.TotalDays = (decimal)lt.AllowedDays;
                    baselineEnt.RemainingDays = Math.Max(0, (decimal)lt.AllowedDays - baselineEnt.ConsumedDays);
                    _unitOfWork.LeaveEntitlements.Update(baselineEnt);
                }
                _unitOfWork.Save();

                // Re-fetch transactions
                allTransactions = (await _unitOfWork.LeaveLedgers.GetAll())
                    .Where(l => l.EmployeeId == employeeId && l.LeaveTypeId == leaveTypeId && l.IsDeleted != true)
                    .OrderBy(l => l.CreatedDate)
                    .ToList();
            }

            var leaveRequests = (await _unitOfWork.LeaveRequest.GetAll())
                .Where(r => r.EmployeeId == employeeId && r.LeaveTypeId == leaveTypeId && r.IsDeleted != true && r.IsActive != false)
                .ToList();

            // Calculate confirmed balance (excluding pending reservations)
            decimal confirmedBalance = 0;
            decimal accruedYtd = 0;
            decimal usedYtd = 0;
            decimal carriedForward = 0;

            foreach (var tx in allTransactions)
            {
                if (tx.TransactionType == "Reservation" || tx.TransactionType == "ReservationRelease")
                {
                    continue; // Skip reservations when summing confirmed balance
                }

                confirmedBalance += tx.Days;

                if (tx.TransactionType == "Accrual" || tx.TransactionType == "OpeningBalance")
                {
                    if (tx.EffectiveDate.Year == currentYear)
                    {
                        accruedYtd += tx.Days;
                    }
                }
                else if (tx.TransactionType == "CarryForward")
                {
                    if (tx.EffectiveDate.Year == currentYear)
                    {
                        carriedForward += tx.Days;
                    }
                }
                else if (tx.TransactionType == "Deduction" || tx.TransactionType == "Encashment" || tx.TransactionType == "LOPToPaidConversion")
                {
                    if (tx.EffectiveDate.Year == currentYear)
                    {
                        usedYtd += Math.Abs(tx.Days);
                    }
                }
                else if (tx.TransactionType == "Restoration")
                {
                    if (tx.EffectiveDate.Year == currentYear)
                    {
                        usedYtd = Math.Max(0, usedYtd - tx.Days);
                    }
                }
            }

            // Cap baseline credits to AllowedDays if policy has a fixed quota
            if (lt.AllowedDays > 0)
            {
                if (!lt.AccrualEnabled)
                {
                    accruedYtd = Math.Min(accruedYtd, (decimal)lt.AllowedDays);
                    decimal baselineBalance = confirmedBalance - carriedForward;
                    if (baselineBalance > (decimal)lt.AllowedDays)
                    {
                        confirmedBalance = (decimal)lt.AllowedDays + carriedForward - usedYtd;
                    }
                }
                else
                {
                    accruedYtd = Math.Min(accruedYtd, (decimal)lt.AllowedDays);
                }
            }

            // Active pending reservations & approved used days
            decimal reservedDays = 0;
            decimal approvedDaysFromRequests = 0;

            if (lt.IsPaid)
            {
                var pendingRequests = leaveRequests
                    .Where(r => string.Equals(r.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                reservedDays = pendingRequests
                    .Where(r => !r.IsLop || r.PaidDays > 0)
                    .Sum(r => r.PaidDays > 0 ? r.PaidDays : (!r.IsLop ? (r.TotalWorkingDays > 0 ? r.TotalWorkingDays : (decimal)r.TotalDays) : 0));

                approvedDaysFromRequests = leaveRequests
                    .Where(r => string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase) && r.StartDate.Year == currentYear && (!r.IsLop || r.PaidDays > 0))
                    .Sum(r => r.PaidDays > 0 ? r.PaidDays : (!r.IsLop ? (r.TotalWorkingDays > 0 ? r.TotalWorkingDays : (decimal)r.TotalDays) : 0));

                usedYtd = Math.Max(usedYtd, approvedDaysFromRequests);
            }
            else
            {
                // Unpaid / Loss of Pay leave type:
                // Sum pending and approved LOP days from requests for this leave type or requests with LopDays/IsLop
                var allEmpRequests = (await _unitOfWork.LeaveRequest.GetAll())
                    .Where(r => r.EmployeeId == employeeId && r.IsDeleted != true && r.IsActive != false)
                    .ToList();

                reservedDays = allEmpRequests
                    .Where(r => string.Equals(r.Status, "Pending", StringComparison.OrdinalIgnoreCase) &&
                                (r.LeaveTypeId == leaveTypeId || r.IsLop || r.LopDays > 0))
                    .Sum(r => r.LeaveTypeId == leaveTypeId
                        ? (r.LopDays > 0 ? r.LopDays : (r.TotalWorkingDays > 0 ? r.TotalWorkingDays : (decimal)r.TotalDays))
                        : r.LopDays);

                approvedDaysFromRequests = allEmpRequests
                    .Where(r => string.Equals(r.Status, "Approved", StringComparison.OrdinalIgnoreCase) &&
                                r.StartDate.Year == currentYear &&
                                (r.LeaveTypeId == leaveTypeId || r.IsLop || r.LopDays > 0))
                    .Sum(r => r.LeaveTypeId == leaveTypeId
                        ? (r.LopDays > 0 ? r.LopDays : (r.TotalWorkingDays > 0 ? r.TotalWorkingDays : (decimal)r.TotalDays))
                        : r.LopDays);

                usedYtd = approvedDaysFromRequests;
            }

            decimal availableDays = lt.IsPaid ? Math.Max(0, confirmedBalance - reservedDays) : 0;

            return new EmployeeLeaveBalanceDto
            {
                LeaveTypeId = lt.Id,
                LeaveTypeName = lt.Type ?? "Leave",
                TotalQuota = lt.AllowedDays,
                RemainingDays = (double)Math.Max(0, confirmedBalance),
                UsedDays = (double)usedYtd,
                PendingDays = (int)Math.Ceiling(reservedDays),
                IsPaid = lt.IsPaid,
                AccruedDays = (double)accruedYtd,
                CarriedForwardDays = (double)carriedForward,
                ReservedDays = (double)reservedDays,
                AvailableDays = (double)availableDays,
                NegativeBalanceAllowed = lt.IsNegativeBalanceAllowed,
                NegativeBalanceLimit = lt.NegativeBalanceLimit,
                EncashmentAllowed = lt.EncashmentAllowed,
                IsCompOff = lt.IsCompOff,
                SandwichRuleEnabled = lt.SandwichRuleEnabled,
                MaxSandwichLeavesAllowed = lt.MaxSandwichLeavesAllowed
            };
        }

        public async Task<IEnumerable<EmployeeLeaveBalanceDto>> GetAllEmployeeBalancesAsync(long employeeId)
        {
            var leaveTypes = (await _unitOfWork.LeaveType.GetAll())
                .Where(t => t.IsActive != false && t.IsDeleted != true)
                .ToList();

            var result = new List<EmployeeLeaveBalanceDto>();
            foreach (var lt in leaveTypes)
            {
                var summary = await GetBalanceSummaryAsync(employeeId, lt.Id);
                result.Add(summary);
            }

            return result;
        }

        public async Task<LeaveLedger> RecordTransactionAsync(
            long employeeId,
            long leaveTypeId,
            string transactionType,
            decimal days,
            long? leaveRequestId = null,
            string? period = null,
            string? remarks = null,
            long createdBy = 0)
        {
            // Get current confirmed balance
            var existingTx = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => l.EmployeeId == employeeId && l.LeaveTypeId == leaveTypeId && l.IsDeleted != true &&
                            l.TransactionType != "Reservation" && l.TransactionType != "ReservationRelease")
                .ToList();

            decimal currentConfirmed = existingTx.Sum(t => t.Days);

            // Get current active reservations
            var allTx = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => l.EmployeeId == employeeId && l.LeaveTypeId == leaveTypeId && l.IsDeleted != true)
                .ToList();

            decimal currentReserved = Math.Abs(allTx.Where(t => t.TransactionType == "Reservation").Sum(t => t.Days)) -
                                      allTx.Where(t => t.TransactionType == "ReservationRelease").Sum(t => t.Days);
            currentReserved = Math.Max(0, currentReserved);

            decimal newBalance = 0;
            if (transactionType == "Reservation")
            {
                decimal newReserved = currentReserved + Math.Abs(days);
                newBalance = Math.Max(0, currentConfirmed - newReserved);
            }
            else if (transactionType == "ReservationRelease")
            {
                decimal newReserved = Math.Max(0, currentReserved - Math.Abs(days));
                newBalance = Math.Max(0, currentConfirmed - newReserved);
            }
            else
            {
                decimal newConfirmed = currentConfirmed + days;
                newBalance = Math.Max(0, newConfirmed - currentReserved);
            }

            var ledgerEntry = new LeaveLedger
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                TransactionType = transactionType,
                Days = days,
                BalanceAfter = newBalance,
                LeaveRequestId = leaveRequestId,
                EffectiveDate = DateTime.UtcNow,
                Period = period ?? $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month:D2}",
                Remarks = remarks,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.LeaveLedgers.Add(ledgerEntry);
            _unitOfWork.Save();

            // Update FEFO entitlements if this is a credit or deduction
            if (days > 0 && (transactionType == "Accrual" || transactionType == "OpeningBalance" ||
                             transactionType == "CarryForward" || transactionType == "CompOffCredit" || transactionType == "Restoration"))
            {
                var lt = await _unitOfWork.LeaveType.GetById(leaveTypeId);
                DateTime? expiry = null;
                if (lt?.IsCompOff == true)
                {
                    expiry = DateTime.UtcNow.AddDays(lt.CompOffValidityDays > 0 ? lt.CompOffValidityDays : 90);
                }
                else if (lt?.CarryForwardEnabled == false)
                {
                    expiry = new DateTime(DateTime.UtcNow.Year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                }

                await _unitOfWork.LeaveEntitlements.Add(new LeaveEntitlement
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = leaveTypeId,
                    SourceType = transactionType,
                    SourceReferenceId = ledgerEntry.Id,
                    TotalDays = days,
                    ConsumedDays = 0,
                    RemainingDays = days,
                    CreditDate = DateTime.UtcNow,
                    ExpiryDate = expiry,
                    IsExpired = false,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            else if (days < 0 && (transactionType == "Deduction" || transactionType == "Encashment" || transactionType == "LOPToPaidConversion"))
            {
                // FEFO consumption
                decimal daysToConsume = Math.Abs(days);
                var activeEntitlements = (await _unitOfWork.LeaveEntitlements.GetAll())
                    .Where(e => e.EmployeeId == employeeId && e.LeaveTypeId == leaveTypeId && !e.IsExpired && e.RemainingDays > 0 && e.IsDeleted != true)
                    .OrderBy(e => e.ExpiryDate ?? DateTime.MaxValue)
                    .ThenBy(e => e.CreditDate)
                    .ToList();

                foreach (var ent in activeEntitlements)
                {
                    if (daysToConsume <= 0) break;

                    decimal take = Math.Min(ent.RemainingDays, daysToConsume);
                    ent.ConsumedDays += take;
                    ent.RemainingDays -= take;
                    daysToConsume -= take;
                    ent.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.LeaveEntitlements.Update(ent);
                }
                _unitOfWork.Save();
            }

            return ledgerEntry;
        }

        public async Task<bool> ReserveBalanceAsync(long employeeId, long leaveTypeId, decimal days, long leaveRequestId)
        {
            if (days <= 0) return true;

            await RecordTransactionAsync(
                employeeId,
                leaveTypeId,
                "Reservation",
                -days,
                leaveRequestId,
                null,
                $"Reserved {days} days for Leave Request #{leaveRequestId}");

            return true;
        }

        public async Task<bool> CommitDeductionAsync(long employeeId, long leaveTypeId, decimal days, long leaveRequestId)
        {
            if (days <= 0) return true;

            // First release reservation entry if one was logged
            await RecordTransactionAsync(
                employeeId,
                leaveTypeId,
                "ReservationRelease",
                days,
                leaveRequestId,
                null,
                $"Released reservation on approval for Leave Request #{leaveRequestId}");

            // Commit final deduction
            await RecordTransactionAsync(
                employeeId,
                leaveTypeId,
                "Deduction",
                -days,
                leaveRequestId,
                null,
                $"Deducted {days} days for approved Leave Request #{leaveRequestId}");

            return true;
        }

        public async Task<bool> ReleaseReservationAsync(long employeeId, long leaveTypeId, decimal days, long leaveRequestId)
        {
            if (days <= 0) return true;

            await RecordTransactionAsync(
                employeeId,
                leaveTypeId,
                "ReservationRelease",
                days,
                leaveRequestId,
                null,
                $"Released reservation for rejected/cancelled Leave Request #{leaveRequestId}");

            return true;
        }

        public async Task<bool> RestoreDeductionAsync(long employeeId, long leaveTypeId, decimal days, long leaveRequestId, string reason)
        {
            if (days <= 0) return true;

            await RecordTransactionAsync(
                employeeId,
                leaveTypeId,
                "Restoration",
                days,
                leaveRequestId,
                null,
                $"Restored {days} days from cancelled Leave Request #{leaveRequestId}: {reason}");

            // Restore in most recent entitlement
            var latestEnt = (await _unitOfWork.LeaveEntitlements.GetAll())
                .Where(e => e.EmployeeId == employeeId && e.LeaveTypeId == leaveTypeId && e.IsDeleted != true && !e.IsExpired)
                .OrderByDescending(e => e.CreditDate)
                .FirstOrDefault();

            if (latestEnt != null)
            {
                latestEnt.RemainingDays += days;
                latestEnt.ConsumedDays = Math.Max(0, latestEnt.ConsumedDays - days);
                latestEnt.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.LeaveEntitlements.Update(latestEnt);
                _unitOfWork.Save();
            }

            return true;
        }

        public async Task<IEnumerable<LeaveLedger>> GetLedgerHistoryAsync(long employeeId, long? leaveTypeId = null, int? year = null)
        {
            // Auto-heal any dangling reservations or workflow-cancelled requests first
            await AutoReleaseDanglingReservationsAsync(employeeId);

            var query = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => l.EmployeeId == employeeId && l.IsDeleted != true);

            if (leaveTypeId.HasValue && leaveTypeId.Value > 0)
            {
                query = query.Where(l => l.LeaveTypeId == leaveTypeId.Value);
            }

            if (year.HasValue && year.Value > 0)
            {
                query = query.Where(l => l.EffectiveDate.Year == year.Value);
            }

            var leaveTypes = (await _unitOfWork.LeaveType.GetAll()).ToDictionary(t => t.Id);

            var list = query.OrderByDescending(l => l.CreatedDate).ToList();
            foreach (var item in list)
            {
                if (leaveTypes.TryGetValue(item.LeaveTypeId, out var lt))
                {
                    item.LeaveType = lt;
                }
            }

            return list;
        }

        public async Task<int> ProcessMonthlyAccrualAsync(int year, int month)
        {
            string period = $"{year}-{month:D2}";
            Log.Information("Starting Leave Accrual Worker for period: {Period}", period);

            var activeEmployees = (await _unitOfWork.Employees.GetAll())
                .Where(e => e.IsActive != false && e.IsDeleted != true && !string.Equals(e.EmploymentStatus, "Exited", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var accruableLeaveTypes = (await _unitOfWork.LeaveType.GetAll())
                .Where(t => t.IsActive != false && t.IsDeleted != true && t.AccrualEnabled && t.AccrualAmount > 0)
                .ToList();

            var allExistingAccruals = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => l.TransactionType == "Accrual" && l.Period == period && l.IsDeleted != true)
                .Select(l => $"{l.EmployeeId}_{l.LeaveTypeId}")
                .ToHashSet();

            int creditCount = 0;

            foreach (var emp in activeEmployees)
            {
                foreach (var lt in accruableLeaveTypes)
                {
                    string key = $"{emp.Id}_{lt.Id}";
                    if (allExistingAccruals.Contains(key))
                    {
                        continue; // Idempotent skip
                    }

                    // Check accrual frequency: Monthly, Quarterly, Annual
                    bool shouldAccrue = true;
                    if (string.Equals(lt.AccrualFrequency, "Quarterly", StringComparison.OrdinalIgnoreCase))
                    {
                        shouldAccrue = (month == 1 || month == 4 || month == 7 || month == 10);
                    }
                    else if (string.Equals(lt.AccrualFrequency, "Annual", StringComparison.OrdinalIgnoreCase))
                    {
                        shouldAccrue = (month == 1);
                    }

                    if (!shouldAccrue) continue;

                    decimal daysToCredit = lt.AccrualAmount;

                    // Check annual ceiling if AllowedDays is configured
                    if (lt.AllowedDays > 0)
                    {
                        var ytdTx = (await _unitOfWork.LeaveLedgers.GetAll())
                            .Where(l => l.EmployeeId == emp.Id && l.LeaveTypeId == lt.Id && l.TransactionType == "Accrual" &&
                                        l.EffectiveDate.Year == year && l.IsDeleted != true)
                            .Sum(l => l.Days);

                        if (ytdTx + daysToCredit > lt.AllowedDays)
                        {
                            daysToCredit = Math.Max(0, lt.AllowedDays - ytdTx);
                        }
                    }

                    if (daysToCredit <= 0) continue;

                    await RecordTransactionAsync(
                        emp.Id,
                        lt.Id,
                        "Accrual",
                        daysToCredit,
                        null,
                        period,
                        $"Periodic {lt.AccrualFrequency} accrual of {daysToCredit} days for {period}");

                    creditCount++;
                }
            }

            Log.Information("Leave Accrual Worker completed: {Count} credits processed for period {Period}", creditCount, period);
            return creditCount;
        }

        public async Task<int> ProcessYearEndRolloverAsync(int currentYear, int targetYear)
        {
            string rolloverPeriod = $"Rollover-{currentYear}-to-{targetYear}";
            Log.Information("Starting Leave Year-End Rollover Worker for {Period}", rolloverPeriod);

            var activeEmployees = (await _unitOfWork.Employees.GetAll())
                .Where(e => e.IsActive != false && e.IsDeleted != true && !string.Equals(e.EmploymentStatus, "Exited", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var leaveTypes = (await _unitOfWork.LeaveType.GetAll())
                .Where(t => t.IsActive != false && t.IsDeleted != true && !t.IsCompOff)
                .ToList();

            var existingRollovers = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => (l.TransactionType == "CarryForward" || l.TransactionType == "Lapse") && l.Period == rolloverPeriod && l.IsDeleted != true)
                .Select(l => $"{l.EmployeeId}_{l.LeaveTypeId}")
                .ToHashSet();

            int processedCount = 0;

            foreach (var emp in activeEmployees)
            {
                foreach (var lt in leaveTypes)
                {
                    string key = $"{emp.Id}_{lt.Id}";
                    if (existingRollovers.Contains(key))
                    {
                        continue; // Idempotent
                    }

                    var summary = await GetBalanceSummaryAsync(emp.Id, lt.Id);
                    decimal currentBalance = (decimal)summary.RemainingDays;
                    if (currentBalance <= 0) continue;

                    if (!lt.CarryForwardEnabled)
                    {
                        // Entire balance lapses
                        await RecordTransactionAsync(
                            emp.Id,
                            lt.Id,
                            "Lapse",
                            -currentBalance,
                            null,
                            rolloverPeriod,
                            $"Year-end unused balance lapsed (Carry Forward disabled for {lt.Type})");
                        processedCount++;
                    }
                    else
                    {
                        decimal carryDays = Math.Min(currentBalance, lt.MaxCarryForwardDays);
                        decimal excessDays = currentBalance - carryDays;

                        // Deduct old year balance to close year
                        await RecordTransactionAsync(
                            emp.Id,
                            lt.Id,
                            "YearEndClose",
                            -currentBalance,
                            null,
                            rolloverPeriod,
                            $"Year-end closing balance for {currentYear}");

                        // Credit carry forward into target year
                        await RecordTransactionAsync(
                            emp.Id,
                            lt.Id,
                            "CarryForward",
                            carryDays,
                            null,
                            $"Opening-{targetYear}",
                            $"Carried forward {carryDays} days from {currentYear}");

                        if (excessDays > 0)
                        {
                            if (string.Equals(lt.ExcessBalanceTreatment, "Encash", StringComparison.OrdinalIgnoreCase) && lt.EncashmentAllowed)
                            {
                                // Route to encashment
                                var salary = (await _unitOfWork.EmployeeSalaryAssignments.GetAll())
                                    .FirstOrDefault(s => s.EmployeeId == emp.Id && s.IsActive && !s.IsDeleted);

                                decimal monthlySalary = salary?.MonthlyGross ?? 30000m;
                                decimal dailyRate = Math.Round(monthlySalary / 30m, 2);

                                await _unitOfWork.LeaveEncashmentRequests.Add(new LeaveEncashmentRequest
                                {
                                    EmployeeId = emp.Id,
                                    LeaveTypeId = lt.Id,
                                    RequestedDays = excessDays,
                                    DailySalaryBasis = dailyRate,
                                    EstimatedAmount = excessDays * dailyRate,
                                    Reason = $"Automated year-end excess leave encashment ({excessDays} days)",
                                    Status = "Approved",
                                    ActionDate = DateTime.UtcNow,
                                    CreatedDate = DateTime.UtcNow,
                                    IsActive = true,
                                    IsDeleted = false
                                });
                                _unitOfWork.Save();

                                await RecordTransactionAsync(
                                    emp.Id,
                                    lt.Id,
                                    "Encashment",
                                    -excessDays,
                                    null,
                                    rolloverPeriod,
                                    $"Automated year-end encashment for {excessDays} excess days");
                            }
                            else
                            {
                                // Excess lapses
                                await RecordTransactionAsync(
                                    emp.Id,
                                    lt.Id,
                                    "Lapse",
                                    -excessDays,
                                    null,
                                    rolloverPeriod,
                                    $"Year-end excess balance lapsed ({excessDays} days above {lt.MaxCarryForwardDays} cap)");
                            }
                        }

                        processedCount++;
                    }
                }
            }

            Log.Information("Leave Year-End Rollover Worker completed: {Count} processed for period {Period}", processedCount, rolloverPeriod);
            return processedCount;
        }

        public async Task<int> ProcessCompOffExpiryAsync()
        {
            var now = DateTime.UtcNow;
            var expiredEntitlements = (await _unitOfWork.LeaveEntitlements.GetAll())
                .Where(e => e.SourceType == "CompOffCredit" && !e.IsExpired && e.RemainingDays > 0 &&
                            e.ExpiryDate.HasValue && e.ExpiryDate.Value.Date < now.Date && e.IsDeleted != true)
                .ToList();

            int expiredCount = 0;
            foreach (var ent in expiredEntitlements)
            {
                decimal daysToExpire = ent.RemainingDays;
                ent.IsExpired = true;
                ent.RemainingDays = 0;
                ent.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.LeaveEntitlements.Update(ent);

                await RecordTransactionAsync(
                    ent.EmployeeId,
                    ent.LeaveTypeId,
                    "CompOffExpiry",
                    -daysToExpire,
                    null,
                    $"{now.Year}-{now.Month:D2}",
                    $"Comp-Off credit expired after validity window ({daysToExpire} days)");

                expiredCount++;
            }

            if (expiredCount > 0)
            {
                _unitOfWork.Save();
                Log.Information("Comp-Off Expiry Worker expired {Count} unconsumed Comp-Off credits.", expiredCount);
            }

            return expiredCount;
        }

        private async Task EnsureLedgerInitializedAsync(long employeeId, LeaveType lt)
        {
            var existingTxs = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => l.EmployeeId == employeeId && l.LeaveTypeId == lt.Id && l.IsDeleted != true)
                .ToList();

            if (existingTxs.Count > 0)
            {
                var curOpening = existingTxs.FirstOrDefault(l => l.TransactionType == "OpeningBalance" && l.EffectiveDate.Year == DateTime.UtcNow.Year);
                if (curOpening != null && !lt.AccrualEnabled && lt.AllowedDays >= 0 && curOpening.Days != (decimal)lt.AllowedDays)
                {
                    decimal diff = (decimal)lt.AllowedDays - curOpening.Days;
                    curOpening.Days = (decimal)lt.AllowedDays;
                    curOpening.BalanceAfter += diff;
                    _unitOfWork.LeaveLedgers.Update(curOpening);

                    var baseEnt = (await _unitOfWork.LeaveEntitlements.GetAll())
                        .FirstOrDefault(e => e.EmployeeId == employeeId && e.LeaveTypeId == lt.Id && e.SourceType == "OpeningBalance" && e.CreditDate.Year == DateTime.UtcNow.Year && e.IsDeleted != true);
                    if (baseEnt != null)
                    {
                        baseEnt.TotalDays = (decimal)lt.AllowedDays;
                        baseEnt.RemainingDays = Math.Max(0, (decimal)lt.AllowedDays - baseEnt.ConsumedDays);
                        _unitOfWork.LeaveEntitlements.Update(baseEnt);
                    }
                    _unitOfWork.Save();
                }
                return;
            }

            // Seed opening balance or accrued amount so existing employees do not lose quotas
            decimal openingDays = 0;
            if (!lt.AccrualEnabled && lt.AllowedDays > 0)
            {
                openingDays = lt.AllowedDays;
            }
            else if (lt.AccrualEnabled && lt.AccrualAmount > 0)
            {
                // Prorate for elapsed months of the current year
                int currentMonth = DateTime.UtcNow.Month;
                openingDays = Math.Min(lt.AllowedDays > 0 ? lt.AllowedDays : 30, lt.AccrualAmount * currentMonth);
            }

            if (openingDays > 0)
            {
                await RecordTransactionAsync(
                    employeeId,
                    lt.Id,
                    "OpeningBalance",
                    openingDays,
                    null,
                    $"Opening-{DateTime.UtcNow.Year}",
                    $"Initial ledger opening balance for {lt.Type}");
            }
        }

        private async Task AutoReleaseDanglingReservationsAsync(long employeeId)
        {
            var leaveRequests = (await _unitOfWork.LeaveRequest.GetAll())
                .Where(r => r.EmployeeId == employeeId && r.IsDeleted != true)
                .ToList();

            // Synchronize any requests that have a completed/cancelled/rejected WorkflowInstance
            foreach (var req in leaveRequests.Where(r => string.Equals(r.Status, "Pending", StringComparison.OrdinalIgnoreCase) && r.WorkflowInstanceId.HasValue && r.WorkflowInstanceId.Value > 0))
            {
                var wf = await _unitOfWork.WorkflowInstances.GetById(req.WorkflowInstanceId!.Value);
                if (wf != null && (string.Equals(wf.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) || string.Equals(wf.Status, "Rejected", StringComparison.OrdinalIgnoreCase)))
                {
                    req.Status = wf.Status;
                    req.ActionDate = wf.CompletedDate ?? DateTime.UtcNow;
                    req.ActionBy = wf.FinalActionBy;
                    _unitOfWork.LeaveRequest.Update(req);
                    _unitOfWork.Save();
                }
            }

            // Re-fetch leave requests
            leaveRequests = (await _unitOfWork.LeaveRequest.GetAll())
                .Where(r => r.EmployeeId == employeeId && r.IsDeleted != true)
                .ToList();

            var cancelledOrRejectedIds = leaveRequests
                .Where(r => string.Equals(r.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(r.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                .Select(r => (long?)r.Id)
                .ToHashSet();

            if (cancelledOrRejectedIds.Count == 0) return;

            var allTransactions = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => l.EmployeeId == employeeId && l.IsDeleted != true)
                .ToList();

            var danglingReservations = allTransactions
                .Where(t => t.TransactionType == "Reservation" && t.LeaveRequestId.HasValue && cancelledOrRejectedIds.Contains(t.LeaveRequestId))
                .ToList();

            foreach (var dang in danglingReservations)
            {
                bool hasRelease = allTransactions.Any(t => (t.TransactionType == "ReservationRelease" || t.TransactionType == "Restoration") && t.LeaveRequestId == dang.LeaveRequestId);
                if (!hasRelease)
                {
                    await RecordTransactionAsync(
                        employeeId,
                        dang.LeaveTypeId,
                        "ReservationRelease",
                        Math.Abs(dang.Days),
                        dang.LeaveRequestId,
                        null,
                        $"Auto-released reservation for cancelled/rejected Leave Request #{dang.LeaveRequestId}");

                    // Re-query transactions for subsequent iterations
                    allTransactions = (await _unitOfWork.LeaveLedgers.GetAll())
                        .Where(l => l.EmployeeId == employeeId && l.LeaveTypeId == dang.LeaveTypeId && l.IsDeleted != true)
                        .ToList();
                }
            }
        }
    }
}
