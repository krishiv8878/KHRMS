using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Core.Models;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Interfaces
{
    public interface IWorkflowEngineService
    {
        Task<WorkflowInstance> StartWorkflowAsync(string requestType, long requestId, long requesterId, WorkflowContext context);
        Task<WorkflowActionResult> ActOnStepAsync(long instanceId, long currentUserId, WorkflowActionRequest request);
        Task<bool> CancelWorkflowAsync(long instanceId, long requesterId, string cancelReason);
        Task<List<WorkflowInboxItemDTO>> GetPendingInboxAsync(long currentUserId);
        Task<List<WorkflowScopedRequestDTO>> GetRequestsInScopeAsync(long currentUserId, string? requestType, string? status);
        Task<WorkflowInstanceDetailDTO?> GetInstanceDetailsAsync(long instanceId, long currentUserId);
        Task EscalateOverdueStepsAsync();
    }
}
