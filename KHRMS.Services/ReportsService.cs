using System.Text;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;

namespace KHRMS.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ================= 1. ATTENDANCE SUMMARY REPORT =================
        public async Task<List<AttendanceReportItemDto>> GetAttendanceSummaryReportAsync(int month, int year, string? department = null)
        {
            var employees = (await _unitOfWork.Employees.GetAll())
                .Where(e => !e.IsDeleted && e.IsActive)
                .ToList();

            if (!string.IsNullOrEmpty(department) && !department.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                employees = employees.Where(e => (e.Department ?? e.Branch ?? "").Equals(department, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var dailyRecords = (await _unitOfWork.DailyAttendanceRecords.GetAll())
                .Where(d => d.AttendanceDate.Month == month && d.AttendanceDate.Year == year)
                .ToList();

            var holidays = (await _unitOfWork.Holidays.GetAll())
                .Where(h => !h.IsDeleted)
                .ToList();

            var holidayDates = holidays
                .Select(h => h.HolidayDate)
                .ToHashSet();

            int daysInMonth = DateTime.DaysInMonth(year, month);
            var result = new List<AttendanceReportItemDto>();

            foreach (var emp in employees.OrderBy(e => e.FirstName))
            {
                var empRecords = dailyRecords.Where(d => d.EmployeeId == emp.Id).ToList();

                var startDate = new DateTime(year, month, 1);
                var doj = emp.DateOfJoining.HasValue ? emp.DateOfJoining.Value.Date : startDate;
                var effectiveStart = doj > startDate ? doj : startDate;
                var effectiveEnd = new DateTime(year, month, daysInMonth);

                int expectedWorkingDays = 0;
                for (var dt = effectiveStart; dt <= effectiveEnd; dt = dt.AddDays(1))
                {
                    var dOnly = DateOnly.FromDateTime(dt);
                    if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday && !holidayDates.Contains(dOnly))
                    {
                        expectedWorkingDays++;
                    }
                }

                decimal presentDays = 0;
                decimal paidLeaveDays = 0;
                decimal unpaidLopDays = 0;
                int lateCount = 0;
                decimal totalHours = 0;

                foreach (var r in empRecords)
                {
                    totalHours += r.TotalWorkMinutes / 60m;
                    if (r.LateArrivalMinutes > 0 || r.Status.Equals("Late", StringComparison.OrdinalIgnoreCase)) lateCount++;

                    switch (r.Status)
                    {
                        case "Present":
                        case "Late":
                        case "WFH":
                            presentDays += 1m;
                            break;
                        case "HalfDay":
                            presentDays += 0.5m;
                            unpaidLopDays += 0.5m;
                            break;
                        case "OnLeave":
                        case "Leave":
                            paidLeaveDays += 1m;
                            break;
                        case "LOP":
                        case "Absent":
                        case "MissedPunch":
                            unpaidLopDays += 1m;
                            break;
                    }
                }

                // If daily records haven't run for some days, calculate residual
                decimal accountedDays = presentDays + paidLeaveDays + unpaidLopDays;
                if (accountedDays < expectedWorkingDays)
                {
                    unpaidLopDays += (expectedWorkingDays - accountedDays);
                }

                decimal punctualityRate = (presentDays + paidLeaveDays) > 0 
                    ? Math.Max(0, Math.Round((1m - ((decimal)lateCount / (presentDays + paidLeaveDays))) * 100m, 1)) 
                    : 100m;

                result.Add(new AttendanceReportItemDto
                {
                    EmployeeId = emp.Id,
                    EmployeeCode = emp.EmployeeCode?.ToString() ?? $"EMP-{emp.Id:D4}",
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    Department = emp.Department ?? emp.Branch ?? "General",
                    Designation = emp.Designation ?? "Staff",
                    Month = month,
                    Year = year,
                    TotalCalendarDays = daysInMonth,
                    ExpectedWorkingDays = expectedWorkingDays,
                    PresentDays = presentDays,
                    PaidLeaveDays = paidLeaveDays,
                    UnpaidLopDays = unpaidLopDays,
                    LateArrivalsCount = lateCount,
                    TotalWorkHours = Math.Round(totalHours, 1),
                    PunctualityRatePercent = punctualityRate
                });
            }

            return result;
        }

        public async Task<byte[]> ExportAttendanceSummaryCsvAsync(int month, int year, string? department = null)
        {
            var data = await GetAttendanceSummaryReportAsync(month, year, department);
            var sb = new StringBuilder();

            sb.AppendLine("Employee Code,Employee Name,Department,Designation,Month/Year,Expected Days,Present Days,Paid Leaves,Unpaid LOP,Late Marks,Total Work Hours,Punctuality Rate %");
            foreach (var r in data)
            {
                sb.AppendLine($"\"{r.EmployeeCode}\",\"{r.EmployeeName}\",\"{r.Department}\",\"{r.Designation}\",\"{r.Month}/{r.Year}\",{r.ExpectedWorkingDays},{r.PresentDays},{r.PaidLeaveDays},{r.UnpaidLopDays},{r.LateArrivalsCount},{r.TotalWorkHours},{r.PunctualityRatePercent}%");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // ================= 2. LEAVE BALANCE & UTILIZATION REPORT =================
        public async Task<List<LeaveBalanceReportItemDto>> GetLeaveBalanceReportAsync(string? department = null, long? leaveTypeId = null)
        {
            var employees = (await _unitOfWork.Employees.GetAll())
                .Where(e => !e.IsDeleted && e.IsActive)
                .ToList();

            if (!string.IsNullOrEmpty(department) && !department.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                employees = employees.Where(e => (e.Department ?? e.Branch ?? "").Equals(department, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var leaveTypes = (await _unitOfWork.LeaveType.GetAll())
                .Where(l => !l.IsDeleted)
                .ToList();

            if (leaveTypeId.HasValue && leaveTypeId.Value > 0)
            {
                leaveTypes = leaveTypes.Where(l => l.Id == leaveTypeId.Value).ToList();
            }

            var currentYear = DateTime.UtcNow.Year;
            var allLedgers = (await _unitOfWork.LeaveLedgers.GetAll())
                .Where(l => !l.IsDeleted && l.EffectiveDate.Year == currentYear)
                .ToList();

            var result = new List<LeaveBalanceReportItemDto>();

            foreach (var emp in employees.OrderBy(e => e.FirstName))
            {
                foreach (var lt in leaveTypes)
                {
                    var empLedgers = allLedgers.Where(l => l.EmployeeId == emp.Id && l.LeaveTypeId == lt.Id).ToList();

                    decimal quota = (decimal)lt.AllowedDays;
                    decimal opening = empLedgers.Where(l => l.TransactionType == "OpeningBalance").Sum(l => l.Days);
                    decimal accrued = empLedgers.Where(l => l.TransactionType == "Accrual").Sum(l => l.Days) + (opening > 0 ? opening : quota);
                    decimal carried = empLedgers.Where(l => l.TransactionType == "CarryForward").Sum(l => l.Days);
                    decimal consumed = Math.Abs(empLedgers.Where(l => l.TransactionType == "Consumption").Sum(l => l.Days));
                    decimal pending = Math.Max(0, Math.Abs(empLedgers.Where(l => l.TransactionType == "Reservation").Sum(l => l.Days)) -
                                                  empLedgers.Where(l => l.TransactionType == "ReservationRelease").Sum(l => l.Days));
                    decimal available = Math.Max(0, (accrued + carried) - consumed - pending);
                    decimal lop = Math.Abs(empLedgers.Where(l => l.TransactionType == "LossOfPay").Sum(l => l.Days));

                    result.Add(new LeaveBalanceReportItemDto
                    {
                        EmployeeId = emp.Id,
                        EmployeeCode = emp.EmployeeCode?.ToString() ?? $"EMP-{emp.Id:D4}",
                        EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                        Department = emp.Department ?? emp.Branch ?? "General",
                        LeaveType = lt.Type ?? "General Leave",
                        AnnualQuota = quota,
                        AccruedToDate = accrued,
                        CarriedForward = carried,
                        ConsumedDays = consumed,
                        PendingReservedDays = pending,
                        AvailableBalance = available,
                        UnpaidLopDays = lop
                    });
                }
            }

            return result;
        }

        public async Task<byte[]> ExportLeaveBalanceCsvAsync(string? department = null, long? leaveTypeId = null)
        {
            var data = await GetLeaveBalanceReportAsync(department, leaveTypeId);
            var sb = new StringBuilder();

            sb.AppendLine("Employee Code,Employee Name,Department,Leave Type,Annual Quota,Accrued To Date,Carried Forward,Consumed (Paid),Pending/Reserved,Available Balance,Loss of Pay (LOP)");
            foreach (var r in data)
            {
                sb.AppendLine($"\"{r.EmployeeCode}\",\"{r.EmployeeName}\",\"{r.Department}\",\"{r.LeaveType}\",{r.AnnualQuota},{r.AccruedToDate},{r.CarriedForward},{r.ConsumedDays},{r.PendingReservedDays},{r.AvailableBalance},{r.UnpaidLopDays}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // ================= 3. PAYROLL RECONCILIATION REPORT =================
        public async Task<List<PayrollReconciliationReportItemDto>> GetPayrollReconciliationReportAsync(long? payRunId = null, int? month = null, int? year = null, string? department = null)
        {
            var allPayRuns = (await _unitOfWork.PayRuns.GetAll())
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
                .ToList();

            PayRun? targetRun = null;
            if (payRunId.HasValue && payRunId.Value > 0)
            {
                targetRun = allPayRuns.FirstOrDefault(p => p.Id == payRunId.Value);
            }
            else if (month.HasValue && year.HasValue)
            {
                targetRun = allPayRuns.FirstOrDefault(p => p.Month == month.Value && p.Year == year.Value);
            }
            else
            {
                targetRun = allPayRuns.FirstOrDefault();
            }

            if (targetRun == null)
            {
                return new List<PayrollReconciliationReportItemDto>();
            }

            var details = (await _unitOfWork.PayRunEmployeeDetails.GetAll())
                .Where(d => d.PayRunId == targetRun.Id && !d.IsDeleted)
                .ToList();

            var employees = (await _unitOfWork.Employees.GetAll()).ToList();

            if (!string.IsNullOrEmpty(department) && !department.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var deptEmpIds = employees
                    .Where(e => (e.Department ?? e.Branch ?? "").Equals(department, StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Id)
                    .ToHashSet();

                details = details.Where(d => deptEmpIds.Contains(d.EmployeeId)).ToList();
            }

            var result = new List<PayrollReconciliationReportItemDto>();
            foreach (var d in details)
            {
                var emp = employees.FirstOrDefault(e => e.Id == d.EmployeeId);

                result.Add(new PayrollReconciliationReportItemDto
                {
                    EmployeeId = d.EmployeeId,
                    EmployeeCode = d.EmployeeCode,
                    EmployeeName = d.EmployeeName ?? (emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : "Staff"),
                    Department = emp?.Department ?? emp?.Branch ?? "General",
                    BankName = d.BankName ?? "Direct Deposit",
                    AccountNumber = d.AccountNumber?.ToString() ?? "",
                    IfscCode = d.IfscCode ?? "",
                    BasicSalary = d.BasicPay,
                    Hra = d.Hra,
                    SpecialAllowance = d.SpecialAllowance,
                    Reimbursements = d.Reimbursements,
                    GrossPay = d.GrossPay,
                    EpfDeduction = d.EpfDeduction,
                    EsicDeduction = d.EsicDeduction,
                    PtDeduction = d.PtDeduction,
                    TdsDeduction = d.TdsDeduction,
                    LopDeduction = d.LopDeduction,
                    LoanEmiDeduction = d.LoanEmiDeduction,
                    ManualAdjustments = d.ManualAdjustmentAmount + d.ArrearsAmount + d.BonusAmount,
                    TotalDeductions = d.TotalDeductions,
                    NetPay = d.NetPay,
                    PaymentStatus = d.PaymentStatus,
                    IsOnHold = d.IsOnHold,
                    HoldReason = d.HoldReason
                });
            }

            return result;
        }

        public async Task<byte[]> ExportPayrollReconciliationCsvAsync(long? payRunId = null, int? month = null, int? year = null, string? department = null)
        {
            var data = await GetPayrollReconciliationReportAsync(payRunId, month, year, department);
            var sb = new StringBuilder();

            sb.AppendLine("Employee Code,Employee Name,Department,Bank Name,Account Number,IFSC,Basic,HRA,Special Allowance,Reimbursements,Gross Pay,PF,ESI,PT,TDS,LOP Deduction,Loan EMI,Adjustments,Total Deductions,Net Take Home,Status,Hold Reason");
            foreach (var r in data)
            {
                sb.AppendLine($"\"{r.EmployeeCode}\",\"{r.EmployeeName}\",\"{r.Department}\",\"{r.BankName}\",\"{r.AccountNumber}\",\"{r.IfscCode}\",{r.BasicSalary},{r.Hra},{r.SpecialAllowance},{r.Reimbursements},{r.GrossPay},{r.EpfDeduction},{r.EsicDeduction},{r.PtDeduction},{r.TdsDeduction},{r.LopDeduction},{r.LoanEmiDeduction},{r.ManualAdjustments},{r.TotalDeductions},{r.NetPay},\"{r.PaymentStatus}\",\"{r.HoldReason ?? ""}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // ================= 4. HEADCOUNT & DEMOGRAPHICS REPORT =================
        public async Task<HeadcountDemographicsReportDto> GetHeadcountDemographicsReportAsync()
        {
            var allEmployees = (await _unitOfWork.Employees.GetAll())
                .Where(e => !e.IsDeleted)
                .ToList();

            var activeEmployees = allEmployees.Where(e => e.IsActive).ToList();
            var onProbation = activeEmployees.Where(e => (e.ProbationStatus ?? "").Equals("OnProbation", StringComparison.OrdinalIgnoreCase)).ToList();

            var currentYear = DateTime.UtcNow.Year;
            var exitedYtd = allEmployees.Where(e => !e.IsActive && e.UpdatedDate.Year == currentYear).ToList();

            var deptMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in activeEmployees)
            {
                var d = string.IsNullOrWhiteSpace(e.Department) ? (string.IsNullOrWhiteSpace(e.Branch) ? "General" : e.Branch) : e.Department;
                deptMap[d] = deptMap.GetValueOrDefault(d, 0) + 1;
            }

            var typeMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in activeEmployees)
            {
                var t = string.IsNullOrWhiteSpace(e.EmploymentType) ? "Full-Time" : e.EmploymentType;
                typeMap[t] = typeMap.GetValueOrDefault(t, 0) + 1;
            }

            decimal totalTenureYears = 0;
            int tenureCount = 0;
            var now = DateTime.UtcNow;

            foreach (var e in activeEmployees)
            {
                if (e.DateOfJoining.HasValue)
                {
                    var days = (now - e.DateOfJoining.Value).TotalDays;
                    if (days > 0)
                    {
                        totalTenureYears += (decimal)(days / 365.25);
                        tenureCount++;
                    }
                }
            }

            decimal avgTenure = tenureCount > 0 ? Math.Round(totalTenureYears / tenureCount, 1) : 1.0m;
            decimal turnover = activeEmployees.Any() ? Math.Round((decimal)exitedYtd.Count / activeEmployees.Count * 100m, 1) : 0m;

            return new HeadcountDemographicsReportDto
            {
                TotalActiveEmployees = activeEmployees.Count,
                TotalOnProbation = onProbation.Count,
                TotalExitedYearToDate = exitedYtd.Count,
                HeadcountByDepartment = deptMap,
                HeadcountByEmploymentType = typeMap,
                AverageTenureYears = avgTenure,
                AnnualTurnoverRatePercent = turnover
            };
        }

        public async Task<byte[]> ExportHeadcountDemographicsCsvAsync()
        {
            var data = await GetHeadcountDemographicsReportAsync();
            var sb = new StringBuilder();

            sb.AppendLine("Metric,Value");
            sb.AppendLine($"Total Active Headcount,{data.TotalActiveEmployees}");
            sb.AppendLine($"Personnel On Probation,{data.TotalOnProbation}");
            sb.AppendLine($"Exits Year-To-Date,{data.TotalExitedYearToDate}");
            sb.AppendLine($"Average Organization Tenure (Years),{data.AverageTenureYears}");
            sb.AppendLine($"Annual Turnover Rate %,{data.AnnualTurnoverRatePercent}%");
            sb.AppendLine();
            sb.AppendLine("Department,Headcount");
            foreach (var kvp in data.HeadcountByDepartment)
            {
                sb.AppendLine($"\"{kvp.Key}\",{kvp.Value}");
            }
            sb.AppendLine();
            sb.AppendLine("Employment Type,Headcount");
            foreach (var kvp in data.HeadcountByEmploymentType)
            {
                sb.AppendLine($"\"{kvp.Key}\",{kvp.Value}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
