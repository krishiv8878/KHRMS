using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Rules
{
    public class ProjectOwnerRule : IApproverRule
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProjectOwnerRule(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string RuleType => "ProjectOwner";

        public async Task<List<ResolvedApproverCandidate>> ResolveCandidatesAsync(
            long requesterId,
            WorkflowContext context,
            string? ruleParametersJson)
        {
            var candidates = new List<ResolvedApproverCandidate>();
            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => e.IsActive && !e.IsDeleted).ToList();
            var allProjects = (await _unitOfWork.ProjectMasters.GetAll()).Where(p => p.IsActive && !p.IsDeleted).ToList();

            long? projectId = context.ProjectId;
            long configuredEmployeeId = 0;

            if (!string.IsNullOrWhiteSpace(ruleParametersJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(ruleParametersJson);
                    if (doc.RootElement.TryGetProperty("EmployeeId", out var empProp) && empProp.TryGetInt64(out var empVal))
                    {
                        configuredEmployeeId = empVal;
                    }
                    if (!projectId.HasValue && doc.RootElement.TryGetProperty("ProjectId", out var projProp) && projProp.TryGetInt64(out var projVal))
                    {
                        projectId = projVal;
                    }
                }
                catch { }
            }

            // If a specific project-based approver person was configured
            if (configuredEmployeeId > 0)
            {
                var proj = projectId.HasValue ? allProjects.FirstOrDefault(p => p.Id == projectId.Value) : null;
                string projName = proj?.ProjectName ?? "Project";

                if (configuredEmployeeId != requesterId)
                {
                    var specificPerson = allEmployees.FirstOrDefault(e => e.Id == configuredEmployeeId);
                    if (specificPerson != null)
                    {
                        candidates.Add(new ResolvedApproverCandidate
                        {
                            EmployeeId = specificPerson.Id,
                            EmployeeName = $"{specificPerson.FirstName} {specificPerson.LastName}".Trim(),
                            Email = specificPerson.EmailAddress,
                            Reason = $"Designated Project Approver for '{projName}'"
                        });
                        return candidates;
                    }
                }

                // If configured person is requester, move up their manager chain
                var personEmp = allEmployees.FirstOrDefault(e => e.Id == configuredEmployeeId);
                long? mgrId = personEmp?.ManagerId;
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
                            Reason = $"Manager of Designated Project Approver for '{projName}'"
                        });
                        return candidates;
                    }
                    mgrId = mgr?.ManagerId;
                }
            }

            if (!projectId.HasValue)
            {
                var requester = allEmployees.FirstOrDefault(e => e.Id == requesterId);
                if (requester?.ProjectIds != null && requester.ProjectIds.Count > 0)
                {
                    projectId = requester.ProjectIds.First();
                }
            }

            if (!projectId.HasValue) return candidates;

            var project = allProjects.FirstOrDefault(p => p.Id == projectId.Value);
            if (project != null && project.ProjectManagerId.HasValue && project.ProjectManagerId.Value > 0)
            {
                long pmId = project.ProjectManagerId.Value;
                if (pmId != requesterId)
                {
                    var pm = allEmployees.FirstOrDefault(e => e.Id == pmId);
                    if (pm != null)
                    {
                        candidates.Add(new ResolvedApproverCandidate
                        {
                            EmployeeId = pm.Id,
                            EmployeeName = $"{pm.FirstName} {pm.LastName}".Trim(),
                            Email = pm.EmailAddress,
                            Reason = $"Project Manager for '{project.ProjectName}'"
                        });
                        return candidates;
                    }
                }

                // If PM is requester, move up PM's manager chain
                var pmEmployee = allEmployees.FirstOrDefault(e => e.Id == pmId);
                long? upMgrId = pmEmployee?.ManagerId;
                while (upMgrId.HasValue && upMgrId.Value > 0 && upMgrId.Value != requesterId)
                {
                    var upMgr = allEmployees.FirstOrDefault(e => e.Id == upMgrId.Value);
                    if (upMgr != null)
                    {
                        candidates.Add(new ResolvedApproverCandidate
                        {
                            EmployeeId = upMgr.Id,
                            EmployeeName = $"{upMgr.FirstName} {upMgr.LastName}".Trim(),
                            Email = upMgr.EmailAddress,
                            Reason = $"Manager of Project Owner for '{project.ProjectName}'"
                        });
                        break;
                    }
                    upMgrId = upMgr?.ManagerId;
                }
            }

            return candidates;
        }
    }
}
