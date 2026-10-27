using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using Serilog;

namespace KHRMS.Services.Workflow.Services
{
    public class ApproverResolver : IApproverResolver
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnumerable<IApproverRule> _rules;
        private readonly IConditionEvaluator _conditionEvaluator;

        public ApproverResolver(
            IUnitOfWork unitOfWork,
            IEnumerable<IApproverRule> rules,
            IConditionEvaluator conditionEvaluator)
        {
            _unitOfWork = unitOfWork;
            _rules = rules;
            _conditionEvaluator = conditionEvaluator;
        }

        public async Task<WorkflowDefinition> ResolveDefinitionAsync(string requestType, WorkflowContext context)
        {
            var routingRules = (await _unitOfWork.WorkflowRoutingRules.GetAll())
                .Where(r => string.Equals(r.RequestType, requestType, StringComparison.OrdinalIgnoreCase) && r.IsActive && !r.IsDeleted)
                .OrderBy(r => r.Priority) // Lowest number wins
                .ToList();

            WorkflowRoutingRule? selectedRule = null;

            // 1. Check Project scope if context has ProjectId
            if (context.ProjectId.HasValue)
            {
                selectedRule = routingRules.FirstOrDefault(r =>
                    string.Equals(r.ScopeType, "Project", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.ScopeValue, context.ProjectId.Value.ToString(), StringComparison.OrdinalIgnoreCase));
            }

            // 2. Check Department scope
            if (selectedRule == null && !string.IsNullOrWhiteSpace(context.Department))
            {
                selectedRule = routingRules.FirstOrDefault(r =>
                    string.Equals(r.ScopeType, "Department", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.ScopeValue, context.Department, StringComparison.OrdinalIgnoreCase));
            }

            // 3. Check Location scope
            if (selectedRule == null && !string.IsNullOrWhiteSpace(context.Location))
            {
                selectedRule = routingRules.FirstOrDefault(r =>
                    string.Equals(r.ScopeType, "Location", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.ScopeValue, context.Location, StringComparison.OrdinalIgnoreCase));
            }

            // 4. Fallback to Default routing rule
            if (selectedRule == null)
            {
                selectedRule = routingRules.FirstOrDefault(r =>
                    r.IsDefault || string.Equals(r.ScopeType, "Default", StringComparison.OrdinalIgnoreCase));
            }

            // 5. If no routing rule found, pick the active published definition for this request type
            long definitionId = selectedRule?.WorkflowDefinitionId ?? 0;
            WorkflowDefinition? definition = null;

            if (definitionId > 0)
            {
                definition = await _unitOfWork.WorkflowDefinitions.GetById(definitionId);
            }

            if (definition == null || !definition.IsActive || definition.IsDeleted)
            {
                var definitions = (await _unitOfWork.WorkflowDefinitions.GetAll())
                    .Where(d => string.Equals(d.RequestType, requestType, StringComparison.OrdinalIgnoreCase) && d.IsActive && !d.IsDeleted)
                    .OrderByDescending(d => d.IsPublished)
                    .ThenByDescending(d => d.Version)
                    .ToList();

                definition = definitions.FirstOrDefault(d => d.IsDefault) ?? definitions.FirstOrDefault();
            }

            if (definition == null)
            {
                definition = await EnsureDefaultDefinitionAsync(requestType);
            }

            // Load associated steps
            var allSteps = (await _unitOfWork.WorkflowSteps.GetAll())
                .Where(s => s.WorkflowDefinitionId == definition.Id && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.StepOrder)
                .ToList();

            if (allSteps.Count > 0)
            {
                definition.Steps = allSteps;
            }

            return definition;
        }

