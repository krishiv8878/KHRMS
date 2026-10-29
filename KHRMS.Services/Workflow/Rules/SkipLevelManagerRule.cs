using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Rules
{
    public class SkipLevelManagerRule : IApproverRule
    {
        private readonly IUnitOfWork _unitOfWork;

        public SkipLevelManagerRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string RuleType => "SkipLevelManager";

        public async Task<List<ResolvedApproverCandidate>> ResolveCandidatesAsync(
            long requesterId,
            WorkflowContext context,
            string? ruleParametersJson)
        {
            var candidates = new List<ResolvedApproverCandidate>();
            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();
            var employee = allEmployees.FirstOrDefault(e => e.Id == requesterId && !e.IsDeleted);

            if (employee == null) return candidates;

            var visited = new HashSet<long> { requesterId };
            long? currentId = employee.ManagerId;
            int level = 0;

            while (currentId.HasValue && currentId.Value > 0)
            {
                if (visited.Contains(currentId.Value)) break;
                visited.Add(currentId.Value);

                var mgr = allEmployees.FirstOrDefault(e => e.Id == currentId.Value && !e.IsDeleted);
                if (mgr != null)
                {
                    if (mgr.IsActive && mgr.Id != requesterId)
                    {
                        level++;
                        if (level == 2)
                        {
                            candidates.Add(new ResolvedApproverCandidate
                            {
                                EmployeeId = mgr.Id,
                                EmployeeName = $"{mgr.FirstName} {mgr.LastName}".Trim(),
                                Email = mgr.EmailAddress,
                                Reason = "Skip-Level Manager (Manager's Manager)"
                            });
                            break;
                        }
                    }
                    currentId = mgr.ManagerId;
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
