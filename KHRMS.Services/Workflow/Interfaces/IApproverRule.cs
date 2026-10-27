using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Interfaces
{
    public interface IApproverRule
    {
        string RuleType { get; }
        Task<List<ResolvedApproverCandidate>> ResolveCandidatesAsync(
            long requesterId,
            WorkflowContext context,
            string? ruleParametersJson);
    }
}
