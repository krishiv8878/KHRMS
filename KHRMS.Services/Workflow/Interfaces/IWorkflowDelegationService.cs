using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Interfaces
{
    public interface IWorkflowDelegationService
    {
        Task<List<WorkflowDelegationDTO>> GetMyDelegationsAsync(long employeeId);
        Task<WorkflowDelegationDTO> CreateDelegationAsync(long fromEmployeeId, CreateWorkflowDelegationRequest request);
        Task<bool> RevokeDelegationAsync(long delegationId, long employeeId);
    }
}
