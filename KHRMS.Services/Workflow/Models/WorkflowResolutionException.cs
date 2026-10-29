using System;

namespace KHRMS.Services.Workflow.Models
{
    public class WorkflowResolutionException : Exception
    {
        public WorkflowResolutionException(string message) : base(message) { }
        public WorkflowResolutionException(string message, Exception inner) : base(message, inner) { }
    }
}
