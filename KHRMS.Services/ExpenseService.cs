using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly KHRMSContextClass _dbContext;
        private readonly IUserContextService _userContext;
        private static bool _tablesVerified = false;
        private static readonly object _tableInitLock = new();

        public ExpenseService(IUnitOfWork unitOfWork, KHRMSContextClass dbContext, IUserContextService userContext)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _userContext = userContext;
        }


        #region 1. Categories

        public async Task<List<ExpenseCategory>> GetCategoriesAsync(bool includeInactive = false)
        {
            var all = (await _unitOfWork.ExpenseCategories.GetAll()).ToList();
            if (!includeInactive)
                all = all.Where(c => c.IsActive && !c.IsDeleted).ToList();
            else
                all = all.Where(c => !c.IsDeleted).ToList();

            return all.OrderBy(c => c.CategoryName).ToList();
        }

        public async Task<ExpenseCategory> SaveCategoryAsync(ExpenseCategory category)
        {
            if (category.Id <= 0)
            {
                category.CreatedDate = DateTime.UtcNow;
                category.IsActive = true;
                await _unitOfWork.ExpenseCategories.Add(category);
            }
            else
            {
                category.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.ExpenseCategories.Update(category);
            }
            _unitOfWork.Save();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(long id)
        {
            var cat = await _unitOfWork.ExpenseCategories.GetById(id);
            if (cat == null) return false;
            cat.IsActive = false;
            cat.IsDeleted = true;
            _unitOfWork.ExpenseCategories.Update(cat);
            _unitOfWork.Save();
            return true;
        }

        #endregion

        #region 2. Expense Reports & Claims

        public async Task<List<ExpenseReportDetailDto>> GetReportsAsync(long? employeeId, string? status, DateTime? fromDate, DateTime? toDate)
        {
            var reports = (await _unitOfWork.ExpenseReports.GetAll())
                .Where(r => !r.IsDeleted)
                .ToList();

            if (employeeId.HasValue && employeeId.Value > 0)
                reports = reports.Where(r => r.EmployeeId == employeeId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                reports = reports.Where(r => r.Status == status).ToList();

            if (fromDate.HasValue)
                reports = reports.Where(r => r.CreatedDate >= fromDate.Value).ToList();

            if (toDate.HasValue)
                reports = reports.Where(r => r.CreatedDate <= toDate.Value).ToList();

            reports = reports.OrderByDescending(r => r.CreatedDate).ToList();
            var result = new List<ExpenseReportDetailDto>();

            var employees = (await _unitOfWork.Employees.GetAll()).ToList();
            var designations = (await _unitOfWork.Designations.GetAll()).ToList();
            var projects = (await _unitOfWork.ProjectMasters.GetAll()).ToList();
            var categories = (await _unitOfWork.ExpenseCategories.GetAll()).ToList();
            var allItems = (await _unitOfWork.ExpenseItems.GetAll()).Where(i => !i.IsDeleted).ToList();
            var allMileages = (await _unitOfWork.ExpenseMileages.GetAll()).Where(m => !m.IsDeleted).ToList();

            foreach (var r in reports)
            {
                var emp = employees.FirstOrDefault(e => e.Id == r.EmployeeId);
                var desig = emp != null ? designations.FirstOrDefault(d => d.Id == emp.DesignationId) : null;
                var proj = r.ProjectId.HasValue ? projects.FirstOrDefault(p => p.Id == r.ProjectId.Value) : null;
                var mgr = r.ManagerId.HasValue ? employees.FirstOrDefault(e => e.Id == r.ManagerId.Value) : null;

                var items = allItems.Where(i => i.ExpenseReportId == r.Id).ToList();

                var itemDtos = new List<ExpenseItemDetailDto>();
                foreach (var item in items)
                {
                    var cat = categories.FirstOrDefault(c => c.Id == item.ExpenseCategoryId);
                    var mileage = allMileages.FirstOrDefault(m => m.ExpenseItemId == item.Id);

                    itemDtos.Add(new ExpenseItemDetailDto
                    {
                        Item = item,
                        CategoryName = cat?.CategoryName ?? "General",
                        Mileage = mileage
                    });
                }

                result.Add(new ExpenseReportDetailDto
                {
                    Report = r,
                    Items = itemDtos,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : "Employee",
                    EmployeeCode = emp?.EmployeeCode?.ToString() ?? string.Empty,
                    DepartmentName = emp?.Branch ?? "General",
                    DesignationName = desig?.DesignationName ?? emp?.Designation ?? string.Empty,
                    ProjectName = proj?.ProjectName,
                    ManagerName = mgr != null ? $"{mgr.FirstName} {mgr.LastName}".Trim() : null
                });
            }

            return result;
        }

        public async Task<ExpenseReportDetailDto?> GetReportByIdAsync(long id)
        {
            var r = await _unitOfWork.ExpenseReports.GetById(id);
            if (r == null || r.IsDeleted) return null;

            var list = await GetReportsAsync(r.EmployeeId, null, null, null);
            return list.FirstOrDefault(x => x.Report.Id == id);
        }

        public async Task<ExpenseReportDetailDto> CreateReportAsync(CreateExpenseReportRequest request)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            long empId = currentEmpId > 0 ? currentEmpId : 1;

            var report = new ExpenseReport
            {
                EmployeeId = empId,
                Title = request.Title,
                Purpose = request.Purpose,
                ProjectId = request.ProjectId,
                Status = "Draft",
                CreatedBy = (int)empId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.ExpenseReports.Add(report);
            _unitOfWork.Save();

            decimal totalAmount = 0;
            foreach (var itemReq in request.Items)
            {
                var item = new ExpenseItem
                {
                    ExpenseReportId = report.Id,
                    ExpenseCategoryId = itemReq.ExpenseCategoryId,
                    ExpenseDate = itemReq.ExpenseDate,
                    MerchantName = itemReq.MerchantName,
                    Amount = itemReq.Amount,
                    TaxAmount = itemReq.TaxAmount,
                    Description = itemReq.Description,
                    ReceiptUrl = itemReq.ReceiptUrl,
                    ApprovedAmount = itemReq.Amount,
                    Status = "Pending",
                    CreatedBy = (int)empId,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.ExpenseItems.Add(item);
                _unitOfWork.Save();

                if (itemReq.Mileage != null)
                {
                    var mileage = new ExpenseMileage
                    {
                        ExpenseItemId = item.Id,
                        VehicleType = itemReq.Mileage.VehicleType,
                        StartLocation = itemReq.Mileage.StartLocation,
                        EndLocation = itemReq.Mileage.EndLocation,
                        DistanceKm = itemReq.Mileage.DistanceKm,
                        RatePerKm = itemReq.Mileage.RatePerKm,
                        CalculatedAmount = itemReq.Mileage.DistanceKm * itemReq.Mileage.RatePerKm,
                        CreatedBy = (int)empId,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _unitOfWork.ExpenseMileages.Add(mileage);
                    _unitOfWork.Save();

                    item.Amount = mileage.CalculatedAmount;
                    item.ApprovedAmount = mileage.CalculatedAmount;
                    _unitOfWork.ExpenseItems.Update(item);
                    _unitOfWork.Save();
                }

                totalAmount += item.Amount;
            }

            decimal advanceAdjust = 0;
            if (request.AdvanceIdToAdjust.HasValue && request.AdvanceIdToAdjust.Value > 0)
            {
                var adv = await _unitOfWork.ExpenseAdvances.GetById(request.AdvanceIdToAdjust.Value);
                if (adv != null && adv.Status == "Disbursed")
                {
                    advanceAdjust = adv.ApprovedAmount;
                    adv.Status = "Settled";
                    adv.SettledReportId = report.Id;
                    _unitOfWork.ExpenseAdvances.Update(adv);
                }
            }

            report.TotalClaimedAmount = totalAmount;
            report.TotalApprovedAmount = totalAmount;
            report.AdvanceAdjustedAmount = advanceAdjust;
            report.NetPayableAmount = Math.Max(0, totalAmount - advanceAdjust);

            _unitOfWork.ExpenseReports.Update(report);
            _unitOfWork.Save();

            return (await GetReportByIdAsync(report.Id))!;
        }

        public async Task<ExpenseReportDetailDto> UpdateReportAsync(UpdateExpenseReportRequest request)
        {
            var report = await _unitOfWork.ExpenseReports.GetById(request.Id);
            if (report == null) throw new KeyNotFoundException("Expense Report not found");

            long currentEmpId = _userContext.GetCurrentEmployeeId();
            report.Title = request.Title;
            report.Purpose = request.Purpose;
            report.ProjectId = request.ProjectId;
            report.UpdatedDate = DateTime.UtcNow;

            var oldItems = (await _unitOfWork.ExpenseItems.GetAll())
                .Where(i => i.ExpenseReportId == report.Id)
                .ToList();

            var allMileages = (await _unitOfWork.ExpenseMileages.GetAll()).ToList();

            foreach (var item in oldItems)
            {
                var mil = allMileages.FirstOrDefault(m => m.ExpenseItemId == item.Id);
                if (mil != null) _unitOfWork.ExpenseMileages.Delete(mil);
                _unitOfWork.ExpenseItems.Delete(item);
            }
            _unitOfWork.Save();

            decimal totalAmount = 0;
            foreach (var itemReq in request.Items)
            {
                var item = new ExpenseItem
                {
                    ExpenseReportId = report.Id,
                    ExpenseCategoryId = itemReq.ExpenseCategoryId,
                    ExpenseDate = itemReq.ExpenseDate,
                    MerchantName = itemReq.MerchantName,
                    Amount = itemReq.Amount,
                    TaxAmount = itemReq.TaxAmount,
                    Description = itemReq.Description,
                    ReceiptUrl = itemReq.ReceiptUrl,
                    ApprovedAmount = itemReq.Amount,
                    Status = "Pending",
                    CreatedBy = (int)currentEmpId,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.ExpenseItems.Add(item);
                _unitOfWork.Save();

                if (itemReq.Mileage != null)
                {
                    var mileage = new ExpenseMileage
                    {
                        ExpenseItemId = item.Id,
                        VehicleType = itemReq.Mileage.VehicleType,
                        StartLocation = itemReq.Mileage.StartLocation,
                        EndLocation = itemReq.Mileage.EndLocation,
                        DistanceKm = itemReq.Mileage.DistanceKm,
                        RatePerKm = itemReq.Mileage.RatePerKm,
                        CalculatedAmount = itemReq.Mileage.DistanceKm * itemReq.Mileage.RatePerKm,
                        CreatedBy = (int)currentEmpId,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    await _unitOfWork.ExpenseMileages.Add(mileage);
                    _unitOfWork.Save();

                    item.Amount = mileage.CalculatedAmount;
                    item.ApprovedAmount = mileage.CalculatedAmount;
                    _unitOfWork.ExpenseItems.Update(item);
                    _unitOfWork.Save();
                }

                totalAmount += item.Amount;
            }

            report.TotalClaimedAmount = totalAmount;
            report.TotalApprovedAmount = totalAmount;
            report.NetPayableAmount = Math.Max(0, totalAmount - report.AdvanceAdjustedAmount);

            _unitOfWork.ExpenseReports.Update(report);
            _unitOfWork.Save();

            return (await GetReportByIdAsync(report.Id))!;
        }

        public async Task<bool> DeleteReportAsync(long id)
        {
            var report = await _unitOfWork.ExpenseReports.GetById(id);
            if (report == null) return false;

            report.IsDeleted = true;
            _unitOfWork.ExpenseReports.Update(report);
            _unitOfWork.Save();
            return true;
        }

        public async Task<ExpenseReportDetailDto> SubmitReportAsync(long id)
        {
            var report = await _unitOfWork.ExpenseReports.GetById(id);
            if (report == null) throw new KeyNotFoundException("Expense report not found");

            report.Status = "Submitted";
            report.SubmittedDate = DateTime.UtcNow;

            var emp = await _unitOfWork.Employees.GetById(report.EmployeeId);
            if (emp != null && emp.ManagerId.HasValue)
            {
                report.ManagerId = emp.ManagerId.Value;
            }

            _unitOfWork.ExpenseReports.Update(report);
            _unitOfWork.Save();

            return (await GetReportByIdAsync(id))!;
        }

        #endregion

        #region 3. Approvals & Audit

        public async Task<ExpenseReportDetailDto> ReviewReportAsync(ReviewExpenseReportRequest request)
        {
            var report = await _unitOfWork.ExpenseReports.GetById(request.ReportId);
            if (report == null) throw new KeyNotFoundException("Expense report not found");

            long currentEmpId = _userContext.GetCurrentEmployeeId();
            long reviewerId = currentEmpId > 0 ? currentEmpId : 1;

            if (request.Action == "ManagerApprove")
            {
                report.Status = "ManagerApproved";
                report.ManagerId = reviewerId;
                report.ManagerActionDate = DateTime.UtcNow;
                report.ManagerRemarks = request.Remarks;
                report.TotalApprovedAmount = request.ApprovedAmount > 0 ? request.ApprovedAmount : report.TotalClaimedAmount;
            }
            else if (request.Action == "FinanceApprove")
            {
                report.Status = "FinanceApproved";
                report.FinanceApprovedBy = reviewerId;
                report.FinanceActionDate = DateTime.UtcNow;
                report.FinanceRemarks = request.Remarks;
                report.TotalApprovedAmount = request.ApprovedAmount > 0 ? request.ApprovedAmount : report.TotalApprovedAmount;
            }
            else if (request.Action == "Reject")
            {
                report.Status = "Rejected";
                report.ManagerActionDate = DateTime.UtcNow;
                report.ManagerRemarks = request.Remarks;
                report.TotalApprovedAmount = 0;
            }

            report.NetPayableAmount = Math.Max(0, report.TotalApprovedAmount - report.AdvanceAdjustedAmount);

            if (request.ItemDecisions != null && request.ItemDecisions.Count > 0)
            {
                foreach (var dec in request.ItemDecisions)
                {
                    var itm = await _unitOfWork.ExpenseItems.GetById(dec.ItemId);
                    if (itm != null)
                    {
                        itm.Status = dec.Status;
                        itm.ApprovedAmount = dec.ApprovedAmount;
                        itm.RejectionReason = dec.RejectionReason;
                        _unitOfWork.ExpenseItems.Update(itm);
                    }
                }
            }

            _unitOfWork.ExpenseReports.Update(report);
            _unitOfWork.Save();

            return (await GetReportByIdAsync(report.Id))!;
        }

        public async Task<ExpenseReportDetailDto> DisburseReportAsync(DisburseExpenseReportRequest request)
        {
            var report = await _unitOfWork.ExpenseReports.GetById(request.ReportId);
            if (report == null) throw new KeyNotFoundException("Expense report not found");

            report.Status = "Paid";
            report.PaidDate = DateTime.UtcNow;
            report.PaymentMode = request.PaymentMode;
            report.PayRunId = request.PayRunId;

            _unitOfWork.ExpenseReports.Update(report);

            var claims = (await _unitOfWork.ReimbursementClaims.GetAll()).ToList();
            var existingClaim = claims.FirstOrDefault(c => c.EmployeeId == report.EmployeeId && c.Description != null && c.Description.Contains($"Report #{report.Id}"));

            if (existingClaim == null && report.NetPayableAmount > 0)
            {
                var claim = new ReimbursementClaim
                {
                    EmployeeId = report.EmployeeId,
                    Category = "Expense Report",
                    Amount = report.NetPayableAmount,
                    ApprovedAmount = report.NetPayableAmount,
                    BillDate = DateTime.UtcNow,
                    BillNumber = $"EXP-{report.Id}",
                    MerchantName = "Various",
                    Description = $"Automated sync from Expense Report #{report.Id} - {report.Title}",
                    Status = "Paid",
                    PayRunId = request.PayRunId
                };
                await _unitOfWork.ReimbursementClaims.Add(claim);
            }

            _unitOfWork.Save();
            return (await GetReportByIdAsync(report.Id))!;
        }

        #endregion

        #region 4. Cash Advances

        public async Task<List<ExpenseAdvanceDto>> GetAdvancesAsync(long? employeeId, string? status)
        {
            var query = (await _unitOfWork.ExpenseAdvances.GetAll())
                .Where(a => !a.IsDeleted)
                .ToList();

            if (employeeId.HasValue && employeeId.Value > 0)
                query = query.Where(a => a.EmployeeId == employeeId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                query = query.Where(a => a.Status == status).ToList();

            var advances = query.OrderByDescending(a => a.CreatedDate).ToList();
            var result = new List<ExpenseAdvanceDto>();

            var employees = (await _unitOfWork.Employees.GetAll()).ToList();
            var designations = (await _unitOfWork.Designations.GetAll()).ToList();

            foreach (var a in advances)
            {
                var emp = employees.FirstOrDefault(e => e.Id == a.EmployeeId);
                var desig = emp != null ? designations.FirstOrDefault(d => d.Id == emp.DesignationId) : null;

                result.Add(new ExpenseAdvanceDto
                {
                    Advance = a,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : "Employee",
                    EmployeeCode = emp?.EmployeeCode?.ToString() ?? string.Empty,
                    DepartmentName = emp?.Branch ?? "General",
                    DesignationName = desig?.DesignationName ?? emp?.Designation ?? string.Empty
                });
            }

            return result;
        }

        public async Task<ExpenseAdvanceDto> RequestAdvanceAsync(CreateExpenseAdvanceRequest request)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            long empId = currentEmpId > 0 ? currentEmpId : 1;

            var advance = new ExpenseAdvance
            {
                EmployeeId = empId,
                AmountRequested = request.AmountRequested,
                ApprovedAmount = request.AmountRequested,
                Purpose = request.Purpose,
                TravelStartDate = request.TravelStartDate,
                TravelEndDate = request.TravelEndDate,
                Status = "Requested",
                CreatedBy = (int)empId,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.ExpenseAdvances.Add(advance);
            _unitOfWork.Save();

            var list = await GetAdvancesAsync(empId, null);
            return list.First(a => a.Advance.Id == advance.Id);
        }

        public async Task<ExpenseAdvanceDto> ReviewAdvanceAsync(ReviewExpenseAdvanceRequest request)
        {
            var advance = await _unitOfWork.ExpenseAdvances.GetById(request.AdvanceId);
            if (advance == null) throw new KeyNotFoundException("Expense advance not found");

            long currentEmpId = _userContext.GetCurrentEmployeeId();
            long reviewerId = currentEmpId > 0 ? currentEmpId : 1;

            if (request.Action == "Approve")
            {
                advance.Status = "Approved";
                advance.ApprovedAmount = request.ApprovedAmount > 0 ? request.ApprovedAmount : advance.AmountRequested;
                advance.ApprovedBy = reviewerId;
                advance.ApprovedDate = DateTime.UtcNow;
                advance.Remarks = request.Remarks;
            }
            else if (request.Action == "Disburse")
            {
                advance.Status = "Disbursed";
                advance.DisbursedDate = DateTime.UtcNow;
                advance.Remarks = request.Remarks;
            }
            else if (request.Action == "Reject")
            {
                advance.Status = "Rejected";
                advance.ApprovedBy = reviewerId;
                advance.ApprovedDate = DateTime.UtcNow;
                advance.Remarks = request.Remarks;
                advance.ApprovedAmount = 0;
            }

            _unitOfWork.ExpenseAdvances.Update(advance);
            _unitOfWork.Save();

            var list = await GetAdvancesAsync(advance.EmployeeId, null);
            return list.First(a => a.Advance.Id == advance.Id);
        }

        #endregion
    }
}