        private async Task<WorkflowDefinition> EnsureDefaultDefinitionAsync(string requestType)
        {
            string normalizedType = requestType;
            string title = $"{requestType} Approval Workflow";
            string desc = $"Default single-step direct manager approval workflow for {requestType}.";

            if (string.Equals(requestType, "Leave", StringComparison.OrdinalIgnoreCase))
            {
                normalizedType = "Leave";
                title = "Standard Leave Approval Workflow";
                desc = "Single-step direct manager approval workflow reproducing legacy reporting line rules.";
            }
            else if (string.Equals(requestType, "AttendanceRegularization", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(requestType, "Attendance", StringComparison.OrdinalIgnoreCase))
            {
                normalizedType = "AttendanceRegularization";
                title = "Standard Attendance Regularization Approval Workflow";
                desc = "Single-step direct manager approval workflow for attendance clock/regularization adjustments.";
            }
            else if (string.Equals(requestType, "Timesheet", StringComparison.OrdinalIgnoreCase))
            {
                normalizedType = "Timesheet";
                title = "Standard Timesheet Approval Workflow";
                desc = "Single-step direct manager approval workflow for weekly and monthly timesheet reviews.";
            }
            else if (string.Equals(requestType, "Expense", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(requestType, "ExpenseReport", StringComparison.OrdinalIgnoreCase))
            {
                normalizedType = "Expense";
                title = "Standard Expense Report Approval Workflow";
                desc = "Single-step direct manager approval workflow for employee expense reimbursement claims.";
            }

            var def = new WorkflowDefinition
            {
                RequestType = normalizedType,
                Title = title,
                Description = desc,
                Version = 1,
                IsPublished = true,
                IsDefault = true,
                EffectiveDate = DateTime.UtcNow.AddDays(-1),
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _unitOfWork.WorkflowDefinitions.Add(def);
            _unitOfWork.Save();
            if (def.Id == 0) def.Id = 1;

            var step = new WorkflowStep
            {
                WorkflowDefinitionId = def.Id,
                StepOrder = 1,
                StepName = "Manager Approval",
                ApproverRuleType = "ReportingManager",
                RuleParameters = "{}",
                StepMode = "AnyOne",
                IsEscalationEnabled = false,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _unitOfWork.WorkflowSteps.Add(step);

            var routingRule = new WorkflowRoutingRule
            {
                RequestType = normalizedType,
                ScopeType = "Default",
                Priority = 999,
                IsDefault = true,
                WorkflowDefinitionId = def.Id,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _unitOfWork.WorkflowRoutingRules.Add(routingRule);
            _unitOfWork.Save();

            def.Steps = new List<WorkflowStep> { step };
            return def;
        }

        public async Task<List<ResolvedStepApprovers>> ResolveStepsAsync(
            string requestType,
            long requesterId,
            WorkflowContext context,
            WorkflowDefinition? definition = null)
        {
            definition ??= await ResolveDefinitionAsync(requestType, context);

            var steps = definition.Steps.OrderBy(s => s.StepOrder).ToList();
            if (steps.Count == 0)
            {
                throw new WorkflowResolutionException($"Workflow definition '{definition.Title}' (v{definition.Version}) has no configured steps.");
            }

            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => e.IsActive && !e.IsDeleted).ToList();
            var allDelegations = (await _unitOfWork.WorkflowDelegations.GetAll()).Where(d => d.IsActive && !d.IsDeleted).ToList();
            var now = DateTime.UtcNow;

            var resolvedSteps = new List<ResolvedStepApprovers>();

            foreach (var step in steps)
            {
                var stepResult = new ResolvedStepApprovers
                {
                    StepOrder = step.StepOrder,
                    StepName = step.StepName,
                    StepMode = step.StepMode,
                    ApproverRuleType = step.ApproverRuleType,
                    TimeLimitHours = step.TimeLimitHours,
                    IsEscalationEnabled = step.IsEscalationEnabled,
                    EscalationTarget = step.EscalationTarget,
                    EscalationAction = step.EscalationAction
                };

                // 1. Condition evaluation
                bool satisfiesCondition = _conditionEvaluator.Evaluate(step.ConditionJson, context);
                stepResult.ConditionSatisfied = satisfiesCondition;
                if (!satisfiesCondition)
                {
                    stepResult.ResolutionReason = $"Step skipped because condition '{step.ConditionJson}' was not met.";
                    resolvedSteps.Add(stepResult);
                    continue;
                }

                // 2. Rule lookup & execution
                var rule = _rules.FirstOrDefault(r => string.Equals(r.RuleType, step.ApproverRuleType, StringComparison.OrdinalIgnoreCase));
                if (rule == null)
                {
                    throw new WorkflowResolutionException($"Approver rule handler '{step.ApproverRuleType}' not found.");
                }

                var candidates = await rule.ResolveCandidatesAsync(requesterId, context, step.RuleParameters);

                // 3. Filter candidates: remove inactive employees and prevent requester from approving own request
                var validApprovers = new List<ResolvedApproverItem>();
                var seenEmployeeIds = new HashSet<long>();

                foreach (var c in candidates)
                {
                    if (c.EmployeeId == requesterId) continue; // Never approve own request

                    var emp = allEmployees.FirstOrDefault(e => e.Id == c.EmployeeId);
                    if (emp == null) continue; // Skip inactive/deleted

                    if (seenEmployeeIds.Add(emp.Id))
                    {
                        validApprovers.Add(new ResolvedApproverItem
                        {
                            EmployeeId = emp.Id,
                            EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                            Email = emp.EmailAddress,
                            Capacity = "Approver",
                            ResolutionReason = c.Reason
                        });
                    }
                }

                // 4. Apply active delegations
                var candidateIds = validApprovers.Select(a => a.EmployeeId).ToList();
                var activeDelegations = allDelegations
                    .Where(d => candidateIds.Contains(d.FromEmployeeId) &&
                                d.StartDate <= now && d.EndDate >= now &&
                                (string.IsNullOrEmpty(d.RequestType) ||
                                 string.Equals(d.RequestType, "All", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(d.RequestType, requestType, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                foreach (var del in activeDelegations)
                {
                    if (del.ToEmployeeId == requesterId) continue; // Delegate cannot be the requester

                    var delegateEmp = allEmployees.FirstOrDefault(e => e.Id == del.ToEmployeeId);
                    if (delegateEmp != null && seenEmployeeIds.Add(delegateEmp.Id))
                    {
                        var principal = validApprovers.FirstOrDefault(a => a.EmployeeId == del.FromEmployeeId);
                        string principalName = principal?.EmployeeName ?? "Principal";

                        validApprovers.Add(new ResolvedApproverItem
                        {
                            EmployeeId = delegateEmp.Id,
                            EmployeeName = $"{delegateEmp.FirstName} {delegateEmp.LastName}".Trim(),
                            Email = delegateEmp.EmailAddress,
                            Capacity = "Delegate",
                            DelegatedFromEmployeeId = del.FromEmployeeId,
                            DelegatedFromEmployeeName = principalName,
                            ResolutionReason = $"Active delegation from {principalName} ({del.StartDate:yyyy-MM-dd} to {del.EndDate:yyyy-MM-dd})"
                        });
                    }
                }

                // 5. Fallback chain if step resolved to nobody
                if (validApprovers.Count == 0)
                {
                    Log.Warning("Step {StepOrder} ({StepName}) resolved 0 approvers for requester {RequesterId}. Engaging fallback chain.",
                        step.StepOrder, step.StepName, requesterId);

                    // Fallback 1: Try Requester's direct manager (if rule was not already ReportingManager)
                    var requester = allEmployees.FirstOrDefault(e => e.Id == requesterId);
                    if (requester?.ManagerId.HasValue == true && requester.ManagerId.Value > 0 && requester.ManagerId.Value != requesterId)
                    {
                        var mgr = allEmployees.FirstOrDefault(e => e.Id == requester.ManagerId.Value);
                        if (mgr != null && seenEmployeeIds.Add(mgr.Id))
                        {
                            validApprovers.Add(new ResolvedApproverItem
                            {
                                EmployeeId = mgr.Id,
                                EmployeeName = $"{mgr.FirstName} {mgr.LastName}".Trim(),
                                Email = mgr.EmailAddress,
                                Capacity = "Approver",
                                ResolutionReason = "Fallback Chain: Requester Direct Manager"
                            });
                        }
                    }

                    // Fallback 2: Organization HR / Admin queue
                    if (validApprovers.Count == 0)
                    {
                        var roles = (await _unitOfWork.RoleMaster.GetAll()).Where(r => r.IsActive && !r.IsDeleted).ToList();
                        var hrAdminRoles = roles.Where(r =>
                            string.Equals(r.RoleName, "HR", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(r.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(r.RoleName, "HR Operations", StringComparison.OrdinalIgnoreCase))
                            .Select(r => r.Id)
                            .ToHashSet();

                        var roleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                            .Where(m => hrAdminRoles.Contains(m.RoleId) && m.IsActive == true && m.IsDeleted != true)
                            .Select(m => m.EmployeeId)
                            .ToHashSet();

                        var fallbackAdmins = allEmployees.Where(e => e.Id != requesterId && roleMappings.Contains(e.Id)).ToList();
                        foreach (var admin in fallbackAdmins)
                        {
                            if (seenEmployeeIds.Add(admin.Id))
                            {
                                validApprovers.Add(new ResolvedApproverItem
                                {
                                    EmployeeId = admin.Id,
                                    EmployeeName = $"{admin.FirstName} {admin.LastName}".Trim(),
                                    Email = admin.EmailAddress,
                                    Capacity = "Approver",
                                    ResolutionReason = "Fallback Chain: HR / Admin Organization Queue"
                                });
                            }
                        }
                    }
                }

                // If STILL nobody, fail clearly at submit time! Never create a stuck request!
                if (validApprovers.Count == 0)
                {
                    throw new WorkflowResolutionException(
                        $"Step '{step.StepName}' (Order: {step.StepOrder}) in workflow '{definition.Title}' could not resolve any eligible approvers for employee #{requesterId} and fallback chain was exhausted. Please verify reporting managers or department assignments before submitting.");
                }

                stepResult.Approvers = validApprovers;
                stepResult.ResolutionReason = string.Join("; ", validApprovers.Select(a => $"{a.EmployeeName} ({a.Capacity}: {a.ResolutionReason})"));
                resolvedSteps.Add(stepResult);
            }

            return resolvedSteps;
        }
    }
}
