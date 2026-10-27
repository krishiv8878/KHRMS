using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Rules
{
    public class ReportingManagerRule : IApproverRule
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReportingManagerRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string RuleType => "ReportingManager";

        public async Task<List<ResolvedApproverCandidate>> ResolveCandidatesAsync(
            long requesterId,
            WorkflowContext context,
            string? ruleParametersJson)
        {
            var candidates = new List<ResolvedApproverCandidate>();
            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();
            var employee = allEmployees.FirstOrDefault(e => e.Id == requesterId && !e.IsDeleted);

            if (employee == null) return candidates;

            long? currentManagerId = employee.ManagerId;
            var visited = new HashSet<long> { requesterId };

            // Traverse manager hierarchy until finding an active non-requester manager
            while (currentManagerId.HasValue && currentManagerId.Value > 0)
            {
                if (visited.Contains(currentManagerId.Value))
                {
                    // Circular loop detected
                    break;
                }

                visited.Add(currentManagerId.Value);
                var manager = allEmployees.FirstOrDefault(e => e.Id == currentManagerId.Value && !e.IsDeleted);

                if (manager != null)
                {
                    if (manager.IsActive && manager.Id != requesterId)
                    {
                        candidates.Add(new ResolvedApproverCandidate
                        {
                            EmployeeId = manager.Id,
                            EmployeeName = $"{manager.FirstName} {manager.LastName}".Trim(),
                            Email = manager.EmailAddress,
                            Reason = "Direct Reporting Manager"
                        });
                        break;
                    }

                    // If manager is inactive or requester, continue chaining up
                    currentManagerId = manager.ManagerId;
                }
                else
                {
                    break;
                }
            }

            return candidates;
        }
    }
}
