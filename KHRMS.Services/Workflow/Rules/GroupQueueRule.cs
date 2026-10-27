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
    public class GroupQueueRule : IApproverRule
    {
        private readonly IUnitOfWork _unitOfWork;

        public GroupQueueRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string RuleType => "GroupQueue";

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

            foreach (var emp in allEmployees)
            {
                if (emp.Id != requesterId && roleMappings.Contains(emp.Id))
                {
                    candidates.Add(new ResolvedApproverCandidate
                    {
                        EmployeeId = emp.Id,
                        EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                        Email = emp.EmailAddress,
                        Reason = $"Member of Queue '{targetRole.RoleName}'"
                    });
                }
            }

            return candidates;
        }
    }
}
