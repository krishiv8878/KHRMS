using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Core.Models;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Interfaces
{
    public interface IApproverResolver
    {
        Task<WorkflowDefinition> ResolveDefinitionAsync(string requestType, WorkflowContext context);
        Task<List<ResolvedStepApprovers>> ResolveStepsAsync(string requestType, long requesterId, WorkflowContext context, WorkflowDefinition? definition = null);
    }
}
