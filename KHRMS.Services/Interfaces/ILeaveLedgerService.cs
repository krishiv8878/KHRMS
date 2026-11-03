using KHRMS.Core.Models;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface ILeaveLedgerService
    {
        Task<EmployeeLeaveBalanceDto> GetBalanceSummaryAsync(long employeeId, long leaveTypeId);
        Task<IEnumerable<EmployeeLeaveBalanceDto>> GetAllEmployeeBalancesAsync(long employeeId);
        Task<LeaveLedger> RecordTransactionAsync(long employeeId, long leaveTypeId, string transactionType, decimal days, long? leaveRequestId = null, string? period = null, string? remarks = null, long createdBy = 0);
        Task<bool> ReserveBalanceAsync(long employeeId, long leaveTypeId, decimal days, long leaveRequestId);
        Task<bool> CommitDeductionAsync(long employeeId, long leaveTypeId, decimal days, long leaveRequestId);
        Task<bool> ReleaseReservationAsync(long employeeId, long leaveTypeId, decimal days, long leaveRequestId);
        Task<bool> RestoreDeductionAsync(long employeeId, long leaveTypeId, decimal days, long leaveRequestId, string reason);
        Task<IEnumerable<LeaveLedger>> GetLedgerHistoryAsync(long employeeId, long? leaveTypeId = null, int? year = null);
        Task<int> ProcessMonthlyAccrualAsync(int year, int month);
        Task<int> ProcessYearEndRolloverAsync(int currentYear, int targetYear);
        Task<int> ProcessCompOffExpiryAsync();
    }
}
