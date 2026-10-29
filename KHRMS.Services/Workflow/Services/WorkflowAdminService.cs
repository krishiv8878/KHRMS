using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using Serilog;

namespace KHRMS.Services.Workflow.Services
{
    public class WorkflowAdminService : IWorkflowAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApproverResolver _resolver;
        private readonly IConditionEvaluator _conditionEvaluator;

        private static readonly HashSet<string> ValidRuleTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "ReportingManager",
            "SkipLevelManager",
            "DepartmentHead",
            "RoleInLocation",
            "ProjectOwner",
            "SpecificPerson",
            "GroupQueue"
        };

        public WorkflowAdminService(
            IUnitOfWork unitOfWork,
            IApproverResolver resolver,
            IConditionEvaluator conditionEvaluator)
        {
            _unitOfWork = unitOfWork;
            _resolver = resolver;
            _conditionEvaluator = conditionEvaluator;
        }

        public async Task<List<WorkflowDefinitionSummaryDTO>> GetDefinitionSummariesAsync()
        {
            var definitions = (await _unitOfWork.WorkflowDefinitions.GetAll())
                .Where(d => d.IsActive && !d.IsDeleted)
                .ToList();

            var steps = (await _unitOfWork.WorkflowSteps.GetAll())
                .Where(s => s.IsActive && !s.IsDeleted)
                .ToList();

            var rules = (await _unitOfWork.WorkflowRoutingRules.GetAll())
                .Where(r => r.IsActive && !r.IsDeleted)
                .ToList();

            // Ensure known core modules are present
            var knownTypes = new[] { "Leave", "AttendanceRegularization", "Timesheet", "Expense" };
            var summaries = new List<WorkflowDefinitionSummaryDTO>();

            var grouped = definitions.GroupBy(d => d.RequestType, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var group in grouped)
            {
                var activeDef = group.FirstOrDefault(d => d.IsPublished) ?? group.OrderByDescending(d => d.Version).First();
                var stepCount = steps.Count(s => s.WorkflowDefinitionId == activeDef.Id);
                var ruleCount = rules.Count(r => r.WorkflowDefinitionId == activeDef.Id);

                DateTime? modDate = activeDef.UpdatedDate != default ? activeDef.UpdatedDate : activeDef.CreatedDate;

                summaries.Add(new WorkflowDefinitionSummaryDTO
                {
                    Id = activeDef.Id,
                    RequestType = activeDef.RequestType,
                    Version = activeDef.Version,
                    Title = activeDef.Title,
                    Description = activeDef.Description,
                    IsPublished = activeDef.IsPublished,
                    EffectiveDate = activeDef.EffectiveDate,
                    StepCount = stepCount,
                    RoutingRuleCount = ruleCount,
                    LastModifiedDate = modDate ?? DateTime.UtcNow,
                    LastModifiedBy = "HR Administrator"
                });
            }

            // Fill in missing known modules with a draft placeholder summary
            foreach (var kt in knownTypes)
            {
                if (!summaries.Any(s => string.Equals(s.RequestType, kt, StringComparison.OrdinalIgnoreCase)))
                {
                    summaries.Add(new WorkflowDefinitionSummaryDTO
                    {
                        Id = 0,
                        RequestType = kt,
                        Version = 1,
                        Title = $"{kt} Standard Approval",
                        Description = $"Configurable approval workflow for {kt} requests",
                        IsPublished = false,
                        EffectiveDate = DateTime.UtcNow,
                        StepCount = 1,
                        RoutingRuleCount = 1,
                        LastModifiedDate = DateTime.UtcNow,
                        LastModifiedBy = "System"
                    });
                }
            }

            return summaries.OrderBy(s => s.RequestType).ToList();
        }

        public async Task<WorkflowDefinitionDetailDTO?> GetDefinitionDetailAsync(string requestType, int? version = null)
        {
            if (string.IsNullOrWhiteSpace(requestType)) return null;

            var allDefs = (await _unitOfWork.WorkflowDefinitions.GetAll())
                .Where(d => string.Equals(d.RequestType, requestType, StringComparison.OrdinalIgnoreCase) && d.IsActive && !d.IsDeleted)
                .ToList();

            WorkflowDefinition? def = null;
            if (version.HasValue)
            {
                def = allDefs.FirstOrDefault(d => d.Version == version.Value);
            }
            else
            {
                def = allDefs.FirstOrDefault(d => d.IsPublished) ?? allDefs.OrderByDescending(d => d.Version).FirstOrDefault();
            }

            if (def == null)
            {
                // Fallback: provide default single-step template
                return new WorkflowDefinitionDetailDTO
                {
                    Definition = new WorkflowDefinitionSummaryDTO
                    {
                        Id = 0,
                        RequestType = requestType,
                        Version = 1,
                        Title = $"{requestType} Approval Workflow",
                        Description = $"Standard multi-step approval workflow for {requestType}",
                        IsPublished = false,
                        EffectiveDate = DateTime.UtcNow,
                        StepCount = 1,
                        RoutingRuleCount = 1
                    },
                    Steps = new List<WorkflowStepConfigDTO>
                    {
                        new()
                        {
                            StepOrder = 1,
                            StepName = "Direct Manager Review",
                            ApproverRuleType = "ReportingManager",
                            StepMode = "AnyOne",
                            TimeLimitHours = 48,
                            IsEscalationEnabled = true,
                            EscalationTarget = "SkipLevelManager",
                            EscalationAction = "Reassign"
                        }
                    },
                    RoutingRules = new List<WorkflowRoutingRuleConfigDTO>
                    {
                        new()
                        {
                            ScopeType = "Default",
                            ScopeValue = null,
                            ApproverRule = "ReportingManager",
                            Priority = 999,
                            IsDefault = true
                        }
                    }
                };
            }

            var allSteps = (await _unitOfWork.WorkflowSteps.GetAll())
                .Where(s => s.WorkflowDefinitionId == def.Id && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.StepOrder)
                .Select(s => new WorkflowStepConfigDTO
                {
                    Id = s.Id,
                    StepOrder = s.StepOrder,
                    StepName = s.StepName,
                    ApproverRuleType = s.ApproverRuleType,
                    RuleParameters = s.RuleParameters,
                    StepMode = s.StepMode,
                    ConditionJson = s.ConditionJson,
                    TimeLimitHours = s.TimeLimitHours,
                    IsEscalationEnabled = s.IsEscalationEnabled,
                    EscalationTarget = s.EscalationTarget,
                    EscalationAction = s.EscalationAction
                })
                .ToList();

            var allRules = (await _unitOfWork.WorkflowRoutingRules.GetAll())
                .Where(r => r.WorkflowDefinitionId == def.Id && r.IsActive && !r.IsDeleted)
                .OrderBy(r => r.Priority)
                .Select(r => new WorkflowRoutingRuleConfigDTO
                {
                    Id = r.Id,
                    ScopeType = r.ScopeType,
                    ScopeValue = r.ScopeValue,
                    ApproverRule = r.ApproverRule,
                    Priority = r.Priority,
                    IsDefault = r.IsDefault
                })
                .ToList();

            DateTime? lastMod = def.UpdatedDate != default ? def.UpdatedDate : def.CreatedDate;

            return new WorkflowDefinitionDetailDTO
            {
                Definition = new WorkflowDefinitionSummaryDTO
                {
                    Id = def.Id,
                    RequestType = def.RequestType,
                    Version = def.Version,
                    Title = def.Title,
                    Description = def.Description,
                    IsPublished = def.IsPublished,
                    EffectiveDate = def.EffectiveDate,
                    StepCount = allSteps.Count,
                    RoutingRuleCount = allRules.Count,
                    LastModifiedDate = lastMod ?? DateTime.UtcNow,
                    LastModifiedBy = "HR Administrator"
                },
                Steps = allSteps,
                RoutingRules = allRules
            };
        }

        public async Task<WorkflowValidationResultDTO> ValidateDefinitionAsync(WorkflowDefinitionSaveDTO dto)
        {
            var result = new WorkflowValidationResultDTO { IsValid = true };

            if (string.IsNullOrWhiteSpace(dto.RequestType))
            {
                result.Errors.Add("Request type is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                result.Errors.Add("Workflow definition title is required.");
            }

            if (dto.Steps == null || dto.Steps.Count == 0)
            {
                result.Errors.Add("Workflow must define at least one approval step.");
            }
            else
            {
                var orders = new HashSet<int>();
                for (int i = 0; i < dto.Steps.Count; i++)
                {
                    var s = dto.Steps[i];
                    if (string.IsNullOrWhiteSpace(s.StepName))
                    {
                        result.Errors.Add($"Step #{i + 1}: Step name is required.");
                    }

                    if (string.IsNullOrWhiteSpace(s.ApproverRuleType) || !ValidRuleTypes.Contains(s.ApproverRuleType))
                    {
                        result.Errors.Add($"Step '{s.StepName}': Invalid or unsupported approver rule type '{s.ApproverRuleType}'. Valid rules: {string.Join(", ", ValidRuleTypes)}.");
                    }

                    if (orders.Contains(s.StepOrder))
                    {
                        result.Errors.Add($"Step order '{s.StepOrder}' is duplicated. Step orders must be strictly sequential.");
                    }
                    orders.Add(s.StepOrder);

                    // Rule parameters validation
                    if (string.Equals(s.ApproverRuleType, "SpecificPerson", StringComparison.OrdinalIgnoreCase))
                    {
                        long empId = 0;
                        if (!string.IsNullOrWhiteSpace(s.RuleParameters))
                        {
                            try
                            {
                                using var doc = JsonDocument.Parse(s.RuleParameters);
                                if (doc.RootElement.TryGetProperty("EmployeeId", out var p) || doc.RootElement.TryGetProperty("PersonId", out p))
                                {
                                    empId = p.GetInt64();
                                }
                            }
                            catch
                            {
                                if (long.TryParse(s.RuleParameters, out var directId)) empId = directId;
                            }
                        }

                        if (empId <= 0)
                        {
                            result.Errors.Add($"Step '{s.StepName}': Rule 'SpecificPerson' requires a valid numeric EmployeeId in RuleParameters.");
                        }
                    }

                    if (string.Equals(s.ApproverRuleType, "RoleInLocation", StringComparison.OrdinalIgnoreCase))
                    {
                        int roleId = 0;
                        if (!string.IsNullOrWhiteSpace(s.RuleParameters))
                        {
                            try
                            {
                                using var doc = JsonDocument.Parse(s.RuleParameters);
                                if (doc.RootElement.TryGetProperty("RoleId", out var r)) roleId = r.GetInt32();
                            }
                            catch { }
                        }
                        if (roleId <= 0)
                        {
                            result.Errors.Add($"Step '{s.StepName}': Rule 'RoleInLocation' requires a valid 'RoleId' parameter.");
                        }
                    }

                    // Condition JSON syntax validation
                    if (!string.IsNullOrWhiteSpace(s.ConditionJson))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(s.ConditionJson);
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add($"Step '{s.StepName}': Invalid condition JSON expression: {ex.Message}");
                        }
                    }

                    // Time limit & escalation target validation
                    if (s.IsEscalationEnabled && string.IsNullOrWhiteSpace(s.EscalationTarget))
                    {
                        result.Warnings.Add($"Step '{s.StepName}': Auto-escalation is enabled but no escalation target is specified.");
                    }
                }
            }

            // Routing rules validation:
            // 1. Every type has a default route
            if (dto.RoutingRules == null || dto.RoutingRules.Count == 0 ||
                !dto.RoutingRules.Any(r => r.IsDefault || string.Equals(r.ScopeType, "Default", StringComparison.OrdinalIgnoreCase)))
            {
                result.Errors.Add("Every request type must define a 'Default' routing rule as a fallback.");
            }

            // 2. Warn on overlapping routing rules
            if (dto.RoutingRules != null)
            {
                var seenScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var r in dto.RoutingRules)
                {
                    string key = $"{r.ScopeType}:{r.ScopeValue ?? ""}";
                    if (seenScopes.Contains(key))
                    {
                        result.Warnings.Add($"Overlapping routing rule detected for scope '{r.ScopeType}' with value '{r.ScopeValue}'. Lowest priority number will win.");
                    }
                    seenScopes.Add(key);
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        public async Task<WorkflowDefinitionDetailDTO> SaveAndPublishDefinitionAsync(WorkflowDefinitionSaveDTO dto, long currentUserId)
        {
            var validation = await ValidateDefinitionAsync(dto);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Workflow validation failed: {string.Join(" | ", validation.Errors)}");
            }

            // Find all existing definitions for this request type
            var existingDefs = (await _unitOfWork.WorkflowDefinitions.GetAll())
                .Where(d => string.Equals(d.RequestType, dto.RequestType, StringComparison.OrdinalIgnoreCase) && d.IsActive && !d.IsDeleted)
                .ToList();

            // Next version increment: "Editing creates a new version. In-flight requests keep the version they started with."
            int nextVersion = (existingDefs.Any() ? existingDefs.Max(d => d.Version) : 0) + 1;

            // Deactivate previous active version's published flag
            foreach (var oldDef in existingDefs)
            {
                if (oldDef.IsPublished)
                {
                    oldDef.IsPublished = false;
                    oldDef.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.WorkflowDefinitions.Update(oldDef);
                }
            }

            // Create new published definition
            var newDef = new WorkflowDefinition
            {
                RequestType = dto.RequestType,
                Version = nextVersion,
                Title = dto.Title,
                Description = dto.Description,
                IsPublished = true,
                EffectiveDate = dto.EffectiveDate ?? DateTime.UtcNow,
                IsDefault = true,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _unitOfWork.WorkflowDefinitions.Add(newDef);
            _unitOfWork.Save();

            // Add Steps
            int order = 1;
            foreach (var stepDto in dto.Steps.OrderBy(s => s.StepOrder))
            {
                var newStep = new WorkflowStep
                {
                    WorkflowDefinitionId = newDef.Id,
                    StepOrder = order++,
                    StepName = stepDto.StepName,
                    ApproverRuleType = stepDto.ApproverRuleType,
                    RuleParameters = stepDto.RuleParameters,
                    StepMode = stepDto.StepMode ?? "AnyOne",
                    ConditionJson = stepDto.ConditionJson,
                    TimeLimitHours = stepDto.TimeLimitHours,
                    IsEscalationEnabled = stepDto.IsEscalationEnabled,
                    EscalationTarget = stepDto.EscalationTarget,
                    EscalationAction = stepDto.EscalationAction ?? "Reassign",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                await _unitOfWork.WorkflowSteps.Add(newStep);
            }

            // Add Routing Rules
            foreach (var ruleDto in dto.RoutingRules)
            {
                var newRule = new WorkflowRoutingRule
                {
                    WorkflowDefinitionId = newDef.Id,
                    RequestType = dto.RequestType,
                    ScopeType = ruleDto.ScopeType,
                    ScopeValue = ruleDto.ScopeValue,
                    ApproverRule = ruleDto.ApproverRule,
                    Priority = ruleDto.Priority,
                    IsDefault = ruleDto.IsDefault || string.Equals(ruleDto.ScopeType, "Default", StringComparison.OrdinalIgnoreCase),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };
                await _unitOfWork.WorkflowRoutingRules.Add(newRule);
            }

            _unitOfWork.Save();

            // Record audit log
            try
            {
                await _unitOfWork.WorkflowAuditLogs.Add(new WorkflowAuditLog
                {
                    WorkflowInstanceId = 0,
                    EventType = "PublishDefinition",
                    PerformedByEmployeeId = currentUserId,
                    ActionCapacity = "Admin",
                    Details = $"Published new version {nextVersion} for '{dto.RequestType}' with {dto.Steps.Count} steps and {dto.RoutingRules.Count} routing rules.",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch { }

            return (await GetDefinitionDetailAsync(dto.RequestType, nextVersion))!;
        }

        public async Task<TestRoutingResponseDTO> TestRoutingAsync(TestRoutingRequestDTO dto)
        {
            var emp = await _unitOfWork.Employees.GetById(dto.EmployeeId);
            if (emp == null)
            {
                return new TestRoutingResponseDTO
                {
                    Success = false,
                    Message = $"Requester employee #{dto.EmployeeId} does not exist."
                };
            }

            var employees = (await _unitOfWork.Employees.GetAll())
                .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            string requesterName = employees.TryGetValue(dto.EmployeeId, out var name) ? name : $"Employee #{dto.EmployeeId}";

            string? dept = dto.Department;
            if (string.IsNullOrWhiteSpace(dept) && emp.DesignationId.HasValue)
            {
                var desig = await _unitOfWork.Designations.GetById(emp.DesignationId.Value);
                dept = desig?.DepartmentCategory;
            }

            var context = new WorkflowContext
            {
                RequesterId = dto.EmployeeId,
                RequestType = dto.RequestType,
                Department = dept,
                Location = dto.Location ?? emp.Branch,
                ProjectId = dto.ProjectId,
                Attributes = dto.SampleContext != null
                    ? dto.SampleContext.ToDictionary(k => k.Key, v => (object?)v.Value)
                    : new Dictionary<string, object?>()
            };

            try
            {
                var def = await _resolver.ResolveDefinitionAsync(dto.RequestType, context);
                var resolvedSteps = await _resolver.ResolveStepsAsync(dto.RequestType, dto.EmployeeId, context, def);

                var stepResults = new List<TestRoutingStepResultDTO>();
                foreach (var step in resolvedSteps)
                {
                    var approverIds = step.Approvers.Select(a => a.EmployeeId).ToList();
                    var approverNames = step.Approvers.Select(a => !string.IsNullOrEmpty(a.EmployeeName) ? a.EmployeeName : $"Emp #{a.EmployeeId}").ToList();

                    string condSummary = step.ConditionSatisfied
                        ? "Condition met"
                        : "Condition evaluated to false (Step skipped)";

                    stepResults.Add(new TestRoutingStepResultDTO
                    {
                        StepOrder = step.StepOrder,
                        StepName = step.StepName,
                        ApproverRuleType = step.ApproverRuleType,
                        ConditionMet = step.ConditionSatisfied,
                        ConditionSummary = condSummary,
                        EligibleApproverIds = approverIds,
                        EligibleApproverNames = approverNames,
                        ResolutionReason = step.ResolutionReason,
                        Mode = step.StepMode,
                        TimeLimitHours = step.TimeLimitHours,
                        EscalationTarget = step.EscalationTarget
                    });
                }

                return new TestRoutingResponseDTO
                {
                    Success = true,
                    RequesterName = requesterName,
                    MatchedRouteScope = $"Definition v{def.Version} ('{def.Title}')",
                    Message = $"Successfully resolved {stepResults.Count} step(s) for {requesterName}.",
                    Steps = stepResults
                };
            }
            catch (WorkflowResolutionException ex)
            {
                return new TestRoutingResponseDTO
                {
                    Success = false,
                    RequesterName = requesterName,
                    Message = $"Routing Resolution Failed: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error testing routing for {RequestType} and employee {EmpId}", dto.RequestType, dto.EmployeeId);
                return new TestRoutingResponseDTO
                {
                    Success = false,
                    RequesterName = requesterName,
                    Message = $"Error during simulation: {ex.Message}"
                };
            }
        }

        public async Task<List<WorkflowVersionHistoryDTO>> GetVersionHistoryAsync(string requestType)
        {
            var definitions = (await _unitOfWork.WorkflowDefinitions.GetAll())
                .Where(d => string.Equals(d.RequestType, requestType, StringComparison.OrdinalIgnoreCase) && d.IsActive && !d.IsDeleted)
                .OrderByDescending(d => d.Version)
                .ToList();

            var steps = (await _unitOfWork.WorkflowSteps.GetAll())
                .Where(s => s.IsActive && !s.IsDeleted)
                .ToList();

            var result = new List<WorkflowVersionHistoryDTO>();
            foreach (var d in definitions)
            {
                int count = steps.Count(s => s.WorkflowDefinitionId == d.Id);
                result.Add(new WorkflowVersionHistoryDTO
                {
                    Version = d.Version,
                    Title = d.Title,
                    IsPublished = d.IsPublished,
                    EffectiveDate = d.EffectiveDate,
                    CreatedDate = d.CreatedDate ?? DateTime.UtcNow,
                    CreatedBy = "HR Administrator",
                    StepCount = count
                });
            }

            return result;
        }

        public async Task<bool> RollbackVersionAsync(string requestType, int targetVersion, long currentUserId)
        {
            var defs = (await _unitOfWork.WorkflowDefinitions.GetAll())
                .Where(d => string.Equals(d.RequestType, requestType, StringComparison.OrdinalIgnoreCase) && d.IsActive && !d.IsDeleted)
                .ToList();

            var target = defs.FirstOrDefault(d => d.Version == targetVersion);
            if (target == null) return false;

            foreach (var d in defs)
            {
                d.IsPublished = (d.Version == targetVersion);
                d.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.WorkflowDefinitions.Update(d);
            }

            _unitOfWork.Save();

            try
            {
                await _unitOfWork.WorkflowAuditLogs.Add(new WorkflowAuditLog
                {
                    WorkflowInstanceId = 0,
                    EventType = "RollbackDefinition",
                    PerformedByEmployeeId = currentUserId,
                    ActionCapacity = "Admin",
                    Details = $"Rolled back '{requestType}' active workflow definition to version {targetVersion}.",
                    Timestamp = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch { }

            return true;
        }
    }
}
