using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Rules
{
    public class SpecificPersonRule : IApproverRule
    {
        private readonly IUnitOfWork _unitOfWork;

        public SpecificPersonRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string RuleType => "SpecificPerson";

        public async Task<List<ResolvedApproverCandidate>> ResolveCandidatesAsync(
            long requesterId,
            WorkflowContext context,
            string? ruleParametersJson)
        {
            var candidates = new List<ResolvedApproverCandidate>();
            if (string.IsNullOrWhiteSpace(ruleParametersJson)) return candidates;

            long targetId = 0;
            try
            {
                using var doc = JsonDocument.Parse(ruleParametersJson);
                if (doc.RootElement.TryGetProperty("EmployeeId", out var idProp))
                {
                    targetId = idProp.GetInt64();
                }
            }
            catch { }

            if (targetId <= 0) return candidates;

            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => e.IsActive && !e.IsDeleted).ToList();

            // If specific person is not the requester, use them directly
            if (targetId != requesterId)
            {
                var person = allEmployees.FirstOrDefault(e => e.Id == targetId);
                if (person != null)
                {
                    candidates.Add(new ResolvedApproverCandidate
                    {
                        EmployeeId = person.Id,
                        EmployeeName = $"{person.FirstName} {person.LastName}".Trim(),
                        Email = person.EmailAddress,
                        Reason = "Specific Named Approver"
                    });
                    return candidates;
                }
            }

            // If specific person IS the requester, move up their chain
            var req = allEmployees.FirstOrDefault(e => e.Id == requesterId);
            long? mgrId = req?.ManagerId;
            while (mgrId.HasValue && mgrId.Value > 0 && mgrId.Value != requesterId)
            {
                var mgr = allEmployees.FirstOrDefault(e => e.Id == mgrId.Value);
                if (mgr != null)
                {
                    candidates.Add(new ResolvedApproverCandidate
                    {
                        EmployeeId = mgr.Id,
                        EmployeeName = $"{mgr.FirstName} {mgr.LastName}".Trim(),
                        Email = mgr.EmailAddress,
                        Reason = "Manager of Named Approver (Requester-Exclusion Escalation)"
                    });
                    break;
                }
                mgrId = mgr?.ManagerId;
            }

            return candidates;
        }
    }
}
