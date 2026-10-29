using KHRMS.Core.Models;

namespace KHRMS.Core.Interfaces
{
    public interface IWorkflowDefinitionRepository : IGenericRepository<WorkflowDefinition> { }
    public interface IWorkflowStepRepository : IGenericRepository<WorkflowStep> { }
    public interface IWorkflowRoutingRuleRepository : IGenericRepository<WorkflowRoutingRule> { }
    public interface IWorkflowInstanceRepository : IGenericRepository<WorkflowInstance> { }
    public interface IWorkflowInstanceStepRepository : IGenericRepository<WorkflowInstanceStep> { }
    public interface IWorkflowDelegationRepository : IGenericRepository<WorkflowDelegation> { }
    public interface IWorkflowAuditLogRepository : IGenericRepository<WorkflowAuditLog> { }
}
