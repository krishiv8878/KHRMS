using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Rules
{
    public class RoleInLocationRule : IApproverRule
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoleInLocationRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string RuleType => "RoleInLocation";

        public async Task<List<ResolvedApproverCandidate>> ResolveCandidatesAsync(
            long requesterId,
            WorkflowContext context,
            string? ruleParametersJson)
        {
            var candidates = new List<ResolvedApproverCandidate>();
            if (string.IsNullOrWhiteSpace(ruleParametersJson)) return candidates;

            string? roleName = null;
            long? roleId = null;

            try
            {
                using var doc = JsonDocument.Parse(ruleParametersJson);
                if (doc.RootElement.TryGetProperty("RoleName", out var nameProp)) roleName = nameProp.GetString();
                if (doc.RootElement.TryGetProperty("RoleId", out var idProp) && idProp.TryGetInt64(out var rId)) roleId = rId;
            }
            catch { }

            var allRoles = (await _unitOfWork.RoleMaster.GetAll()).Where(r => r.IsActive && !r.IsDeleted).ToList();
            var targetRole = roleId.HasValue 
                ? allRoles.FirstOrDefault(r => r.Id == roleId.Value)
                : allRoles.FirstOrDefault(r => string.Equals(r.RoleName, roleName, StringComparison.OrdinalIgnoreCase));

            if (targetRole == null) return candidates;

            var roleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => m.RoleId == targetRole.Id && m.IsActive == true && m.IsDeleted != true)
                .Select(m => m.EmployeeId)
                .ToHashSet();

            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => e.IsActive && !e.IsDeleted).ToList();
            var requester = allEmployees.FirstOrDefault(e => e.Id == requesterId);
            string? targetLocation = !string.IsNullOrWhiteSpace(context.Location) ? context.Location : requester?.Branch;

            // 1. First attempt: match role + specific location
            var locationMatched = allEmployees
                .Where(e => e.Id != requesterId && roleMappings.Contains(e.Id) &&
                            !string.IsNullOrWhiteSpace(targetLocation) &&
                            string.Equals(e.Branch, targetLocation, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (locationMatched.Count > 0)
            {
                foreach (var emp in locationMatched)
                {
                    candidates.Add(new ResolvedApproverCandidate
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                        Email = emp.EmailAddress,
                        Reason = $"{targetRole.RoleName} at {targetLocation}"
                    });
                }
                return candidates;
            }

            // 2. Fallback: match role organization-wide
            var generalMatched = allEmployees
                .Where(e => e.Id != requesterId && roleMappings.Contains(e.Id))
                .ToList();

            foreach (var emp in generalMatched)
            {
                candidates.Add(new ResolvedApproverCandidate
                {
                    EmployeeId = emp.Id,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    Email = emp.EmailAddress,
                    Reason = $"{targetRole.RoleName} (Company-wide fallback)"
                });
            }

            return candidates;
        }
    }
}
