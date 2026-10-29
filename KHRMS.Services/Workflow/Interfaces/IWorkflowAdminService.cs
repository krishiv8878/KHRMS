using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Interfaces
{
    public interface IWorkflowAdminService
    {
        Task<List<WorkflowDefinitionSummaryDTO>> GetDefinitionSummariesAsync();
        Task<WorkflowDefinitionDetailDTO?> GetDefinitionDetailAsync(string requestType, int? version = null);
        Task<WorkflowValidationResultDTO> ValidateDefinitionAsync(WorkflowDefinitionSaveDTO dto);
        Task<WorkflowDefinitionDetailDTO> SaveAndPublishDefinitionAsync(WorkflowDefinitionSaveDTO dto, long currentUserId);
        Task<TestRoutingResponseDTO> TestRoutingAsync(TestRoutingRequestDTO dto);
        Task<List<WorkflowVersionHistoryDTO>> GetVersionHistoryAsync(string requestType);
        Task<bool> RollbackVersionAsync(string requestType, int targetVersion, long currentUserId);
    }
}
