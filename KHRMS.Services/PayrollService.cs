using System.Text;
using System.Text.Json;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KHRMS.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContext;

        public PayrollService(IUnitOfWork unitOfWork, IUserContextService userContext)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        #region Salary Structure & Components

        public async Task<IEnumerable<SalaryComponent>> GetAllComponentsAsync()
        {
            var components = (await _unitOfWork.SalaryComponents.GetAll()).ToList();
            if (!components.Any())
            {
                // Seed standard statutory and base components if empty
                var defaults = new List<SalaryComponent>
                {
                    new SalaryComponent { Name = "Basic Salary", Code = "BASIC", Type = "Earning", CalculationType = "PercentageOfGross", DefaultPercentage = 40, IsTaxable = true, IsStatutory = true, Description = "Base component of salary structure", IsActive = true },
                    new SalaryComponent { Name = "House Rent Allowance", Code = "HRA", Type = "Earning", CalculationType = "PercentageOfBasic", DefaultPercentage = 50, IsTaxable = true, IsStatutory = false, Description = "50% of Basic for metro / 40% non-metro", IsActive = true },
                    new SalaryComponent { Name = "Special Allowance", Code = "SA", Type = "Earning", CalculationType = "Flat", DefaultAmount = 0, IsTaxable = true, IsStatutory = false, Description = "Balancing earnings allowance", IsActive = true },
                    new SalaryComponent { Name = "Provident Fund (EPF)", Code = "EPF", Type = "Deduction", CalculationType = "PercentageOfBasic", DefaultPercentage = 12, IsTaxable = false, IsStatutory = true, Description = "Statutory 12% deduction on Basic", IsActive = true },
                    new SalaryComponent { Name = "Employee State Insurance (ESIC)", Code = "ESIC", Type = "Deduction", CalculationType = "PercentageOfGross", DefaultPercentage = 0.75m, IsTaxable = false, IsStatutory = true, Description = "0.75% for Gross <= 21000", IsActive = true },
                    new SalaryComponent { Name = "Professional Tax", Code = "PT", Type = "Deduction", CalculationType = "Flat", DefaultAmount = 200, IsTaxable = false, IsStatutory = true, Description = "State statutory deduction (₹200/month)", IsActive = true },
                    new SalaryComponent { Name = "Tax Deducted at Source (TDS)", Code = "TDS", Type = "Deduction", CalculationType = "Flat", DefaultAmount = 0, IsTaxable = false, IsStatutory = true, Description = "Income tax deduction", IsActive = true }
                };

                foreach (var d in defaults)
                {
                    d.CreatedDate = DateTime.UtcNow;
                    d.UpdatedDate = DateTime.UtcNow;
                    await _unitOfWork.SalaryComponents.Add(d);
                }
                _unitOfWork.Save();
                return await _unitOfWork.SalaryComponents.GetAll();
            }

            return components;
        }

        public async Task<SalaryComponent> AddSalaryComponentAsync(SalaryComponent component)
        {
            component.CreatedDate = DateTime.UtcNow;
            component.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.SalaryComponents.Add(component);
            _unitOfWork.Save();
            return component;
        }

        public async Task<SalaryComponent> UpdateSalaryComponentAsync(SalaryComponent component)
        {
            var existing = await _unitOfWork.SalaryComponents.GetById(component.Id);
            if (existing == null) throw new KeyNotFoundException($"Salary component {component.Id} not found");

            existing.Name = component.Name;
            existing.Code = component.Code;
            existing.Type = component.Type;
            existing.CalculationType = component.CalculationType;
            existing.DefaultPercentage = component.DefaultPercentage;
            existing.DefaultAmount = component.DefaultAmount;
            existing.IsTaxable = component.IsTaxable;
            existing.IsStatutory = component.IsStatutory;
            existing.Description = component.Description;
            existing.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.SalaryComponents.Update(existing);
            _unitOfWork.Save();
            return existing;
        }

        public async Task<bool> DeleteSalaryComponentAsync(long id)
        {
            var existing = await _unitOfWork.SalaryComponents.GetById(id);
            if (existing == null) return false;
            existing.IsDeleted = true;
            existing.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.SalaryComponents.Update(existing);
            _unitOfWork.Save();
            return true;
        }

        public SalaryCalculationResult CalculateCtcBreakdown(SalaryCalculationRequest req)
        {
            decimal annualCtc = Math.Max(0, req.AnnualCtc);
            decimal monthlyCtc = Math.Round(annualCtc / 12m, 2);

            // Basic: default 40% of CTC
            decimal basicPercent = req.BasicPercentage > 0 ? req.BasicPercentage : 40m;
            decimal basic = Math.Round(monthlyCtc * (basicPercent / 100m), 2);

            // HRA: default 50% of Basic
            decimal hraPercent = req.HraPercentage > 0 ? req.HraPercentage : 50m;
            decimal hra = Math.Round(basic * (hraPercent / 100m), 2);

            // PF: 12% of Basic (max 1800 if wage ceiling of 15,000 applies, or unconstrained)
            decimal epfEmployee = 0;
            decimal epfEmployer = 0;
            if (req.IncludePf)
            {
                decimal pfWage = Math.Min(basic, 15000m); // Statutory standard cap 15k, or basic
                epfEmployee = Math.Round(pfWage * 0.12m, 2);
                epfEmployer = epfEmployee;
            }

            // ESIC: Applicable only if gross <= 21,000
            decimal esicEmployee = 0;
            decimal esicEmployer = 0;
            if (req.IncludeEsic && (basic + hra) <= 21000m)
            {
                esicEmployee = Math.Round((basic + hra) * 0.0075m, 2);
                esicEmployer = Math.Round((basic + hra) * 0.0325m, 2);
            }

            // Professional Tax (Standard ₹200/mo)
            decimal pt = monthlyCtc > 15000m ? 200m : (monthlyCtc > 10000m ? 150m : 0m);

            // Special Allowance = Monthly CTC - (Basic + HRA + EpfEmployer + EsicEmployer)
            decimal specialAllowance = Math.Max(0, monthlyCtc - (basic + hra + epfEmployer + esicEmployer));
            decimal monthlyGross = basic + hra + specialAllowance;

            // TDS estimation (Indian New Tax Regime slabs)
            decimal taxableIncome = Math.Max(0, (monthlyGross * 12m) - 50000m); // ₹50k standard deduction
            decimal annualTds = 0;
            if (req.TaxRegime?.Equals("Old", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Old Regime: 2.5L to 5L (5%), 5L to 10L (20%), >10L (30%)
                if (taxableIncome > 1000000m)
                    annualTds = 112500m + ((taxableIncome - 1000000m) * 0.30m);
                else if (taxableIncome > 500000m)
                    annualTds = 12500m + ((taxableIncome - 500000m) * 0.20m);
                else if (taxableIncome > 250000m)
                    annualTds = (taxableIncome - 250000m) * 0.05m;
            }
            else
            {
                // New Regime 2024-25: Rebate up to ₹7,00,000
                if (taxableIncome > 700000m)
                {
                    if (taxableIncome > 1500000m)
                        annualTds = 150000m + ((taxableIncome - 1500000m) * 0.30m);
                    else if (taxableIncome > 1200000m)
                        annualTds = 90000m + ((taxableIncome - 1200000m) * 0.20m);
                    else if (taxableIncome > 900000m)
                        annualTds = 45000m + ((taxableIncome - 900000m) * 0.15m);
                    else if (taxableIncome > 600000m)
                        annualTds = 15000m + ((taxableIncome - 600000m) * 0.10m);
                    else if (taxableIncome > 300000m)
                        annualTds = (taxableIncome - 300000m) * 0.05m;

                    // 4% Health & Education cess
                    annualTds *= 1.04m;
                }
            }
            decimal monthlyTds = Math.Round(annualTds / 12m, 2);

            decimal totalDeductions = epfEmployee + esicEmployee + pt + monthlyTds;
            decimal takeHome = Math.Max(0, monthlyGross - totalDeductions);

            return new SalaryCalculationResult
            {
                AnnualCtc = annualCtc,
                MonthlyCtc = monthlyCtc,
                MonthlyGross = monthlyGross,
                BasicSalary = basic,
                Hra = hra,
                SpecialAllowance = specialAllowance,
                EpfEmployee = epfEmployee,
                EpfEmployer = epfEmployer,
                EsicEmployee = esicEmployee,
                EsicEmployer = esicEmployer,
                ProfessionalTax = pt,
                MonthlyTds = monthlyTds,
                TotalMonthlyDeductions = totalDeductions,
                MonthlyNetTakeHome = takeHome,
                TaxRegime = req.TaxRegime ?? "New"
            };
        }

        public async Task<EmployeeSalaryAssignment?> GetEmployeeSalaryAssignmentAsync(long employeeId)
        {
            var list = await _unitOfWork.EmployeeSalaryAssignments.GetAll();
            return list.Where(a => a.EmployeeId == employeeId && !a.IsDeleted)
                       .OrderByDescending(a => a.IsCurrent)
                       .ThenByDescending(a => a.EffectiveFromDate)
                       .FirstOrDefault();
        }

        public async Task<IEnumerable<EmployeeSalaryAssignment>> GetAllSalaryAssignmentsAsync()
        {
            var list = await _unitOfWork.EmployeeSalaryAssignments.GetAll();
            return list.Where(a => !a.IsDeleted && a.IsCurrent).ToList();
        }

        public async Task<IEnumerable<EmployeeSalaryAssignment>> GetEmployeeSalaryHistoryAsync(long employeeId)
        {
            var list = await _unitOfWork.EmployeeSalaryAssignments.GetAll();
            return list.Where(a => a.EmployeeId == employeeId && !a.IsDeleted)
                       .OrderByDescending(a => a.EffectiveFromDate)
                       .ThenByDescending(a => a.CreatedDate)
                       .ToList();
        }

        public async Task<EmployeeSalaryAssignment> AssignSalaryAsync(AssignSalaryRequest req)
        {
            var calc = CalculateCtcBreakdown(new SalaryCalculationRequest
            {
                AnnualCtc = req.AnnualCtc,
                TaxRegime = req.TaxRegime
            });

            var list = await _unitOfWork.EmployeeSalaryAssignments.GetAll();
            var existing = list.FirstOrDefault(a => a.EmployeeId == req.EmployeeId && !a.IsDeleted && a.IsCurrent);

            long currentUserId = _userContext.GetCurrentEmployeeId();

            if (existing != null)
            {
                existing.IsCurrent = false;
                existing.EffectiveToDate = req.EffectiveFromDate > existing.EffectiveFromDate ? req.EffectiveFromDate.AddDays(-1) : DateTime.UtcNow;
                existing.UpdatedBy = (int)currentUserId;
                existing.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.EmployeeSalaryAssignments.Update(existing);
                _unitOfWork.Save();
            }

            var newAssignment = new EmployeeSalaryAssignment
            {
                EmployeeId = req.EmployeeId,
                SalaryStructureId = req.SalaryStructureId,
                AnnualCtc = req.AnnualCtc,
                MonthlyGross = calc.MonthlyGross,
                BasicSalary = req.BasicSalary ?? calc.BasicSalary,
                Hra = req.Hra ?? calc.Hra,
                SpecialAllowance = req.SpecialAllowance ?? calc.SpecialAllowance,
                EpfEmployee = req.EpfEmployee ?? calc.EpfEmployee,
                EpfEmployer = req.EpfEmployer ?? calc.EpfEmployer,
                EsicEmployee = req.EsicEmployee ?? calc.EsicEmployee,
                EsicEmployer = req.EsicEmployer ?? calc.EsicEmployer,
                ProfessionalTax = req.ProfessionalTax ?? calc.ProfessionalTax,
                MonthlyTds = req.MonthlyTds ?? calc.MonthlyTds,
                MonthlyNetTakeHome = calc.MonthlyNetTakeHome,
                TaxRegime = req.TaxRegime,
                EffectiveFromDate = req.EffectiveFromDate,
                EffectiveToDate = null,
                IsCurrent = true,
                PreviousAssignmentId = existing?.Id,
                Remarks = req.Remarks,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.EmployeeSalaryAssignments.Add(newAssignment);
            _unitOfWork.Save();
            return newAssignment;
        }

        #endregion

        #region Pay Run Engine (Maker-Checker & LOP)

        public async Task<IEnumerable<PayRun>> GetAllPayRunsAsync()
        {
            var payruns = await _unitOfWork.PayRuns.GetAll();
            return payruns.Where(p => !p.IsDeleted).OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();
        }

        public async Task<PayRun?> GetPayRunByIdAsync(long id)
        {
            return await _unitOfWork.PayRuns.GetById(id);
        }

        public async Task<IEnumerable<PayRunEmployeeDetail>> GetPayRunDetailsAsync(long payRunId)
        {
            var details = await _unitOfWork.PayRunEmployeeDetails.GetAll();
            return details.Where(d => d.PayRunId == payRunId && !d.IsDeleted).ToList();
        }

        public async Task<PayRun> CreatePayRunAsync(CreatePayRunRequest req)
        {
            var now = DateTime.UtcNow;
            if (req.Year > now.Year || (req.Year == now.Year && req.Month > now.Month))
            {
                throw new InvalidOperationException($"Cannot run payroll for future period ({req.Month}/{req.Year}). Payroll can only be run for current or past months.");
            }

            var existingRuns = await _unitOfWork.PayRuns.GetAll();
            if (existingRuns.Any(p => p.Month == req.Month && p.Year == req.Year && !p.IsDeleted))
            {
                throw new InvalidOperationException($"A pay run for {req.Month}/{req.Year} already exists. Please discard the existing draft pay run first if you wish to regenerate it.");
            }

            int daysInMonth = DateTime.DaysInMonth(req.Year, req.Month);
            var startDate = new DateTime(req.Year, req.Month, 1);
            var endDate = new DateTime(req.Year, req.Month, daysInMonth);

            // FIN-01: Exclude official public holidays from standard working days so employees are not penalized
            var holidaysInMonth = (await _unitOfWork.Holidays.GetAll())
                .Where(h => !h.IsDeleted && !h.IsOptional &&
                            h.HolidayDate.Year == req.Year && h.HolidayDate.Month == req.Month)
                .Select(h => h.HolidayDate)
                .ToHashSet();

            // Calculate total working days (excluding Saturdays, Sundays, and public holidays)
            int standardWorkingDays = 0;
            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                var dOnly = DateOnly.FromDateTime(d);
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday && !holidaysInMonth.Contains(dOnly))
                {
                    standardWorkingDays++;
                }
            }

            // FIN-03: Fetch LeaveTypes to distinguish between Paid Leaves and Loss of Pay (LOP / LWP)
            var leaveTypesMap = (await _unitOfWork.LeaveType.GetAll())
                .Where(lt => !lt.IsDeleted)
                .ToDictionary(lt => lt.Id, lt => lt.Type ?? string.Empty);

            var employees = (await _unitOfWork.Employees.GetAll())
                .Where(e => !e.IsDeleted && (!e.DateOfJoining.HasValue || e.DateOfJoining.Value.Date <= endDate.Date))
                .ToList();
            var assignments = (await _unitOfWork.EmployeeSalaryAssignments.GetAll()).Where(a => !a.IsDeleted).ToList();
            var bankInfos = (await _unitOfWork.EmployeePaymentInfo.GetAll()).Where(b => !b.IsDeleted).ToList();
            var allAttendance = (await _unitOfWork.EmployeeAttendance.GetAll()).ToList();
            var allLeaves = (await _unitOfWork.LeaveRequest.GetAll())
                .Where(l => l.Status != null && l.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)).ToList();
            var pendingReimbursements = (await _unitOfWork.ReimbursementClaims.GetAll())
                .Where(r => !r.IsDeleted && r.Status == "Approved" && r.PayRunId == null).ToList();

            var activeLoans = (await _unitOfWork.EmployeeLoans.GetAll())
                .Where(l => l.Status == "Active" && l.RemainingBalance > 0 && !l.IsDeleted)
                .ToList();

            int priorMonth = req.Month == 1 ? 12 : req.Month - 1;
            int priorYear = req.Month == 1 ? req.Year - 1 : req.Year;
            var priorPayRun = (await _unitOfWork.PayRuns.GetAll())
                .FirstOrDefault(p => p.Month == priorMonth && p.Year == priorYear && !p.IsDeleted);
            var priorDetails = priorPayRun != null
                ? (await _unitOfWork.PayRunEmployeeDetails.GetAll()).Where(d => d.PayRunId == priorPayRun.Id && !d.IsDeleted).ToList()
                : new List<PayRunEmployeeDetail>();

            long currentUserId = _userContext.GetCurrentEmployeeId();

            var payRun = new PayRun
            {
                Month = req.Month,
                Year = req.Year,
                PayPeriodStart = startDate,
                PayPeriodEnd = endDate,
                Status = "Calculated",
                PaymentMode = req.PaymentMode ?? "Bank Transfer",
                Notes = req.Notes,
                ProcessedByEmployeeId = (int)currentUserId,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.PayRuns.Add(payRun);
            _unitOfWork.Save(); // save to get payRun.Id

            decimal totalGross = 0;
            decimal totalDeductions = 0;
            decimal totalNet = 0;
            int employeeCount = 0;

            foreach (var emp in employees)
            {
                var salary = assignments.FirstOrDefault(a => a.EmployeeId == emp.Id);
                // Fallback default CTC if not assigned: ₹3,60,000 (30,000/mo)
                decimal monthlyGross = salary?.MonthlyGross ?? 30000m;
                decimal basic = salary?.BasicSalary ?? (monthlyGross * 0.4m);
                decimal hra = salary?.Hra ?? (basic * 0.5m);
                decimal specialAllowance = salary?.SpecialAllowance ?? (monthlyGross - basic - hra);
                decimal epf = salary?.EpfEmployee ?? Math.Round(Math.Min(basic, 15000m) * 0.12m, 2);
                decimal esic = salary?.EsicEmployee ?? 0m;
                decimal pt = salary?.ProfessionalTax ?? 200m;
                decimal tds = salary?.MonthlyTds ?? 0m;

                // Attendance calculation
                var empAttendance = allAttendance.Where(a => a.EmployeeId == emp.Id &&
                    a.AttendanceDate.Year == req.Year && a.AttendanceDate.Month == req.Month).ToList();
                decimal presentDays = empAttendance.Count(a => a.ClockIn != default);

                // FIN-03: Leaves calculation - separate Paid Leaves from explicit Loss of Pay (LOP / LWP) and handle Half-Days
                decimal paidLeaveDays = 0;
                decimal explicitLopDays = 0;

                foreach (var leave in allLeaves.Where(l => l.EmployeeId == emp.Id))
                {
                    var leaveStart = leave.StartDate < startDate ? startDate : leave.StartDate;
                    var leaveEnd = leave.EndDate > endDate ? endDate : leave.EndDate;
                    if (leaveStart <= leaveEnd)
                    {
                        bool isLopType = false;
                        if (leaveTypesMap.TryGetValue(leave.LeaveTypeId, out var ltName))
                        {
                            var lowerName = ltName.ToLowerInvariant();
                            isLopType = lowerName.Contains("loss of pay") || lowerName.Contains("lwp") ||
                                        lowerName.Contains("unpaid") || lowerName.Contains("lop");
                        }

                        decimal dayWeight = (leave.LeaveMode?.Equals("HalfDay", StringComparison.OrdinalIgnoreCase) == true ||
                                             leave.LeaveMode?.Equals("Half Day", StringComparison.OrdinalIgnoreCase) == true)
                                            ? 0.5m : 1.0m;

                        for (var ld = leaveStart; ld <= leaveEnd; ld = ld.AddDays(1))
                        {
                            var ldOnly = DateOnly.FromDateTime(ld);
                            if (ld.DayOfWeek != DayOfWeek.Saturday && ld.DayOfWeek != DayOfWeek.Sunday && !holidaysInMonth.Contains(ldOnly))
                            {
                                if (isLopType)
                                {
                                    explicitLopDays += dayWeight;
                                }
                                else
                                {
                                    paidLeaveDays += dayWeight;
                                }
                            }
                        }
                    }
                }

                // Proration for employees who joined after cycle start
                DateTime effectiveCycleStart = startDate;
                if (emp.DateOfJoining.HasValue && emp.DateOfJoining.Value > startDate)
                {
                    effectiveCycleStart = emp.DateOfJoining.Value.Date;
                }

                int employeeExpectedDays = 0;
                for (var d = effectiveCycleStart; d <= endDate; d = d.AddDays(1))
                {
                    var dOnly = DateOnly.FromDateTime(d);
                    if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday && !holidaysInMonth.Contains(dOnly))
                    {
                        employeeExpectedDays++;
                    }
                }

                if (employeeExpectedDays <= 0)
                {
                    continue;
                }

                // Account for worked days and paid leaves against expected working days
                decimal accountedDays = presentDays + paidLeaveDays;
                decimal unexcusedAbsenceDays = 0;
                // FIN-02: Zero-Attendance Anomaly Detection
                if (!empAttendance.Any() && paidLeaveDays == 0 && explicitLopDays == 0)
                {
                    unexcusedAbsenceDays = employeeExpectedDays;
                    presentDays = 0;
                    Log.Warning("Zero attendance recorded for EmployeeId: {EmployeeId} ({Name}) for {Month}/{Year}. Marked as unexcused absence.", emp.Id, $"{emp.FirstName} {emp.LastName}", req.Month, req.Year);
                }
                else if (accountedDays < employeeExpectedDays)
                {
                    unexcusedAbsenceDays = employeeExpectedDays - accountedDays;
                }
                decimal lopDays = Math.Min(employeeExpectedDays, explicitLopDays + unexcusedAbsenceDays);

                decimal perDayPay = standardWorkingDays > 0 ? (monthlyGross / standardWorkingDays) : 0m;
                decimal lopDeduction = Math.Round(perDayPay * lopDays, 2);

                // Reimbursements for this employee
                var empReimbursements = pendingReimbursements.Where(r => r.EmployeeId == emp.Id).ToList();
                decimal totalApprovedReimbursement = empReimbursements.Sum(r => r.ApprovedAmount > 0 ? r.ApprovedAmount : r.Amount);
                foreach (var r in empReimbursements)
                {
                    r.PayRunId = payRun.Id;
                    _unitOfWork.ReimbursementClaims.Update(r);
                }

                // Loan EMI deductions
                var empLoans = activeLoans.Where(l => l.EmployeeId == emp.Id).ToList();
                decimal loanEmi = empLoans.Sum(l => Math.Min(l.MonthlyEmiAmount, l.RemainingBalance));

                decimal actualGross = Math.Max(0, monthlyGross - lopDeduction + totalApprovedReimbursement);
                decimal actualDeductions = epf + esic + pt + tds + loanEmi;
                decimal actualNet = Math.Max(0, actualGross - actualDeductions);

                var bank = bankInfos.FirstOrDefault(b => b.EmployeeId == emp.Id);

                // Automated Exception Detection
                var exceptions = new List<string>();
                if (bank == null || string.IsNullOrWhiteSpace(bank.AccountNumber) || string.IsNullOrWhiteSpace(bank.IFSCCode))
                {
                    exceptions.Add("Missing bank account coordinates");
                }
                if (actualNet <= 0 || actualDeductions >= actualGross)
                {
                    exceptions.Add("Negative or zero net take-home pay");
                }
                if (unexcusedAbsenceDays > 0 && (!empAttendance.Any() || unexcusedAbsenceDays >= 3))
                {
                    exceptions.Add($"High unexcused absence gap ({unexcusedAbsenceDays} days)");
                }
                if (emp.DateOfJoining.HasValue && emp.DateOfJoining.Value.Date > startDate.Date)
                {
                    exceptions.Add($"New joiner proration (Joined {emp.DateOfJoining.Value:dd MMM yyyy})");
                }
                if (string.Equals(emp.EmploymentStatus, "Exited", StringComparison.OrdinalIgnoreCase))
                {
                    exceptions.Add("Exited employee (Subject to FnF settlement)");
                }
                var priorEmpDetail = priorDetails.FirstOrDefault(p => p.EmployeeId == emp.Id);
                if (priorEmpDetail != null && priorEmpDetail.NetPay > 0)
                {
                    decimal diff = Math.Abs(actualNet - priorEmpDetail.NetPay);
                    if (diff / priorEmpDetail.NetPay > 0.20m)
                    {
                        exceptions.Add($">20% variance vs prior month (Prior: ₹{priorEmpDetail.NetPay:N0}, Current: ₹{actualNet:N0})");
                    }
                }

                var detail = new PayRunEmployeeDetail
                {
                    PayRunId = payRun.Id,
                    EmployeeId = emp.Id,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    EmployeeCode = emp.EmployeeCode.HasValue ? emp.EmployeeCode.Value.ToString() : $"EMP{emp.Id:D4}",
                    TotalWorkingDays = employeeExpectedDays,
                    PresentDays = presentDays,
                    PaidLeaveDays = paidLeaveDays,
                    LopDays = lopDays,
                    LopDeduction = lopDeduction,
                    BasicPay = basic,
                    Hra = hra,
                    SpecialAllowance = specialAllowance,
                    Reimbursements = totalApprovedReimbursement,
                    GrossPay = actualGross,
                    EpfDeduction = epf,
                    EsicDeduction = esic,
                    PtDeduction = pt,
                    TdsDeduction = tds,
                    LoanEmiDeduction = loanEmi,
                    OtherDeductions = 0,
                    TotalDeductions = actualDeductions,
                    NetPay = actualNet,
                    PaymentStatus = "Pending",
                    IsOnHold = false,
                    ManualAdjustmentAmount = 0,
                    ArrearsAmount = 0,
                    BonusAmount = 0,
                    HasExceptionFlag = exceptions.Count > 0,
                    ExceptionDetails = exceptions.Count > 0 ? string.Join("; ", exceptions) : null,
                    BankName = bank?.BankName ?? "Not Configured",
                    AccountNumber = bank?.AccountNumber,
                    IfscCode = bank?.IFSCCode ?? "",
                    CreatedBy = (int)currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.PayRunEmployeeDetails.Add(detail);

                totalGross += actualGross;
                totalDeductions += actualDeductions;
                totalNet += actualNet;
                employeeCount++;
            }

            payRun.TotalEmployees = employeeCount;
            payRun.TotalGrossPay = totalGross;
            payRun.TotalDeductions = totalDeductions;
            payRun.TotalNetPay = totalNet;

            _unitOfWork.PayRuns.Update(payRun);
            _unitOfWork.Save();

            return payRun;
        }

        public async Task<PayRun> UpdatePayRunStatusAsync(UpdatePayRunStatusRequest req)
        {
            var payRun = await _unitOfWork.PayRuns.GetById(req.PayRunId);
            if (payRun == null) throw new KeyNotFoundException($"PayRun {req.PayRunId} not found");

            // SEC-03: Pay Run State Machine Transition Validation
            if (payRun.Status.Equals("Disbursed", StringComparison.OrdinalIgnoreCase) || payRun.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot modify or change the status of an already disbursed/paid pay run. Disbursed pay runs are final and immutable.");
            }

            var currentStatus = (payRun.Status ?? "Draft").Trim();
            var targetStatus = (req.Status ?? "").Trim();

            bool isValidTransition = false;
            if (string.Equals(currentStatus, targetStatus, StringComparison.OrdinalIgnoreCase))
            {
                isValidTransition = true;
            }
            else if (string.Equals(currentStatus, "Draft", StringComparison.OrdinalIgnoreCase))
            {
                isValidTransition = string.Equals(targetStatus, "Calculated", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Submitted", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "In Review", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);
            }
            else if (string.Equals(currentStatus, "Calculated", StringComparison.OrdinalIgnoreCase))
            {
                isValidTransition = string.Equals(targetStatus, "In Review", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Approved", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Draft", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);
            }
            else if (string.Equals(currentStatus, "Submitted", StringComparison.OrdinalIgnoreCase) || string.Equals(currentStatus, "In Review", StringComparison.OrdinalIgnoreCase))
            {
                isValidTransition = string.Equals(targetStatus, "Approved", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "In Review", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Calculated", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Draft", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);
            }
            else if (string.Equals(currentStatus, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                isValidTransition = string.Equals(targetStatus, "Locked", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Disbursed", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Paid", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "In Review", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Draft", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);
            }
            else if (string.Equals(currentStatus, "Locked", StringComparison.OrdinalIgnoreCase))
            {
                isValidTransition = string.Equals(targetStatus, "Disbursed", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(targetStatus, "Paid", StringComparison.OrdinalIgnoreCase);
            }
            else if (string.Equals(currentStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                isValidTransition = string.Equals(targetStatus, "Draft", StringComparison.OrdinalIgnoreCase);
            }

            if (!isValidTransition)
            {
                throw new InvalidOperationException($"Invalid pay run status transition from '{currentStatus}' to '{targetStatus}'. Sequential pay run lifecycle: Draft -> Calculated -> In Review -> Approved -> Locked -> Paid.");
            }

            long currentUserId = _userContext.GetCurrentEmployeeId();
            payRun.Status = req.Status;
            payRun.Notes = string.IsNullOrEmpty(req.Notes) ? payRun.Notes : req.Notes;
            payRun.UpdatedBy = (int)currentUserId;
            payRun.UpdatedDate = DateTime.UtcNow;

            if (req.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                payRun.ApprovedByEmployeeId = (int)currentUserId;
                payRun.ApprovedDate = DateTime.UtcNow;
            }
            else if (req.Status.Equals("Locked", StringComparison.OrdinalIgnoreCase))
            {
                payRun.IsLocked = true;
                payRun.LockedDate = DateTime.UtcNow;
                payRun.LockedByEmployeeId = currentUserId;
            }
            else if (req.Status.Equals("Disbursed", StringComparison.OrdinalIgnoreCase) || req.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                payRun.DisbursedDate = DateTime.UtcNow;

                // Decrement remaining balance on active employee loans
                var activeLoans = (await _unitOfWork.EmployeeLoans.GetAll())
                    .Where(l => l.Status == "Active" && l.RemainingBalance > 0 && !l.IsDeleted).ToList();

                var payRunDetails = (await _unitOfWork.PayRunEmployeeDetails.GetAll())
                    .Where(d => d.PayRunId == payRun.Id && !d.IsDeleted).ToList();

                foreach (var d in payRunDetails.Where(d => d.LoanEmiDeduction > 0))
                {
                    decimal remEmi = d.LoanEmiDeduction;
                    foreach (var loan in activeLoans.Where(l => l.EmployeeId == d.EmployeeId).OrderBy(l => l.CreatedDate))
                    {
                        if (remEmi <= 0) break;
                        decimal deductLoan = Math.Min(remEmi, loan.RemainingBalance);
                        loan.RemainingBalance -= deductLoan;
                        remEmi -= deductLoan;
                        if (loan.RemainingBalance <= 0)
                        {
                            loan.Status = "Closed";
                        }
                        loan.UpdatedDate = DateTime.UtcNow;
                        _unitOfWork.EmployeeLoans.Update(loan);
                    }
                }

                // Update employee detail payment statuses
                var details = (await _unitOfWork.PayRunEmployeeDetails.GetAll())
                    .Where(d => d.PayRunId == payRun.Id && !d.IsDeleted).ToList();

                var allExistingPayslips = (await _unitOfWork.Payslips.GetAll())
                    .Where(p => p.PayRunId == payRun.Id || (p.Month == payRun.Month && p.Year == payRun.Year)).ToList();

                var monthName = new DateTime(payRun.Year, payRun.Month, 1).ToString("MMMM yyyy");

                foreach (var d in details)
                {
                    d.PaymentStatus = "Processed";
                    _unitOfWork.PayRunEmployeeDetails.Update(d);

                    // Check if payslip already exists for this employee for this month/year
                    var existingPayslip = allExistingPayslips.FirstOrDefault(p => p.EmployeeId == d.EmployeeId && !p.IsDeleted);

                    var breakdownJson = JsonSerializer.Serialize(new
                    {
                        BasicPay = d.BasicPay,
                        Hra = d.Hra,
                        SpecialAllowance = d.SpecialAllowance,
                        Reimbursements = d.Reimbursements,
                        EpfDeduction = d.EpfDeduction,
                        EsicDeduction = d.EsicDeduction,
                        PtDeduction = d.PtDeduction,
                        TdsDeduction = d.TdsDeduction,
                        LopDeduction = d.LopDeduction
                    });

                    if (existingPayslip != null)
                    {
                        existingPayslip.PayRunId = payRun.Id;
                        existingPayslip.GrossSalary = d.GrossPay;
                        existingPayslip.TotalDeductions = d.TotalDeductions;
                        existingPayslip.NetSalary = d.NetPay;
                        existingPayslip.LopDays = d.LopDays;
                        existingPayslip.WorkingDays = d.TotalWorkingDays;
                        existingPayslip.PresentDays = d.PresentDays;
                        existingPayslip.BreakdownJson = breakdownJson;
                        existingPayslip.UpdatedBy = (int)currentUserId;
                        existingPayslip.UpdatedDate = DateTime.UtcNow;
                        _unitOfWork.Payslips.Update(existingPayslip);
                    }
                    else
                    {
                        var payslip = new Payslip
                        {
                            PayRunId = payRun.Id,
                            EmployeeId = d.EmployeeId,
                            Month = payRun.Month,
                            Year = payRun.Year,
                            PayslipNumber = $"PAY-{payRun.Year}{payRun.Month:D2}-{d.EmployeeId:D4}",
                            GrossSalary = d.GrossPay,
                            TotalDeductions = d.TotalDeductions,
                            NetSalary = d.NetPay,
                            LopDays = d.LopDays,
                            WorkingDays = d.TotalWorkingDays,
                            PresentDays = d.PresentDays,
                            BreakdownJson = breakdownJson,
                            GeneratedDate = DateTime.UtcNow,
                            CreatedBy = (int)currentUserId,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            IsActive = true
                        };
                        await _unitOfWork.Payslips.Add(payslip);
                    }

                    // Sync with Employee.Responsibilities JSON so legacy and matrix views also immediately reflect "Credited"
                    try
                    {
                        var empEntity = await _unitOfWork.Employees.GetById(d.EmployeeId);
                        if (empEntity != null)
                        {
                            var respDict = new Dictionary<string, object>();
                            if (!string.IsNullOrEmpty(empEntity.Responsibilities) && empEntity.Responsibilities.Trim().StartsWith("{"))
                            {
                                try
                                {
                                    respDict = JsonSerializer.Deserialize<Dictionary<string, object>>(empEntity.Responsibilities) ?? new();
                                }
                                catch { }
                            }

                            var disbList = new List<Dictionary<string, object>>();
                            if (respDict.TryGetValue("disbursements", out var dObj) && dObj is JsonElement jElem && jElem.ValueKind == JsonValueKind.Array)
                            {
                                try
                                {
                                    disbList = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jElem.GetRawText()) ?? new();
                                }
                                catch { }
                            }

                            // Remove existing entry for this month
                            disbList.RemoveAll(x => x.TryGetValue("monthYear", out var my) && my?.ToString()?.Equals(monthName, StringComparison.OrdinalIgnoreCase) == true);

                            var newDisb = new Dictionary<string, object>
                            {
                                ["id"] = $"PR-{payRun.Id}-{d.EmployeeId}",
                                ["monthYear"] = monthName,
                                ["month"] = monthName,
                                ["creditDate"] = payRun.DisbursedDate?.ToString("o") ?? DateTime.UtcNow.ToString("o"),
                                ["paymentDate"] = payRun.DisbursedDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                                ["paymentMode"] = "Direct Deposit / Bank Transfer",
                                ["transactionRef"] = $"PR-{payRun.Id}-EMP{d.EmployeeId:D4}",
                                ["grossSalary"] = d.GrossPay,
                                ["basicSalary"] = d.BasicPay,
                                ["hra"] = d.Hra,
                                ["specialAllowance"] = d.SpecialAllowance,
                                ["pfDeduction"] = d.EpfDeduction,
                                ["additionalDeductions"] = d.TotalDeductions - d.EpfDeduction,
                                ["netSalaryCredited"] = d.NetPay,
                                ["netSalary"] = d.NetPay,
                                ["status"] = "Credited",
                                ["markedBy"] = "Automated Pay Run Engine",
                                ["markedAt"] = DateTime.UtcNow.ToString("o"),
                                ["bankName"] = d.BankName ?? "Direct Deposit",
                                ["accountNumber"] = d.AccountNumber ?? "",
                                ["ifscCode"] = d.IfscCode ?? ""
                            };
                            disbList.Add(newDisb);
                            respDict["disbursements"] = disbList;
                            respDict["lastDisbursedMonth"] = monthName;
                            respDict["lastDisbursedDate"] = newDisb["creditDate"];
                            empEntity.Responsibilities = JsonSerializer.Serialize(respDict);
                            _unitOfWork.Employees.Update(empEntity);
                        }
                    }
                    catch { }
                }

                // Update attached Reimbursements to "Paid"
                var reimbursements = (await _unitOfWork.ReimbursementClaims.GetAll())
                    .Where(r => r.PayRunId == payRun.Id && !r.IsDeleted).ToList();
                foreach (var r in reimbursements)
                {
                    r.Status = "Paid";
                    _unitOfWork.ReimbursementClaims.Update(r);
                }
            }

            _unitOfWork.PayRuns.Update(payRun);
            _unitOfWork.Save();
            return payRun;
        }

        public async Task<bool> DeletePayRunAsync(long payRunId)
        {
            var payRun = await _unitOfWork.PayRuns.GetById(payRunId);
            if (payRun == null) return false;
            if (payRun.IsLocked || payRun.Status.Equals("Locked", StringComparison.OrdinalIgnoreCase) ||
                payRun.Status.Equals("Disbursed", StringComparison.OrdinalIgnoreCase) ||
                payRun.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot discard or delete a pay run that has already been Locked, Disbursed, or Paid.");
            }

            payRun.IsDeleted = true;
            payRun.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.PayRuns.Update(payRun);

            var details = (await _unitOfWork.PayRunEmployeeDetails.GetAll())
                .Where(d => d.PayRunId == payRunId).ToList();
            foreach (var d in details)
            {
                d.IsDeleted = true;
                d.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.PayRunEmployeeDetails.Update(d);
            }

            var reimbursements = (await _unitOfWork.ReimbursementClaims.GetAll())
                .Where(r => r.PayRunId == payRunId).ToList();
            foreach (var r in reimbursements)
            {
                r.PayRunId = null;
                if (r.Status == "Paid") r.Status = "Approved";
                _unitOfWork.ReimbursementClaims.Update(r);
            }

            _unitOfWork.Save();
            return true;
        }

        public async Task<bool> LockPayRunAsync(long payRunId, long employeeId)
        {
            var payRun = await _unitOfWork.PayRuns.GetById(payRunId);
            if (payRun == null) throw new KeyNotFoundException($"PayRun {payRunId} not found");

            if (payRun.Status.Equals("Disbursed", StringComparison.OrdinalIgnoreCase) || payRun.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot lock an already disbursed or paid pay run.");
            }

            payRun.IsLocked = true;
            payRun.LockedDate = DateTime.UtcNow;
            payRun.LockedByEmployeeId = employeeId;
            payRun.Status = "Locked";
            payRun.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.PayRuns.Update(payRun);
            _unitOfWork.Save();
            return true;
        }

        public async Task<bool> RecalculatePayRunAsync(long payRunId)
        {
            var payRun = await _unitOfWork.PayRuns.GetById(payRunId);
            if (payRun == null) throw new KeyNotFoundException($"PayRun {payRunId} not found");

            if (payRun.IsLocked || payRun.Status.Equals("Locked", StringComparison.OrdinalIgnoreCase) ||
                payRun.Status.Equals("Disbursed", StringComparison.OrdinalIgnoreCase) ||
                payRun.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot recalculate a locked or disbursed pay run.");
            }

            var details = (await _unitOfWork.PayRunEmployeeDetails.GetAll())
                .Where(d => d.PayRunId == payRunId && !d.IsDeleted).ToList();

            decimal totalGross = 0;
            decimal totalDeductions = 0;
            decimal totalNet = 0;

            foreach (var d in details)
            {
                decimal baseGross = d.BasicPay + d.Hra + d.SpecialAllowance + d.Reimbursements - d.LopDeduction;
                d.GrossPay = Math.Max(0, baseGross + d.ArrearsAmount + d.BonusAmount);

                decimal manualDeduction = d.ManualAdjustmentAmount < 0 ? Math.Abs(d.ManualAdjustmentAmount) : 0m;
                decimal manualAddition = d.ManualAdjustmentAmount > 0 ? d.ManualAdjustmentAmount : 0m;

                d.TotalDeductions = d.EpfDeduction + d.EsicDeduction + d.PtDeduction + d.TdsDeduction + d.LoanEmiDeduction + d.OtherDeductions + manualDeduction;

                if (d.IsOnHold)
                {
                    d.NetPay = 0;
                }
                else
                {
                    d.NetPay = Math.Max(0, d.GrossPay - d.TotalDeductions + manualAddition);
                }

                d.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.PayRunEmployeeDetails.Update(d);

                totalGross += d.GrossPay;
                totalDeductions += d.TotalDeductions;
                totalNet += d.NetPay;
            }

            payRun.TotalGrossPay = totalGross;
            payRun.TotalDeductions = totalDeductions;
            payRun.TotalNetPay = totalNet;
            payRun.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.PayRuns.Update(payRun);
            _unitOfWork.Save();
            return true;
        }

        public async Task<bool> ToggleHoldEmployeeDetailAsync(long detailId, bool isHold, string? reason)
        {
            var detail = await _unitOfWork.PayRunEmployeeDetails.GetById(detailId);
            if (detail == null) throw new KeyNotFoundException($"PayRunEmployeeDetail {detailId} not found");

            var payRun = await _unitOfWork.PayRuns.GetById(detail.PayRunId);
            if (payRun != null && (payRun.IsLocked || payRun.Status.Equals("Locked", StringComparison.OrdinalIgnoreCase) ||
                                  payRun.Status.Equals("Disbursed", StringComparison.OrdinalIgnoreCase) ||
                                  payRun.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Cannot modify holds on a locked or disbursed pay run.");
            }

            detail.IsOnHold = isHold;
            detail.HoldReason = isHold ? reason : null;
            detail.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.PayRunEmployeeDetails.Update(detail);
            _unitOfWork.Save();

            await RecalculatePayRunAsync(detail.PayRunId);
            return true;
        }

        public async Task<bool> UpdateEmployeeAdjustmentAsync(long detailId, decimal manualAdjustment, decimal arrears, decimal bonus, string? reason)
        {
            var detail = await _unitOfWork.PayRunEmployeeDetails.GetById(detailId);
            if (detail == null) throw new KeyNotFoundException($"PayRunEmployeeDetail {detailId} not found");

            var payRun = await _unitOfWork.PayRuns.GetById(detail.PayRunId);
            if (payRun != null && (payRun.IsLocked || payRun.Status.Equals("Locked", StringComparison.OrdinalIgnoreCase) ||
                                  payRun.Status.Equals("Disbursed", StringComparison.OrdinalIgnoreCase) ||
                                  payRun.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Cannot modify adjustments on a locked or disbursed pay run.");
            }

            detail.ManualAdjustmentAmount = manualAdjustment;
            detail.ArrearsAmount = arrears;
            detail.BonusAmount = bonus;
            detail.AdjustmentReason = reason;
            detail.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.PayRunEmployeeDetails.Update(detail);
            _unitOfWork.Save();

            await RecalculatePayRunAsync(detail.PayRunId);
            return true;
        }

        public async Task<IEnumerable<EmployeeLoan>> GetAllLoansAsync(long? employeeId = null)
        {
            var loans = (await _unitOfWork.EmployeeLoans.GetAll())
                .Where(l => !l.IsDeleted);

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                loans = loans.Where(l => l.EmployeeId == employeeId.Value);
            }

            var emps = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);
            var list = loans.OrderByDescending(l => l.CreatedDate).ToList();
            foreach (var item in list)
            {
                if (emps.TryGetValue(item.EmployeeId, out var emp))
                {
                    item.Employee = emp;
                }
            }

            return list;
        }

        public async Task<EmployeeLoan> ApplyLoanAsync(ApplyEmployeeLoanRequest req, long createdBy)
        {
            if (req.PrincipalAmount <= 0)
            {
                throw new ArgumentException("Loan principal amount must be greater than 0.");
            }
            if (req.TenureMonths <= 0)
            {
                throw new ArgumentException("Loan tenure must be at least 1 month.");
            }

            decimal interestAmount = req.PrincipalAmount * (req.InterestRatePercent / 100m);
            decimal totalRepayable = req.PrincipalAmount + interestAmount;
            decimal monthlyEmi = Math.Round(totalRepayable / req.TenureMonths, 2);

            var loan = new EmployeeLoan
            {
                EmployeeId = req.EmployeeId,
                LoanType = req.LoanType ?? "Personal Loan",
                PrincipalAmount = req.PrincipalAmount,
                InterestRatePercent = req.InterestRatePercent,
                TotalRepayableAmount = totalRepayable,
                MonthlyEmiAmount = monthlyEmi,
                TenureMonths = req.TenureMonths,
                DisbursementDate = DateTime.UtcNow,
                RemainingBalance = totalRepayable,
                Status = "Active",
                Reason = req.Reason,
                CreatedBy = (int)createdBy,
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.EmployeeLoans.Add(loan);
            _unitOfWork.Save();
            return loan;
        }

        public async Task<EmployeeLoan> UpdateLoanStatusAsync(long loanId, string status, string? reason)
        {
            var loan = await _unitOfWork.EmployeeLoans.GetById(loanId);
            if (loan == null) throw new KeyNotFoundException($"Loan {loanId} not found");

            loan.Status = status;
            loan.Reason = !string.IsNullOrWhiteSpace(reason) ? reason : loan.Reason;
            loan.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.EmployeeLoans.Update(loan);
            _unitOfWork.Save();
            return loan;
        }

        #endregion

        #region Payslips & Self-Service

        public async Task<PayslipDto> RecordIndividualSalaryCreditAsync(IndividualSalaryCreditRequest req)
        {
            if (req.EmployeeId <= 0) throw new ArgumentException("Valid EmployeeId is required");
            if (req.Month < 1 || req.Month > 12) throw new ArgumentException("Month must be between 1 and 12");
            if (req.Year < 2000 || req.Year > 2100) throw new ArgumentException("Valid Year is required");

            long currentUserId = _userContext.GetCurrentEmployeeId();
            var employee = await _unitOfWork.Employees.GetById((int)req.EmployeeId);
            if (employee == null) throw new KeyNotFoundException($"Employee {req.EmployeeId} not found");

            var bank = (await _unitOfWork.EmployeePaymentInfo.GetAll())
                .FirstOrDefault(b => b.EmployeeId == req.EmployeeId && !b.IsDeleted);

            var allSlips = await _unitOfWork.Payslips.GetAll();
            var existingSlip = allSlips.FirstOrDefault(p => p.EmployeeId == req.EmployeeId && p.Month == req.Month && p.Year == req.Year && !p.IsDeleted);

            var breakdownJson = JsonSerializer.Serialize(new
            {
                BasicPay = req.BasicSalary,
                Hra = req.Hra,
                SpecialAllowance = req.SpecialAllowance,
                Bonus = req.BonusAmount,
                EpfDeduction = req.EpfDeduction,
                AdditionalDeductions = req.AdditionalDeductions,
                DeductionReason = req.DeductionReason,
                PaymentMode = req.PaymentMode,
                TransactionRef = req.TransactionRef,
                PaymentDate = req.PaymentDate?.ToString("yyyy-MM-dd")
            });

            decimal totalDeductions = req.EpfDeduction + req.AdditionalDeductions;
            decimal grossSalary = req.GrossSalary > 0 ? req.GrossSalary : (req.BasicSalary + req.Hra + req.SpecialAllowance + req.BonusAmount);
            decimal netSalary = req.NetSalary > 0 ? req.NetSalary : (grossSalary - totalDeductions);

            Payslip payslip;
            if (existingSlip != null)
            {
                payslip = existingSlip;
                payslip.GrossSalary = grossSalary;
                payslip.TotalDeductions = totalDeductions;
                payslip.NetSalary = netSalary;
                payslip.BreakdownJson = breakdownJson;
                payslip.UpdatedBy = (int)currentUserId;
                payslip.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Payslips.Update(payslip);
            }
            else
            {
                payslip = new Payslip
                {
                    EmployeeId = req.EmployeeId,
                    Month = req.Month,
                    Year = req.Year,
                    PayslipNumber = $"PAY-{req.Year}{req.Month:D2}-{req.EmployeeId:D4}",
                    GrossSalary = grossSalary,
                    TotalDeductions = totalDeductions,
                    NetSalary = netSalary,
                    WorkingDays = DateTime.DaysInMonth(req.Year, req.Month),
                    PresentDays = DateTime.DaysInMonth(req.Year, req.Month),
                    LopDays = 0,
                    BreakdownJson = breakdownJson,
                    GeneratedDate = req.PaymentDate ?? DateTime.UtcNow,
                    CreatedBy = (int)currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true
                };
                await _unitOfWork.Payslips.Add(payslip);
            }

            // Sync with PayRunEmployeeDetail if exists
            var matchingPayRun = (await _unitOfWork.PayRuns.GetAll())
                .FirstOrDefault(p => p.Month == req.Month && p.Year == req.Year && !p.IsDeleted);
            if (matchingPayRun != null)
            {
                var details = await _unitOfWork.PayRunEmployeeDetails.GetAll();
                var detail = details.FirstOrDefault(d => d.EmployeeId == req.EmployeeId && d.PayRunId == matchingPayRun.Id && !d.IsDeleted);
                if (detail != null)
                {
                    detail.PaymentStatus = "Processed";
                    detail.GrossPay = grossSalary;
                    detail.NetPay = netSalary;
                    detail.UpdatedBy = (int)currentUserId;
                    detail.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.PayRunEmployeeDetails.Update(detail);
                }
            }

            // Sync with Employee.Responsibilities JSON for backward compatibility
            try
            {
                var monthName = new DateTime(req.Year, req.Month, 1).ToString("MMMM yyyy");
                var respStr = employee.Responsibilities;
                Dictionary<string, object>? respDict = null;
                if (!string.IsNullOrWhiteSpace(respStr) && respStr.TrimStart().StartsWith("{"))
                {
                    respDict = JsonSerializer.Deserialize<Dictionary<string, object>>(respStr);
                }
                respDict ??= new Dictionary<string, object>();

                var disbList = new List<Dictionary<string, object>>();
                if (respDict.TryGetValue("disbursements", out var dObj) && dObj is JsonElement je && je.ValueKind == JsonValueKind.Array)
                {
                    foreach (var elem in je.EnumerateArray())
                    {
                        var dDict = JsonSerializer.Deserialize<Dictionary<string, object>>(elem.GetRawText());
                        if (dDict != null)
                        {
                            var m = dDict.GetValueOrDefault("month")?.ToString() ?? dDict.GetValueOrDefault("monthYear")?.ToString() ?? "";
                            if (!string.Equals(m, monthName, StringComparison.OrdinalIgnoreCase))
                            {
                                disbList.Add(dDict);
                            }
                        }
                    }
                }

                var paymentDateIso = req.PaymentDate?.ToString("o") ?? DateTime.UtcNow.ToString("o");
                var paymentDateYmd = req.PaymentDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

                var newDisb = new Dictionary<string, object>
                {
                    ["id"] = $"INDIV-{req.Year}{req.Month:D2}-{req.EmployeeId}",
                    ["monthYear"] = monthName,
                    ["month"] = monthName,
                    ["creditDate"] = paymentDateIso,
                    ["paymentDate"] = paymentDateYmd,
                    ["paymentMode"] = req.PaymentMode ?? "Direct Bank Transfer (NEFT)",
                    ["transactionRef"] = req.TransactionRef ?? $"UTR-{req.Year}{req.Month:D2}-{req.EmployeeId}",
                    ["grossSalary"] = grossSalary,
                    ["basicSalary"] = req.BasicSalary,
                    ["hra"] = req.Hra,
                    ["specialAllowance"] = req.SpecialAllowance,
                    ["pfDeduction"] = req.EpfDeduction,
                    ["additionalDeductions"] = req.AdditionalDeductions,
                    ["deductionReason"] = req.DeductionReason ?? "",
                    ["bonusAmount"] = req.BonusAmount,
                    ["netSalaryCredited"] = netSalary,
                    ["netSalary"] = netSalary,
                    ["status"] = "Credited",
                    ["markedBy"] = _userContext.GetCurrentEmployeeId() > 0 ? "HR Operations" : "System",
                    ["markedAt"] = DateTime.UtcNow.ToString("o"),
                    ["bankName"] = bank?.BankName ?? "Direct Deposit",
                    ["accountNumber"] = bank?.AccountNumber ?? "",
                    ["ifscCode"] = bank?.IFSCCode ?? ""
                };
                disbList.Add(newDisb);
                respDict["disbursements"] = disbList;
                respDict["lastDisbursedMonth"] = monthName;
                respDict["lastDisbursedDate"] = paymentDateIso;
                employee.Responsibilities = JsonSerializer.Serialize(respDict);
                _unitOfWork.Employees.Update(employee);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to sync individual salary credit to employee.Responsibilities");
            }

            _unitOfWork.Save();

            return await GetPayslipByIdAsync(payslip.Id) ?? new PayslipDto
            {
                Id = payslip.Id,
                EmployeeId = req.EmployeeId,
                Month = req.Month,
                Year = req.Year,
                GrossSalary = grossSalary,
                NetSalary = netSalary,
                PayslipNumber = payslip.PayslipNumber
            };
        }

        public async Task<IEnumerable<PayslipDto>> GetAllPayslipsAsync()
        {
            var payslips = (await _unitOfWork.Payslips.GetAll())
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();

            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);
            var banks = (await _unitOfWork.EmployeePaymentInfo.GetAll())
                .Where(b => !b.IsDeleted)
                .GroupBy(b => b.EmployeeId)
                .ToDictionary(g => g.Key, g => g.First());

            var list = new List<PayslipDto>();
            foreach (var p in payslips)
            {
                employees.TryGetValue(p.EmployeeId, out var emp);
                banks.TryGetValue(p.EmployeeId, out var bank);
                var dto = BuildPayslipDto(p, emp, bank);
                list.Add(dto);
            }
            return list;
        }

        public async Task<IEnumerable<PayslipDto>> GetEmployeePayslipsAsync(long employeeId)
        {
            var payslips = (await _unitOfWork.Payslips.GetAll())
                .Where(p => p.EmployeeId == employeeId && !p.IsDeleted)
                .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).ToList();

            var emp = await _unitOfWork.Employees.GetById(employeeId);
            if (emp?.DateOfJoining.HasValue == true)
            {
                var doj = emp.DateOfJoining.Value.Date;
                payslips = payslips.Where(p =>
                    p.Year > doj.Year || (p.Year == doj.Year && p.Month >= doj.Month)
                ).ToList();
            }

            var bank = (await _unitOfWork.EmployeePaymentInfo.GetAll()).FirstOrDefault(b => b.EmployeeId == employeeId);

            var list = new List<PayslipDto>();
            foreach (var p in payslips)
            {
                var dto = BuildPayslipDto(p, emp, bank);
                list.Add(dto);
            }
            return list;
        }

        public async Task<PayslipDto?> GetPayslipByIdAsync(long id)
        {
            var p = await _unitOfWork.Payslips.GetById(id);
            if (p == null) return null;

            var emp = await _unitOfWork.Employees.GetById(p.EmployeeId);
            var bank = (await _unitOfWork.EmployeePaymentInfo.GetAll()).FirstOrDefault(b => b.EmployeeId == p.EmployeeId);
            return BuildPayslipDto(p, emp, bank);
        }

        private static PayslipDto BuildPayslipDto(Payslip p, Employee? emp, EmployeePaymentInfo? bank)
        {
            decimal basic = 0, hra = 0, sa = 0, reimb = 0, epf = 0, esic = 0, pt = 0, tds = 0, lop = 0;
            if (!string.IsNullOrEmpty(p.BreakdownJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(p.BreakdownJson);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("BasicPay", out var prop)) basic = prop.GetDecimal();
                    if (root.TryGetProperty("Hra", out prop)) hra = prop.GetDecimal();
                    if (root.TryGetProperty("SpecialAllowance", out prop)) sa = prop.GetDecimal();
                    if (root.TryGetProperty("Reimbursements", out prop)) reimb = prop.GetDecimal();
                    if (root.TryGetProperty("EpfDeduction", out prop)) epf = prop.GetDecimal();
                    if (root.TryGetProperty("EsicDeduction", out prop)) esic = prop.GetDecimal();
                    if (root.TryGetProperty("PtDeduction", out prop)) pt = prop.GetDecimal();
                    if (root.TryGetProperty("TdsDeduction", out prop)) tds = prop.GetDecimal();
                    if (root.TryGetProperty("LopDeduction", out prop)) lop = prop.GetDecimal();
                }
                catch { }
            }

            return new PayslipDto
            {
                Id = p.Id,
                PayRunId = p.PayRunId ?? 0,
                EmployeeId = p.EmployeeId,
                EmployeeName = $"{emp?.FirstName} {emp?.LastName}".Trim(),
                EmployeeCode = emp?.EmployeeCode.HasValue == true ? emp.EmployeeCode.Value.ToString() : $"EMP{p.EmployeeId:D4}",
                Designation = emp?.Designation ?? "Staff",
                Department = emp?.Branch ?? "General",
                BankName = bank?.BankName ?? "",
                AccountNumber = bank?.AccountNumber ?? "",
                IfscCode = bank?.IFSCCode ?? "",
                Pan = emp?.PassportNumber ?? "N/A",
                Month = p.Month,
                Year = p.Year,
                PayslipNumber = p.PayslipNumber,
                TotalWorkingDays = p.WorkingDays,
                PresentDays = p.PresentDays,
                LopDays = p.LopDays,
                BasicSalary = basic,
                Hra = hra,
                SpecialAllowance = sa,
                Reimbursements = reimb,
                GrossSalary = p.GrossSalary,
                EpfDeduction = epf,
                EsicDeduction = esic,
                PtDeduction = pt,
                TdsDeduction = tds,
                LopDeduction = lop,
                OtherDeductions = 0,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                GeneratedDate = p.GeneratedDate
            };
        }

        public async Task<string> GeneratePayslipHtmlAsync(long payslipId)
        {
            var dto = await GetPayslipByIdAsync(payslipId);
            if (dto == null) throw new KeyNotFoundException($"Payslip {payslipId} not found");

            string monthName = new DateTime(dto.Year, dto.Month, 1).ToString("MMMM yyyy");

            var sb = new StringBuilder();
            sb.Append($@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<title>Payslip - {monthName} - {dto.EmployeeName}</title>
<style>
  body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 30px; background: #f8fafc; color: #1e293b; }}
  .container {{ max-width: 800px; margin: auto; background: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.06); padding: 40px; border: 1px solid #e2e8f0; }}
  .header {{ display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #6366f1; padding-bottom: 20px; margin-bottom: 25px; }}
  .company-title {{ font-size: 24px; font-weight: 700; color: #4338ca; margin: 0; }}
  .doc-title {{ font-size: 18px; font-weight: 600; color: #64748b; margin: 4px 0 0 0; text-align: right; }}
  .grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 25px; font-size: 14px; }}
  .grid-box {{ background: #f8fafc; padding: 15px; border-radius: 8px; border: 1px solid #edf2f7; }}
  .grid-box p {{ margin: 6px 0; }}
  .grid-box strong {{ color: #334155; }}
  table {{ width: 100%; border-collapse: collapse; margin-bottom: 25px; font-size: 14px; }}
  th {{ background: #f1f5f9; padding: 12px; text-align: left; font-weight: 600; border-bottom: 2px solid #cbd5e1; }}
  td {{ padding: 10px 12px; border-bottom: 1px solid #f1f5f9; }}
  .text-right {{ text-align: right; }}
  .total-row td {{ font-weight: 700; background: #f8fafc; border-top: 2px solid #cbd5e1; }}
  .net-pay {{ background: linear-gradient(135deg, #4f46e5, #6366f1); color: white; padding: 20px; border-radius: 10px; text-align: center; margin-top: 20px; }}
  .net-amount {{ font-size: 28px; font-weight: 800; margin-top: 6px; }}
  .footer {{ margin-top: 30px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; padding-top: 15px; }}
  @media print {{ body {{ background: white; padding: 0; }} .container {{ box-shadow: none; border: none; padding: 0; }} }}
</style>
</head>
<body>
<div class='container'>
  <div class='header'>
    <div>
      <h1 class='company-title'>KHRMS Portal</h1>
      <p style='margin: 4px 0 0 0; color: #64748b; font-size: 13px;'>Monthly Salary Statement</p>
    </div>
    <div>
      <div class='doc-title'>Payslip: {monthName}</div>
      <p style='margin: 4px 0 0 0; color: #94a3b8; font-size: 12px; text-align: right;'>#{dto.PayslipNumber}</p>
    </div>
  </div>

  <div class='grid'>
    <div class='grid-box'>
      <p><strong>Employee:</strong> {dto.EmployeeName} ({dto.EmployeeCode})</p>
      <p><strong>Designation:</strong> {dto.Designation}</p>
      <p><strong>Department:</strong> {dto.Department}</p>
      <p><strong>PAN:</strong> {dto.Pan}</p>
    </div>
    <div class='grid-box'>
      <p><strong>Bank:</strong> {dto.BankName}</p>
      <p><strong>Account No:</strong> {dto.AccountNumber}</p>
      <p><strong>IFSC Code:</strong> {dto.IfscCode}</p>
      <p><strong>Days (Worked / LOP):</strong> {dto.PresentDays} / {dto.LopDays} (Total: {dto.TotalWorkingDays})</p>
    </div>
  </div>

  <table>
    <thead>
      <tr>
        <th>Earnings</th>
        <th class='text-right'>Amount (₹)</th>
        <th>Deductions</th>
        <th class='text-right'>Amount (₹)</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td>Basic Salary</td>
        <td class='text-right'>{dto.BasicSalary:N2}</td>
        <td>Employee PF (EPF)</td>
        <td class='text-right'>{dto.EpfDeduction:N2}</td>
      </tr>
      <tr>
        <td>House Rent Allowance (HRA)</td>
        <td class='text-right'>{dto.Hra:N2}</td>
        <td>Employee ESIC</td>
        <td class='text-right'>{dto.EsicDeduction:N2}</td>
      </tr>
      <tr>
        <td>Special Allowance</td>
        <td class='text-right'>{dto.SpecialAllowance:N2}</td>
        <td>Professional Tax (PT)</td>
        <td class='text-right'>{dto.PtDeduction:N2}</td>
      </tr>
      <tr>
        <td>Approved Reimbursements</td>
        <td class='text-right'>{dto.Reimbursements:N2}</td>
        <td>Income Tax (TDS)</td>
        <td class='text-right'>{dto.TdsDeduction:N2}</td>
      </tr>
      <tr>
        <td></td>
        <td></td>
        <td>Loss of Pay (LOP)</td>
        <td class='text-right'>{dto.LopDeduction:N2}</td>
      </tr>
      <tr class='total-row'>
        <td>Total Earnings (Gross)</td>
        <td class='text-right'>{dto.GrossSalary:N2}</td>
        <td>Total Deductions</td>
        <td class='text-right'>{dto.TotalDeductions:N2}</td>
      </tr>
    </tbody>
  </table>

  <div class='net-pay'>
    <div style='font-size: 14px; text-transform: uppercase; letter-spacing: 1px; opacity: 0.9;'>Net Take Home Pay</div>
    <div class='net-amount'>₹{dto.NetSalary:N2}</div>
  </div>

  <div class='footer'>
    This is a system generated document. No physical signature is required. Generated on {dto.GeneratedDate:dd MMM yyyy HH:mm} UTC.
  </div>
</div>
</body>
</html>");

            return sb.ToString();
        }

        #endregion

        #region Reimbursements & FBP

        public async Task<IEnumerable<ReimbursementClaim>> GetEmployeeReimbursementsAsync(long employeeId)
        {
            var claims = await _unitOfWork.ReimbursementClaims.GetAll();
            return claims.Where(c => c.EmployeeId == employeeId && !c.IsDeleted).OrderByDescending(c => c.BillDate).ToList();
        }

        public async Task<IEnumerable<ReimbursementClaim>> GetAllPendingReimbursementsAsync()
        {
            var claims = await _unitOfWork.ReimbursementClaims.GetAll();
            return claims.Where(c => !c.IsDeleted).OrderByDescending(c => c.CreatedDate).ToList();
        }

        public async Task<ReimbursementClaim> SubmitReimbursementClaimAsync(ReimbursementClaimRequest req)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            long empId = req.EmployeeId > 0 ? req.EmployeeId : currentUserId;

            var claim = new ReimbursementClaim
            {
                EmployeeId = empId,
                Category = req.Category,
                Amount = req.Amount,
                BillDate = req.BillDate,
                BillNumber = req.BillNumber,
                MerchantName = req.MerchantName,
                Description = req.Description,
                ReceiptUrl = req.ReceiptUrl,
                Status = "Pending",
                ApprovedAmount = 0,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.ReimbursementClaims.Add(claim);
            _unitOfWork.Save();
            return claim;
        }

        public async Task<ReimbursementClaim> ReviewReimbursementClaimAsync(ReviewReimbursementRequest req)
        {
            var claim = await _unitOfWork.ReimbursementClaims.GetById(req.ClaimId);
            if (claim == null) throw new KeyNotFoundException($"Claim {req.ClaimId} not found");

            long currentUserId = _userContext.GetCurrentEmployeeId();
            claim.Status = req.Status;
            claim.ApprovedAmount = req.Status == "Approved" ? (req.ApprovedAmount ?? claim.Amount) : 0;
            claim.ReviewedByEmployeeId = (int)currentUserId;
            claim.ReviewedDate = DateTime.UtcNow;
            claim.ReviewRemarks = req.Remarks;
            claim.UpdatedBy = (int)currentUserId;
            claim.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.ReimbursementClaims.Update(claim);
            _unitOfWork.Save();
            return claim;
        }

        #endregion

        #region Bank Payout Export (NEFT / RTGS)

        public async Task<byte[]> GenerateBankPayoutCsvAsync(long payRunId)
        {
            var details = (await _unitOfWork.PayRunEmployeeDetails.GetAll())
                .Where(d => d.PayRunId == payRunId && !d.IsDeleted).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Employee Code,Beneficiary Name,Bank Name,Account Number,IFSC Code,Amount,Payment Mode,Remarks");

            foreach (var d in details)
            {
                string empCode = EscapeCsv(d.EmployeeCode ?? "");
                string name = EscapeCsv(d.EmployeeName ?? "");
                string bank = EscapeCsv(d.BankName ?? "");
                string acc = EscapeCsv(d.AccountNumber ?? "");
                string ifsc = EscapeCsv(d.IfscCode ?? "");
                string amount = d.NetPay.ToString("F2");
                string mode = "NEFT";
                string remarks = EscapeCsv($"Salary Payout {d.NetPay:F2}");

                sb.AppendLine($"{empCode},{name},{bank},{acc},{ifsc},{amount},{mode},{remarks}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // SEC-19: Neutralize CSV formula injection by prefixing formula triggers with single apostrophe
            char firstChar = field[0];
            if (firstChar == '=' || firstChar == '+' || firstChar == '-' || firstChar == '@' || firstChar == '\t' || firstChar == '\r')
            {
                field = "'" + field;
            }

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        #endregion

        #region Full & Final (FnF) Settlement

        public async Task<FullAndFinalSettlement> CalculateFnFSettlementAsync(FullAndFinalCalculationRequest req)
        {
            var emp = await _unitOfWork.Employees.GetById(req.EmployeeId);
            if (emp == null) throw new KeyNotFoundException($"Employee {req.EmployeeId} not found");

            var salary = (await _unitOfWork.EmployeeSalaryAssignments.GetAll())
                .FirstOrDefault(s => s.EmployeeId == req.EmployeeId && !s.IsDeleted);

            decimal basic = salary?.BasicSalary ?? 15000m;
            decimal gross = salary?.MonthlyGross ?? 30000m;

            // Calculate tenure in years
            DateTime doj = emp.DateOfJoining ?? DateTime.UtcNow.AddYears(-1);
            decimal tenureYears = Math.Max(0, Math.Round((decimal)(req.RelievingDate - doj).TotalDays / 365.25m, 1));

            // Gratuity: Eligible if tenure >= 5 years. Formula: (15 * LastDrawnBasic * TenureYears) / 26
            bool gratuityEligible = tenureYears >= 4.8m; // standard threshold with rounding
            decimal gratuity = gratuityEligible ? Math.Round((15m * basic * tenureYears) / 26m, 2) : 0m;

            // Unutilized leave encashment (assume unutilized leaves, default 15 or query)
            decimal unutilizedLeaves = 15m;
            decimal leaveEncashment = Math.Round((basic / 30m) * unutilizedLeaves, 2);

            // Notice period shortfall recovery
            int shortfallDays = Math.Max(0, req.NoticePeriodDays - req.NoticePeriodServedDays);
            decimal noticeRecovery = Math.Round((gross / 30m) * shortfallDays, 2);

            // Pending salary for days worked in relieving month
            decimal pendingSalary = Math.Round((gross / 30m) * req.PendingSalaryDays, 2);

            // Approved unpaid reimbursements
            var unpaidClaims = (await _unitOfWork.ReimbursementClaims.GetAll())
                .Where(r => r.EmployeeId == req.EmployeeId && r.Status == "Approved" && r.PayRunId == null && !r.IsDeleted).ToList();
            decimal approvedReimbursements = unpaidClaims.Sum(r => r.ApprovedAmount > 0 ? r.ApprovedAmount : r.Amount);

            decimal netSettlement = pendingSalary + leaveEncashment + gratuity + approvedReimbursements + req.OtherAllowances - noticeRecovery - req.OtherDeductions;

            long currentUserId = _userContext.GetCurrentEmployeeId();

            var fnf = new FullAndFinalSettlement
            {
                EmployeeId = req.EmployeeId,
                ResignationId = req.ResignationId,
                RelievingDate = req.RelievingDate,
                TenureYears = tenureYears,
                LastDrawnBasicSalary = basic,
                LastDrawnGrossSalary = gross,
                UnutilizedLeaveDays = unutilizedLeaves,
                LeaveEncashmentAmount = leaveEncashment,
                IsGratuityEligible = gratuityEligible,
                GratuityAmount = gratuity,
                NoticePeriodDays = req.NoticePeriodDays,
                NoticePeriodShortfallDays = shortfallDays,
                NoticeRecoveryAmount = noticeRecovery,
                PendingSalaryDays = req.PendingSalaryDays,
                PendingSalaryAmount = pendingSalary,
                ApprovedReimbursements = approvedReimbursements,
                OtherAllowances = req.OtherAllowances,
                OtherDeductions = req.OtherDeductions,
                NetSettlementAmount = Math.Max(0, netSettlement),
                Status = "Draft",
                Remarks = req.Remarks,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.FullAndFinalSettlements.Add(fnf);
            _unitOfWork.Save();
            return fnf;
        }

        public async Task<FullAndFinalSettlement?> GetFnFSettlementByEmployeeIdAsync(long employeeId)
        {
            var list = await _unitOfWork.FullAndFinalSettlements.GetAll();
            return list.OrderByDescending(f => f.CreatedDate).FirstOrDefault(f => f.EmployeeId == employeeId && !f.IsDeleted);
        }

        public async Task<FullAndFinalSettlement> UpdateFnFStatusAsync(long fnfId, string status, string? remarks)
        {
            var fnf = await _unitOfWork.FullAndFinalSettlements.GetById(fnfId);
            if (fnf == null) throw new KeyNotFoundException($"FnF {fnfId} not found");

            long currentUserId = _userContext.GetCurrentEmployeeId();
            fnf.Status = status;
            if (!string.IsNullOrEmpty(remarks)) fnf.Remarks = remarks;
            fnf.UpdatedBy = (int)currentUserId;
            fnf.UpdatedDate = DateTime.UtcNow;

            if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                fnf.ApprovedByEmployeeId = (int)currentUserId;
                fnf.ApprovedDate = DateTime.UtcNow;
            }
            else if (status.Equals("Settled", StringComparison.OrdinalIgnoreCase))
            {
                fnf.SettledDate = DateTime.UtcNow;
            }

            _unitOfWork.FullAndFinalSettlements.Update(fnf);
            _unitOfWork.Save();
            return fnf;
        }

        #endregion
    }
}
