using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Interfaces
{
    public interface IConditionEvaluator
    {
        bool Evaluate(string? conditionJson, WorkflowContext context);
    }
}
