using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IChecklistService
    {
        Task<IEnumerable<ChecklistTemplateDto>> GetTemplatesAsync(string? checklistType = null, string? department = null);
        Task<ChecklistTemplateDto?> GetTemplateByIdAsync(long id);
        Task<ChecklistTemplateDto> SaveTemplateAsync(ChecklistTemplateDto dto);
        Task<bool> DeleteTemplateAsync(long id);

        Task<EmployeeChecklistDto> InstantiateChecklistForEmployeeAsync(long employeeId, string checklistType, long? templateId = null);
        Task<IEnumerable<EmployeeChecklistDto>> GetEmployeeChecklistsAsync(long? employeeId = null, string? checklistType = null);
        Task<EmployeeChecklistDto?> GetChecklistByIdAsync(long id);
        Task<bool> UpdateTaskStatusAsync(long taskId, ChecklistTaskUpdateDto dto);
        Task<bool> UpdateTaskAssigneeAsync(long taskId, TaskAssigneeUpdateDto dto);
        Task<AssetReturnVerificationDto> VerifyAssetReturnsAsync(long employeeId);
        Task<IEnumerable<ChecklistTaskDto>> GetPendingTasksForUserAsync(long? employeeId = null);
    }
}
