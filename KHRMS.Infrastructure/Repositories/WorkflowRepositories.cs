using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;

namespace KHRMS.Infrastructure.Repositories
{
    public class WorkflowDefinitionRepository : GenericRepository<WorkflowDefinition>, IWorkflowDefinitionRepository
    {
        public WorkflowDefinitionRepository(KHRMSContextClass context) : base(context) { }
    }

    public class WorkflowStepRepository : GenericRepository<WorkflowStep>, IWorkflowStepRepository
    {
        public WorkflowStepRepository(KHRMSContextClass context) : base(context) { }
    }

    public class WorkflowRoutingRuleRepository : GenericRepository<WorkflowRoutingRule>, IWorkflowRoutingRuleRepository
    {
        public WorkflowRoutingRuleRepository(KHRMSContextClass context) : base(context) { }
    }

    public class WorkflowInstanceRepository : GenericRepository<WorkflowInstance>, IWorkflowInstanceRepository
    {
        public WorkflowInstanceRepository(KHRMSContextClass context) : base(context) { }
    }

    public class WorkflowInstanceStepRepository : GenericRepository<WorkflowInstanceStep>, IWorkflowInstanceStepRepository
    {
        public WorkflowInstanceStepRepository(KHRMSContextClass context) : base(context) { }
    }

    public class WorkflowDelegationRepository : GenericRepository<WorkflowDelegation>, IWorkflowDelegationRepository
    {
        public WorkflowDelegationRepository(KHRMSContextClass context) : base(context) { }
    }

    public class WorkflowAuditLogRepository : GenericRepository<WorkflowAuditLog>, IWorkflowAuditLogRepository
    {
        public WorkflowAuditLogRepository(KHRMSContextClass context) : base(context) { }
    }
}
