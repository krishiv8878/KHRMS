using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface ITimesheetService
    {
        Task<TimesheetViewDTO> GetPeriodTimesheetAsync(long employeeId, DateTime startDate, DateTime endDate, long? timesheetId = null);
        Task<TimesheetEntryDTO> SaveEntryAsync(TimesheetEntryDTO dto, long employeeId);
        Task<bool> DeleteEntryAsync(long entryId, long employeeId);
        Task<TimesheetViewDTO> SubmitPeriodAsync(TimesheetSubmitDTO dto, long employeeId);
        Task<List<TimesheetViewDTO>> GetPendingApprovalsForManagerAsync(long managerId, bool isAdminOrHR = false);
        Task<bool> ApproveOrRejectAsync(TimesheetApprovalDTO dto, long managerId);
    }
}
