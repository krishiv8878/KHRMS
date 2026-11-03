using KHRMS.Core;
using KHRMS.Services.Request;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public interface IEmployeeService
    {
        Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel);
        Task<IEnumerable<EmployeeRequestModel>> GetAllEmployees();
        Task<IEnumerable<EmployeeLookupDto>> GetEmployeesLookupAsync();
        Task<Employee> GetEmployeeById(int employeeId);
        Task<bool> UpdateEmployee(EmployeeRequestModel employeeRequestModel);
        Task<bool> ExistingEmployeeUpdate(EmployeeRequestModel employeeRequestModel);
        Task<bool> DeleteEmployee(long employeeId);
        Task<IEnumerable<EmployeeRequestModel>> GetAllManagers();
        Task<string?> UploadProfileImage(ProfileImageRequest request);
        Task<IEnumerable<EmployeeRequestModel>> GetProbationRemindersAsync();
        Task<bool> ProcessProbationDecisionAsync(long employeeId, ProbationDecisionDto dto);

        // Tenures, Rehire & Notice Overrides
        Task<bool> UpdateNoticeOverrideAsync(long employeeId, NoticePeriodOverrideDto dto);
        Task<IEnumerable<MatchingFormerEmployeeDto>> GetMatchingFormerEmployeesAsync(string? email, string? pan, string? uan);
        Task<IEnumerable<HistoricalRehireReportItemDto>> GetHistoricalRehiresReportAsync();
        Task<IEnumerable<PreviousEmploymentHistoryDto>> GetPreviousEmploymentHistoryAsync(long employeeId);
    }
}
