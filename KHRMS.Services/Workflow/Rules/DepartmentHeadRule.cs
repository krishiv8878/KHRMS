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
    public class DepartmentHeadRule : IApproverRule
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentHeadRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string RuleType => "DepartmentHead";

        public async Task<List<ResolvedApproverCandidate>> ResolveCandidatesAsync(
            long requesterId,
            WorkflowContext context,
            string? ruleParametersJson)
        {
            var candidates = new List<ResolvedApproverCandidate>();
            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => e.IsActive && !e.IsDeleted).ToList();
            var designations = (await _unitOfWork.Designations.GetAll()).ToList();

            // 1. Direct explicit head in parameters
            if (!string.IsNullOrWhiteSpace(ruleParametersJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(ruleParametersJson);
                    if (doc.RootElement.TryGetProperty("HeadEmployeeId", out var headProp) && headProp.TryGetInt64(out var explicitHeadId))
                    {
                        var head = allEmployees.FirstOrDefault(e => e.Id == explicitHeadId && e.Id != requesterId);
                        if (head != null)
                        {
                            candidates.Add(new ResolvedApproverCandidate
                            {
                                EmployeeId = head.Id,
                                EmployeeName = $"{head.FirstName} {head.LastName}".Trim(),
                                Email = head.EmailAddress,
                                Reason = "Designated Department Head"
                            });
                            return candidates;
                        }
                    }
                }
                catch { }
            }

            // 2. Identify department name
            string? dept = context.Department;
            var requester = allEmployees.FirstOrDefault(e => e.Id == requesterId);

            if (string.IsNullOrWhiteSpace(dept) && requester?.DesignationId.HasValue == true)
            {
                var desig = designations.FirstOrDefault(d => d.Id == requester.DesignationId.Value);
                dept = desig?.DepartmentCategory;
            }

            if (string.IsNullOrWhiteSpace(dept)) return candidates;

            // 3. Find candidates in that department
            var deptDesigIds = designations
                .Where(d => string.Equals(d.DepartmentCategory, dept, StringComparison.OrdinalIgnoreCase))
                .Select(d => d.Id)
                .ToHashSet();

            var deptEmployees = allEmployees
                .Where(e => e.Id != requesterId && e.DesignationId.HasValue && deptDesigIds.Contains(e.DesignationId.Value))
                .ToList();

            // Look for leadership keywords
            var leadershipKeywords = new[] { "Head", "Director", "VP", "Lead", "Chief", "Manager" };
            var headCandidate = deptEmployees.FirstOrDefault(e =>
            {
                var desig = designations.FirstOrDefault(d => d.Id == e.DesignationId!.Value);
                var title = desig?.DesignationName ?? e.Designation ?? string.Empty;
                return leadershipKeywords.Any(k => title.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            }) ?? deptEmployees.FirstOrDefault();

            if (headCandidate != null)
            {
                candidates.Add(new ResolvedApproverCandidate
                {
                    EmployeeId = headCandidate.Id,
                    EmployeeName = $"{headCandidate.FirstName} {headCandidate.LastName}".Trim(),
                    Email = headCandidate.EmailAddress,
                    Reason = $"Department Head for {dept}"
                });
            }

            return candidates;
        }
    }
}
