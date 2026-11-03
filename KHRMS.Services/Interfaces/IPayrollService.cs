using KHRMS.Core.Models;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IPayrollService
    {
        // Salary Structure & Components
        Task<IEnumerable<SalaryComponent>> GetAllComponentsAsync();
        Task<SalaryComponent> AddSalaryComponentAsync(SalaryComponent component);
        Task<SalaryComponent> UpdateSalaryComponentAsync(SalaryComponent component);
        Task<bool> DeleteSalaryComponentAsync(long id);
        SalaryCalculationResult CalculateCtcBreakdown(SalaryCalculationRequest req);
        Task<EmployeeSalaryAssignment?> GetEmployeeSalaryAssignmentAsync(long employeeId);
        Task<IEnumerable<EmployeeSalaryAssignment>> GetAllSalaryAssignmentsAsync();
        Task<IEnumerable<EmployeeSalaryAssignment>> GetEmployeeSalaryHistoryAsync(long employeeId);
        Task<EmployeeSalaryAssignment> AssignSalaryAsync(AssignSalaryRequest req);

        // Pay Run Engine
        Task<IEnumerable<PayRun>> GetAllPayRunsAsync();
        Task<PayRun?> GetPayRunByIdAsync(long id);
        Task<IEnumerable<PayRunEmployeeDetail>> GetPayRunDetailsAsync(long payRunId);
        Task<PayRun> CreatePayRunAsync(CreatePayRunRequest req);
        Task<PayRun> UpdatePayRunStatusAsync(UpdatePayRunStatusRequest req);
        Task<bool> DeletePayRunAsync(long payRunId);
        Task<bool> LockPayRunAsync(long payRunId, long employeeId);
        Task<bool> RecalculatePayRunAsync(long payRunId);
        Task<bool> ToggleHoldEmployeeDetailAsync(long detailId, bool isHold, string? reason);
        Task<bool> UpdateEmployeeAdjustmentAsync(long detailId, decimal manualAdjustment, decimal arrears, decimal bonus, string? reason);

        // Employee Loans & Advances
        Task<IEnumerable<EmployeeLoan>> GetAllLoansAsync(long? employeeId = null);
        Task<EmployeeLoan> ApplyLoanAsync(ApplyEmployeeLoanRequest req, long createdBy);
        Task<EmployeeLoan> UpdateLoanStatusAsync(long loanId, string status, string? reason);

        // Payslips & Self-Service
        Task<PayslipDto> RecordIndividualSalaryCreditAsync(IndividualSalaryCreditRequest req);
        Task<IEnumerable<PayslipDto>> GetAllPayslipsAsync();
        Task<IEnumerable<PayslipDto>> GetEmployeePayslipsAsync(long employeeId);
        Task<PayslipDto?> GetPayslipByIdAsync(long id);
        Task<string> GeneratePayslipHtmlAsync(long payslipId);

        // Reimbursements & FBP
        Task<IEnumerable<ReimbursementClaim>> GetEmployeeReimbursementsAsync(long employeeId);
        Task<IEnumerable<ReimbursementClaim>> GetAllPendingReimbursementsAsync();
        Task<ReimbursementClaim> SubmitReimbursementClaimAsync(ReimbursementClaimRequest req);
        Task<ReimbursementClaim> ReviewReimbursementClaimAsync(ReviewReimbursementRequest req);

        // Bank Payout Export
        Task<byte[]> GenerateBankPayoutCsvAsync(long payRunId);

        // Full & Final Settlement
        Task<FullAndFinalSettlement> CalculateFnFSettlementAsync(FullAndFinalCalculationRequest req);
        Task<FullAndFinalSettlement?> GetFnFSettlementByEmployeeIdAsync(long employeeId);
        Task<FullAndFinalSettlement> UpdateFnFStatusAsync(long fnfId, string status, string? remarks);
    }
}
