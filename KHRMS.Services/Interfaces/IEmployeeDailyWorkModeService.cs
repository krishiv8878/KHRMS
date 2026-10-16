using System.Security.Claims;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IEmployeeDailyWorkModeService
    {
        Task<WorkModeResponseDTO?> GetWorkModeForDateAsync(long employeeId, DateOnly date);
        Task<IEnumerable<WorkModeResponseDTO>> GetWorkModesForRangeAsync(long employeeId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<WorkModeResponseDTO>> GetAllWorkModesForDateAsync(DateOnly date);
        Task<IEnumerable<WorkModeResponseDTO>> GetAllWorkModesForRangeAsync(DateOnly startDate, DateOnly endDate);
        Task<WorkModeResponseDTO> SetWorkModeAsync(SetWorkModeDTO dto, ClaimsPrincipal user);
    }
}
