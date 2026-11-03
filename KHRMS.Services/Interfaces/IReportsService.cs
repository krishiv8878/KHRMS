using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IReportsService
    {
        Task<List<AttendanceReportItemDto>> GetAttendanceSummaryReportAsync(int month, int year, string? department = null);
        Task<byte[]> ExportAttendanceSummaryCsvAsync(int month, int year, string? department = null);

        Task<List<LeaveBalanceReportItemDto>> GetLeaveBalanceReportAsync(string? department = null, long? leaveTypeId = null);
        Task<byte[]> ExportLeaveBalanceCsvAsync(string? department = null, long? leaveTypeId = null);

        Task<List<PayrollReconciliationReportItemDto>> GetPayrollReconciliationReportAsync(long? payRunId = null, int? month = null, int? year = null, string? department = null);
        Task<byte[]> ExportPayrollReconciliationCsvAsync(long? payRunId = null, int? month = null, int? year = null, string? department = null);

        Task<HeadcountDemographicsReportDto> GetHeadcountDemographicsReportAsync();
        Task<byte[]> ExportHeadcountDemographicsCsvAsync();
    }
}
