using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace KHRMS.Services.Workflow.Services
{
    public class WorkflowEngineService : IWorkflowEngineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApproverResolver _approverResolver;
        private readonly IPermissionService _permissionService;
        private readonly IConditionEvaluator _conditionEvaluator;

        public WorkflowEngineService(
            IUnitOfWork unitOfWork,
            IApproverResolver approverResolver,
            IPermissionService permissionService,
            IConditionEvaluator conditionEvaluator)
        {
            _unitOfWork = unitOfWork;
            _approverResolver = approverResolver;
            _permissionService = permissionService;
            _conditionEvaluator = conditionEvaluator;
        }

        public async Task<WorkflowInstance> StartWorkflowAsync(
            string requestType,
            long requestId,
            long requesterId,
            WorkflowContext context)
        {
            context.RequesterId = requesterId;
            context.RequestType = requestType;

            // 1. Resolve definition and steps
            var definition = await _approverResolver.ResolveDefinitionAsync(requestType, context);
            var resolvedSteps = await _approverResolver.ResolveStepsAsync(requestType, requesterId, context, definition);

            if (resolvedSteps.Count == 0)
            {
                throw new WorkflowResolutionException($"Cannot start workflow: definition '{definition.Title}' has no valid steps.");
            }

            // Find first active step that satisfies condition
            var firstActive = resolvedSteps.FirstOrDefault(s => s.ConditionSatisfied);
            if (firstActive == null)
            {
                throw new WorkflowResolutionException($"Cannot start workflow: all steps for '{definition.Title}' were skipped by conditions.");
            }

            // 2. Create Workflow Instance
            var instance = new WorkflowInstance
            {
                RequestType = requestType,
                RequestId = requestId,
                RequesterEmployeeId = requesterId,
                WorkflowDefinitionId = definition.Id,
                DefinitionVersion = definition.Version,
                CurrentStepOrder = firstActive.StepOrder,
                Status = "Pending",
                StartedDate = DateTime.UtcNow,
                CreatedBy = (int)requesterId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = (int)requesterId,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.WorkflowInstances.Add(instance);
            _unitOfWork.Save();

            // 3. Create all instance steps
            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            foreach (var rStep in resolvedSteps)
            {
                bool isStartingStep = rStep.StepOrder == firstActive.StepOrder;
                string stepStatus = !rStep.ConditionSatisfied ? "Skipped" : (isStartingStep ? "Pending" : "Waiting");

                var approverIds = rStep.Approvers.Select(a => a.EmployeeId).Distinct().ToList();
                DateTime? dueTime = null;
                if (isStartingStep && rStep.TimeLimitHours.HasValue && rStep.IsEscalationEnabled)
                {
                    dueTime = DateTime.UtcNow.AddHours(rStep.TimeLimitHours.Value);
                }

                var instanceStep = new WorkflowInstanceStep
                {
                    WorkflowInstanceId = instance.Id,
                    StepOrder = rStep.StepOrder,
                    StepName = rStep.StepName,
                    Status = stepStatus,
                    ApproverRuleType = rStep.ApproverRuleType,
                    ResolutionReason = rStep.ResolutionReason,
                    EligibleApproverIdsJson = JsonSerializer.Serialize(approverIds),
                    StepMode = rStep.StepMode,
                    StepStartTime = DateTime.UtcNow,
                    DueTime = dueTime,
                    CreatedBy = (int)requesterId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = (int)requesterId,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.WorkflowInstanceSteps.Add(instanceStep);
            }
            _unitOfWork.Save();

            // 4. Audit Log
            await _unitOfWork.WorkflowAuditLogs.Add(new WorkflowAuditLog
            {
                WorkflowInstanceId = instance.Id,
                EventType = "WorkflowStarted",
                PerformedByEmployeeId = requesterId,
                ActionCapacity = "Requester",
                Details = $"Workflow started for {requestType} #{requestId} under definition '{definition.Title}' (v{definition.Version}). Step 1 '{firstActive.StepName}' pending with {firstActive.Approvers.Count} approver(s).",
                NewStatus = "Pending",
                Timestamp = DateTime.UtcNow,
                CreatedBy = (int)requesterId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = (int)requesterId,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            });
            _unitOfWork.Save();

            // 5. Notify active step approvers
            string requesterName = allEmployees.TryGetValue(requesterId, out var rName) ? rName : $"Employee #{requesterId}";
            foreach (var app in firstActive.Approvers)
            {
                try
                {
                    await _unitOfWork.Notifications.Add(new Notification
                    {
                        EmployeeId = app.EmployeeId,
                        Title = $"New {requestType} Request Approval Required",
                        Message = $"{requesterName} submitted a {requestType} request requiring your approval at step '{firstActive.StepName}'.",
                        Category = requestType,
                        Type = "request",
                        Icon = "assignment",
                        IconBg = "#eff6ff",
                        IconColor = "#2563eb",
                        Route = "/index/request",
                        QueryParams = $"tab={requestType.ToLowerInvariant()}",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
                catch { }
            }
            _unitOfWork.Save();

            return instance;
        }

        public async Task<WorkflowActionResult> ActOnStepAsync(
            long instanceId,
            long currentUserId,
            WorkflowActionRequest request)
        {
            var instance = await _unitOfWork.WorkflowInstances.GetById(instanceId);
            if (instance == null || !instance.IsActive || instance.IsDeleted)
            {
                return new WorkflowActionResult { Success = false, StatusCode = "NotFound", Message = "Workflow instance not found." };
            }

            if (!string.Equals(instance.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return new WorkflowActionResult
                {
                    Success = false,
                    StatusCode = "AlreadyActioned",
                    Message = $"Workflow instance #{instanceId} is already {instance.Status} and cannot accept further decisions."
                };
            }

            var allSteps = (await _unitOfWork.WorkflowInstanceSteps.GetAll())
                .Where(s => s.WorkflowInstanceId == instanceId && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.StepOrder)
                .ToList();

            var currentStep = allSteps.FirstOrDefault(s => s.StepOrder == instance.CurrentStepOrder && s.Status == "Pending");
            if (currentStep == null)
            {
                return new WorkflowActionResult
                {
                    Success = false,
                    StatusCode = "AlreadyActioned",
                    Message = "Current workflow step is not pending or has already been actioned."
                };
            }

            var actionNorm = request.Action?.Trim() ?? string.Empty;
            bool isOverride = actionNorm.Equals("OverrideApprove", StringComparison.OrdinalIgnoreCase) || actionNorm.Equals("OverrideReject", StringComparison.OrdinalIgnoreCase);
            bool isReassign = actionNorm.Equals("Reassign", StringComparison.OrdinalIgnoreCase);
            bool isStandardApprove = actionNorm.Equals("Approve", StringComparison.OrdinalIgnoreCase);
            bool isStandardReject = actionNorm.Equals("Reject", StringComparison.OrdinalIgnoreCase);

            if (!isOverride && !isReassign && !isStandardApprove && !isStandardReject)
            {
                return new WorkflowActionResult { Success = false, StatusCode = "BadRequest", Message = $"Invalid action '{request.Action}'." };
            }

            // ==============================================================
            // 1. REASSIGN LOGIC
            // ==============================================================
            if (isReassign)
            {
                bool canReassign = await _permissionService.HasPermissionAsync(currentUserId, "REQUEST_REASSIGN");
                if (!canReassign)
                {
                    return new WorkflowActionResult { Success = false, StatusCode = "Forbidden", Message = "Missing required permission 'REQUEST_REASSIGN'." };
                }

                if (!request.ReassignToEmployeeId.HasValue || request.ReassignToEmployeeId.Value <= 0)
                {
                    return new WorkflowActionResult { Success = false, StatusCode = "BadRequest", Message = "A valid target employee must be specified for reassignment." };
                }

                if (request.ReassignToEmployeeId.Value == instance.RequesterEmployeeId)
                {
                    return new WorkflowActionResult { Success = false, StatusCode = "BadRequest", Message = "Cannot reassign approval step to the requester." };
                }

                var targetEmp = await _unitOfWork.Employees.GetById(request.ReassignToEmployeeId.Value);
                if (targetEmp == null || !targetEmp.IsActive || targetEmp.IsDeleted)
                {
                    return new WorkflowActionResult { Success = false, StatusCode = "BadRequest", Message = "Target reassignment employee is inactive or not found." };
                }

                string reassignReason = string.IsNullOrWhiteSpace(request.Reason) ? "Administrative reassignment" : request.Reason;
                currentStep.EligibleApproverIdsJson = JsonSerializer.Serialize(new List<long> { targetEmp.Id });
                currentStep.ResolutionReason += $" [Reassigned to {targetEmp.FirstName} {targetEmp.LastName} by User #{currentUserId}. Reason: {reassignReason}]";
                currentStep.UpdatedBy = (int)currentUserId;
                currentStep.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.WorkflowInstanceSteps.Update(currentStep);
                _unitOfWork.Save();

                // Audit log
                await _unitOfWork.WorkflowAuditLogs.Add(new WorkflowAuditLog
                {
                    WorkflowInstanceId = instance.Id,
                    WorkflowInstanceStepId = currentStep.Id,
                    EventType = "StepReassigned",
                    PerformedByEmployeeId = currentUserId,
                    ActionCapacity = "Reassign",
                    Details = $"Step #{currentStep.StepOrder} '{currentStep.StepName}' reassigned to {targetEmp.FirstName} {targetEmp.LastName} (Emp #{targetEmp.Id}). Reason: {reassignReason}",
                    Timestamp = DateTime.UtcNow,
                    CreatedBy = (int)currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = (int)currentUserId,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();

                // Notify target employee
                try
                {
                    await _unitOfWork.Notifications.Add(new Notification
                    {
                        EmployeeId = targetEmp.Id,
                        Title = "Approval Step Reassigned to You",
                        Message = $"Step '{currentStep.StepName}' for {instance.RequestType} #{instance.RequestId} was reassigned to you. Reason: {reassignReason}",
                        Category = instance.RequestType,
                        Type = "request",
                        Icon = "forward_to_inbox",
                        IconBg = "#eff6ff",
                        IconColor = "#2563eb",
                        Route = "/index/request",
                        QueryParams = $"tab={instance.RequestType.ToLowerInvariant()}",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    });
                    _unitOfWork.Save();
                }
                catch { }

                return new WorkflowActionResult
                {
                    Success = true,
                    Message = $"Step reassigned successfully to {targetEmp.FirstName} {targetEmp.LastName}.",
                    CurrentStepOrder = instance.CurrentStepOrder,
                    WorkflowStatus = instance.Status,
                    ActionCapacity = "Reassign"
                };
            }

            // ==============================================================
            // 2. OVERRIDE & STANDARD APPROVAL AUTHORIZATION CHECKS
            // ==============================================================
            string actionCapacity;
            string decision;

            if (isOverride)
            {
                bool canOverride = await _permissionService.HasPermissionAsync(currentUserId, "REQUEST_OVERRIDE");
                if (!canOverride)
                {
                    return new WorkflowActionResult { Success = false, StatusCode = "Forbidden", Message = "Missing required permission 'REQUEST_OVERRIDE'." };
                }

                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return new WorkflowActionResult { Success = false, StatusCode = "BadRequest", Message = "A mandatory override reason must be provided when executing an administrative override." };
                }

                actionCapacity = "Override";
                decision = actionNorm.Equals("OverrideApprove", StringComparison.OrdinalIgnoreCase) ? "Approved" : "Rejected";
            }
            else
            {
                // Two mandatory backend checks:
                // Check 1: User has module permission (e.g. LEAVE_APPROVE)
                string modulePerm = GetModuleApprovalPermission(instance.RequestType);
                bool hasModulePerm = await _permissionService.HasPermissionAsync(currentUserId, modulePerm);
                if (!hasModulePerm)
                {
                    return new WorkflowActionResult
                    {
                        Success = false,
                        StatusCode = "Forbidden",
                        Message = $"Access Denied: Missing required permission '{modulePerm}'."
                    };
                }

                // Check 2: User is in current step's eligible approvers (holding permission alone is NOT enough!)
                var eligibleIds = new List<long>();
                try
                {
                    eligibleIds = JsonSerializer.Deserialize<List<long>>(currentStep.EligibleApproverIdsJson) ?? new List<long>();
                }
                catch { }

                if (!eligibleIds.Contains(currentUserId))
                {
                    return new WorkflowActionResult
                    {
                        Success = false,
                        StatusCode = "Forbidden",
                        Message = "Access Denied: You are not an assigned approver for this workflow step. Holding approval permissions alone is not sufficient."
                    };
                }

                // Check if acting as Delegate
                var now = DateTime.UtcNow;
                var delegations = (await _unitOfWork.WorkflowDelegations.GetAll())
                    .Where(d => d.ToEmployeeId == currentUserId && d.IsActive && !d.IsDeleted &&
                                d.StartDate <= now && d.EndDate >= now &&
                                eligibleIds.Contains(d.FromEmployeeId))
                    .ToList();

                actionCapacity = delegations.Any() ? "Delegate" : "Approver";
                decision = isStandardApprove ? "Approved" : "Rejected";
            }

            // ==============================================================
            // 3. ATOMIC STEP CLOSING (SINGLE-WINNER CONCURRENCY)
            // ==============================================================
            // Only update if step is still Pending!
            if (!string.Equals(currentStep.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                return new WorkflowActionResult
                {
                    Success = false,
                    StatusCode = "AlreadyActioned",
                    Message = "This step has already been actioned by another approver."
                };
            }

            currentStep.Status = decision;
            currentStep.ActionByEmployeeId = currentUserId;
            currentStep.ActionCapacity = actionCapacity;
            currentStep.ActionDecision = decision;
            currentStep.Comment = request.Reason;
            currentStep.ActionTime = DateTime.UtcNow;
            currentStep.UpdatedBy = (int)currentUserId;
            currentStep.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.WorkflowInstanceSteps.Update(currentStep);
            int rowsAffected = _unitOfWork.Save();

            if (rowsAffected <= 0)
            {
                return new WorkflowActionResult
                {
                    Success = false,
                    StatusCode = "AlreadyActioned",
                    Message = "Action could not be completed. The step may have already been closed."
                };
            }

            // ==============================================================
            // 4. POST-DECISION PROGRESSION & AUDIT LOGGING
            // ==============================================================
            string eventType = decision == "Approved" ? "StepApproved" : "StepRejected";
            await _unitOfWork.WorkflowAuditLogs.Add(new WorkflowAuditLog
            {
                WorkflowInstanceId = instance.Id,
                WorkflowInstanceStepId = currentStep.Id,
                EventType = eventType,
                PerformedByEmployeeId = currentUserId,
                ActionCapacity = actionCapacity,
                Details = $"Step #{currentStep.StepOrder} '{currentStep.StepName}' was {decision.ToLowerInvariant()} by User #{currentUserId} (Capacity: {actionCapacity}). Comment: {request.Reason ?? "None"}",
                OldStatus = "Pending",
                NewStatus = decision,
                Timestamp = DateTime.UtcNow,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = (int)currentUserId,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            });
            _unitOfWork.Save();

            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());
            string actorName = allEmployees.TryGetValue(currentUserId, out var aName) ? aName : $"User #{currentUserId}";

            if (decision == "Rejected")
            {
                // Workflow terminates on rejection
                instance.Status = "Rejected";
                instance.CompletedDate = DateTime.UtcNow;
                instance.FinalActionBy = currentUserId;
                instance.FinalComment = request.Reason;
                instance.UpdatedBy = (int)currentUserId;
                instance.UpdatedDate = DateTime.UtcNow;

                // Cancel all remaining waiting steps
                var remainingWaiting = allSteps.Where(s => s.StepOrder > currentStep.StepOrder && s.Status == "Waiting").ToList();
                foreach (var ws in remainingWaiting)
                {
                    ws.Status = "Skipped";
                    ws.ResolutionReason = "Skipped due to prior step rejection.";
                    _unitOfWork.WorkflowInstanceSteps.Update(ws);
                }

                _unitOfWork.WorkflowInstances.Update(instance);
                _unitOfWork.Save();
                await SyncDomainStatusAsync(instance, "Rejected", currentUserId, request.Reason);

                // Notify requester
                try
                {
                    await _unitOfWork.Notifications.Add(new Notification
                    {
                        EmployeeId = instance.RequesterEmployeeId,
                        Title = $"Your {instance.RequestType} Request was Rejected",
                        Message = $"Your {instance.RequestType} request #{instance.RequestId} was rejected at step '{currentStep.StepName}' by {actorName}. Reason: {request.Reason ?? "No reason provided."}",
                        Category = instance.RequestType,
                        Type = "alert",
                        Icon = "cancel",
                        IconBg = "#fee2e2",
                        IconColor = "#dc2626",
                        Route = "/index/request",
                        QueryParams = $"tab={instance.RequestType.ToLowerInvariant()}",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    });
                    _unitOfWork.Save();
                }
                catch { }

                return new WorkflowActionResult
                {
                    Success = true,
                    Message = $"Workflow step #{currentStep.StepOrder} rejected.",
                    WorkflowStatus = "Rejected",
                    IsWorkflowCompleted = true,
                    CurrentStepOrder = currentStep.StepOrder,
                    ActionCapacity = actionCapacity
                };
            }

            // If Approved, check for next step in pipeline
            var nextStep = allSteps.FirstOrDefault(s => s.StepOrder > currentStep.StepOrder && s.Status != "Skipped");

            if (nextStep != null)
            {
                // Advance to next step
                nextStep.Status = "Pending";
                nextStep.StepStartTime = DateTime.UtcNow;
                if (nextStep.DueTime == null)
                {
                    // Look up definition step for time limit
                    var defStep = (await _unitOfWork.WorkflowSteps.GetAll())
                        .FirstOrDefault(s => s.WorkflowDefinitionId == instance.WorkflowDefinitionId && s.StepOrder == nextStep.StepOrder);
                    if (defStep != null && defStep.TimeLimitHours.HasValue && defStep.IsEscalationEnabled)
                    {
                        nextStep.DueTime = DateTime.UtcNow.AddHours(defStep.TimeLimitHours.Value);
                    }
                }

                instance.CurrentStepOrder = nextStep.StepOrder;
                instance.UpdatedBy = (int)currentUserId;
                instance.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.WorkflowInstanceSteps.Update(nextStep);
                _unitOfWork.WorkflowInstances.Update(instance);
                _unitOfWork.Save();
                await SyncDomainStatusAsync(instance, "Rejected", currentUserId, request.Reason);

                // Notify next step approvers
                var nextApproverIds = new List<long>();
                try
                {
                    nextApproverIds = JsonSerializer.Deserialize<List<long>>(nextStep.EligibleApproverIdsJson) ?? new List<long>();
                }
                catch { }

                foreach (var nextAppId in nextApproverIds)
                {
                    try
                    {
                        await _unitOfWork.Notifications.Add(new Notification
                        {
                            EmployeeId = nextAppId,
                            Title = $"New {instance.RequestType} Request Approval Required",
                            Message = $"A {instance.RequestType} request #{instance.RequestId} has reached step '{nextStep.StepName}' and requires your decision.",
                            Category = instance.RequestType,
                            Type = "request",
                            Icon = "assignment",
                            IconBg = "#eff6ff",
                            IconColor = "#2563eb",
                            Route = "/index/request",
                            QueryParams = $"tab={instance.RequestType.ToLowerInvariant()}",
                            IsRead = false,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false
                        });
                    }
                    catch { }
                }
                _unitOfWork.Save();

                return new WorkflowActionResult
                {
                    Success = true,
                    Message = $"Step #{currentStep.StepOrder} approved. Workflow advanced to step #{nextStep.StepOrder} '{nextStep.StepName}'.",
                    WorkflowStatus = "Pending",
                    IsWorkflowCompleted = false,
                    CurrentStepOrder = nextStep.StepOrder,
                    ActionCapacity = actionCapacity
                };
            }
            else
            {
                // All steps completed -> Entire workflow Approved!
                instance.Status = "Approved";
                instance.CompletedDate = DateTime.UtcNow;
                instance.FinalActionBy = currentUserId;
                instance.FinalComment = request.Reason;
                instance.UpdatedBy = (int)currentUserId;
                instance.UpdatedDate = DateTime.UtcNow;

                _unitOfWork.WorkflowInstances.Update(instance);
                _unitOfWork.Save();
                await SyncDomainStatusAsync(instance, "Approved", currentUserId, request.Reason);

                // Audit log
                await _unitOfWork.WorkflowAuditLogs.Add(new WorkflowAuditLog
                {
                    WorkflowInstanceId = instance.Id,
                    EventType = "WorkflowCompleted",
                    PerformedByEmployeeId = currentUserId,
                    ActionCapacity = actionCapacity,
                    Details = $"Workflow for {instance.RequestType} #{instance.RequestId} reached final approval. All steps successfully completed.",
                    OldStatus = "Pending",
                    NewStatus = "Approved",
                    Timestamp = DateTime.UtcNow,
                    CreatedBy = (int)currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = (int)currentUserId,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();

                // Notify requester
                try
                {
                    await _unitOfWork.Notifications.Add(new Notification
                    {
                        EmployeeId = instance.RequesterEmployeeId,
                        Title = $"Your {instance.RequestType} Request was Approved",
                        Message = $"Congratulations! Your {instance.RequestType} request #{instance.RequestId} has received final approval.",
                        Category = instance.RequestType,
                        Type = "approval",
                        Icon = "check_circle",
                        IconBg = "#f0fdf4",
                        IconColor = "#16a34a",
                        Route = "/index/request",
                        QueryParams = $"tab={instance.RequestType.ToLowerInvariant()}",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    });
                    _unitOfWork.Save();
                }
                catch { }

                return new WorkflowActionResult
                {
                    Success = true,
                    Message = "Workflow successfully approved and completed.",
                    WorkflowStatus = "Approved",
                    IsWorkflowCompleted = true,
                    CurrentStepOrder = currentStep.StepOrder,
                    ActionCapacity = actionCapacity
                };
            }
        }

        public async Task<bool> CancelWorkflowAsync(long instanceId, long requesterId, string cancelReason)
        {
            var instance = await _unitOfWork.WorkflowInstances.GetById(instanceId);
            if (instance == null || !instance.IsActive || instance.IsDeleted) return false;

            if (instance.RequesterEmployeeId != requesterId)
            {
                bool canOverride = await _permissionService.HasPermissionAsync(requesterId, "REQUEST_OVERRIDE");
                if (!canOverride)
                {
                    throw new UnauthorizedAccessException("Only the requester or an authorized administrator can cancel this workflow.");
                }
            }

            if (!string.Equals(instance.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Cannot cancel a workflow that is already '{instance.Status}'.");
            }

            instance.Status = "Cancelled";
            instance.CompletedDate = DateTime.UtcNow;
            instance.FinalActionBy = requesterId;
            instance.FinalComment = cancelReason;
            instance.UpdatedBy = (int)requesterId;
            instance.UpdatedDate = DateTime.UtcNow;

            var steps = (await _unitOfWork.WorkflowInstanceSteps.GetAll())
                .Where(s => s.WorkflowInstanceId == instanceId && s.IsActive && !s.IsDeleted)
                .ToList();

            foreach (var s in steps)
            {
                if (s.Status == "Pending" || s.Status == "Waiting")
                {
                    s.Status = "Cancelled";
                    s.Comment = cancelReason;
                    s.UpdatedBy = (int)requesterId;
                    s.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.WorkflowInstanceSteps.Update(s);
                }
            }

            _unitOfWork.WorkflowInstances.Update(instance);
            _unitOfWork.Save();
            await SyncDomainStatusAsync(instance, "Cancelled", requesterId, cancelReason);

            // Audit log
            await _unitOfWork.WorkflowAuditLogs.Add(new WorkflowAuditLog
            {
                WorkflowInstanceId = instance.Id,
                EventType = "WorkflowCancelled",
                PerformedByEmployeeId = requesterId,
                ActionCapacity = "Requester",
                Details = $"Workflow was cancelled by requester/admin. Reason: {cancelReason}",
                OldStatus = "Pending",
                NewStatus = "Cancelled",
                Timestamp = DateTime.UtcNow,
                CreatedBy = (int)requesterId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = (int)requesterId,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            });
            _unitOfWork.Save();

            return true;
        }

        public async Task<List<WorkflowInboxItemDTO>> GetPendingInboxAsync(long currentUserId)
        {
            var pendingInstances = (await _unitOfWork.WorkflowInstances.GetAll())
                .Where(i => i.Status == "Pending" && i.IsActive && !i.IsDeleted)
                .ToList();

            if (pendingInstances.Count == 0) return new List<WorkflowInboxItemDTO>();

            var instanceIds = pendingInstances.Select(i => i.Id).ToHashSet();
            var allSteps = (await _unitOfWork.WorkflowInstanceSteps.GetAll())
                .Where(s => instanceIds.Contains(s.WorkflowInstanceId) && s.Status == "Pending" && s.IsActive && !s.IsDeleted)
                .ToList();

            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);
            var designations = (await _unitOfWork.Designations.GetAll()).ToDictionary(d => d.Id);
            var leaves = (await _unitOfWork.LeaveRequest.GetAll()).Where(l => l.IsActive != false && l.IsDeleted != true).ToDictionary(l => l.Id);
            var leaveTypes = (await _unitOfWork.LeaveType.GetAll()).ToDictionary(t => t.Id);
            bool canReassign = await _permissionService.HasPermissionAsync(currentUserId, "REQUEST_REASSIGN");

            var inboxItems = new List<WorkflowInboxItemDTO>();

            foreach (var inst in pendingInstances)
            {
                var curStep = allSteps.FirstOrDefault(s => s.WorkflowInstanceId == inst.Id && s.StepOrder == inst.CurrentStepOrder);
                if (curStep == null) continue;

                var approverIds = new List<long>();
                try
                {
                    approverIds = JsonSerializer.Deserialize<List<long>>(curStep.EligibleApproverIdsJson) ?? new List<long>();
                }
                catch { }

                // INBOX FILTER: only items where current user is an eligible approver!
                if (!approverIds.Contains(currentUserId)) continue;

                employees.TryGetValue(inst.RequesterEmployeeId, out var reqEmp);
                string reqName = reqEmp != null ? $"{reqEmp.FirstName} {reqEmp.LastName}".Trim() : $"Employee #{inst.RequesterEmployeeId}";
                string? desigName = reqEmp?.DesignationId.HasValue == true && designations.TryGetValue(reqEmp.DesignationId.Value, out var desig) ? desig.DesignationName : reqEmp?.Designation;
                string? deptName = reqEmp?.DesignationId.HasValue == true && designations.TryGetValue(reqEmp.DesignationId.Value, out var desigD) ? desigD.DepartmentCategory : null;

                var pendingNames = approverIds
                    .Select(id => employees.TryGetValue(id, out var emp) ? $"{emp.FirstName} {emp.LastName}".Trim() : $"Emp #{id}")
                    .ToList();

                var payload = new Dictionary<string, object?>();
                string title = $"{inst.RequestType} Request #{inst.RequestId}";
                string summary = $"Awaiting {curStep.StepName}";

                if (string.Equals(inst.RequestType, "Leave", StringComparison.OrdinalIgnoreCase) && leaves.TryGetValue(inst.RequestId, out var leave))
                {
                    string ltName = leaveTypes.TryGetValue(leave.LeaveTypeId, out var lt) ? (lt.Type ?? "Leave") : "Leave";
                    payload["StartDate"] = leave.StartDate.ToString("yyyy-MM-dd");
                    payload["EndDate"] = leave.EndDate.ToString("yyyy-MM-dd");
                    payload["TotalDays"] = leave.TotalDays;
                    payload["LeaveType"] = ltName;
                    payload["LeaveMode"] = leave.LeaveMode ?? "Full Day";
                    payload["LeaveReason"] = leave.LeaveReason;

                    title = $"{ltName} - {reqName}";
                    summary = $"{leave.StartDate:yyyy-MM-dd} to {leave.EndDate:yyyy-MM-dd}";
                }

                inboxItems.Add(new WorkflowInboxItemDTO
                {
                    WorkflowInstanceId = inst.Id,
                    RequestType = inst.RequestType,
                    RequestId = inst.RequestId,
                    RequesterId = inst.RequesterEmployeeId,
                    RequesterName = reqName,
                    RequesterRole = desigName,
                    RequesterDepartment = deptName,
                    RequesterAvatar = reqEmp?.ProfileImage,
                    Title = title,
                    Summary = summary,
                    SubmittedDate = inst.StartedDate,
                    DueTime = curStep.DueTime,
                    CurrentStepOrder = curStep.StepOrder,
                    CurrentStepName = curStep.StepName,
                    PendingWith = string.Join(", ", pendingNames),
                    Status = inst.Status,
                    IsEscalated = curStep.IsEscalated,
                    CanApprove = true,
                    CanOverride = false,
                    CanReassign = canReassign,
                    CanCancel = inst.RequesterEmployeeId == currentUserId,
                    PayloadSummary = payload
                });
            }

            return inboxItems.OrderBy(i => i.DueTime ?? DateTime.MaxValue).ThenByDescending(i => i.SubmittedDate).ToList();
        }

        public async Task<List<WorkflowScopedRequestDTO>> GetRequestsInScopeAsync(long currentUserId, string? requestType, string? status)
        {
            var query = (await _unitOfWork.WorkflowInstances.GetAll())
                .Where(i => i.IsActive && !i.IsDeleted);

            if (!string.IsNullOrWhiteSpace(requestType) && !requestType.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(i => string.Equals(i.RequestType, requestType, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(i => string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase));
            }

            var instances = query.ToList();
            if (instances.Count == 0) return new List<WorkflowScopedRequestDTO>();

            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id);
            var designations = (await _unitOfWork.Designations.GetAll()).ToDictionary(d => d.Id);
            var leaves = (await _unitOfWork.LeaveRequest.GetAll()).Where(l => l.IsActive != false && l.IsDeleted != true).ToDictionary(l => l.Id);
            var leaveTypes = (await _unitOfWork.LeaveType.GetAll()).ToDictionary(t => t.Id);

            // Scope filter:
            bool canViewAll = await _permissionService.HasPermissionAsync(currentUserId, "LEAVE_VIEW_ALL") ||
                              await _permissionService.HasPermissionAsync(currentUserId, "TIMESHEET_VIEW_ALL") ||
                              await _permissionService.HasPermissionAsync(currentUserId, "EXPENSE_VIEW_ALL");

            var directReporteeIds = employees.Values.Where(e => e.ManagerId == currentUserId).Select(e => e.Id).ToHashSet();

            if (!canViewAll)
            {
                // Scoped to direct reportees or self
                instances = instances.Where(i => i.RequesterEmployeeId == currentUserId || directReporteeIds.Contains(i.RequesterEmployeeId)).ToList();
            }

            var instanceIds = instances.Select(i => i.Id).ToHashSet();
            var allSteps = (await _unitOfWork.WorkflowInstanceSteps.GetAll())
                .Where(s => instanceIds.Contains(s.WorkflowInstanceId) && s.IsActive && !s.IsDeleted)
                .ToList();

            bool canOverrideGlobal = await _permissionService.HasPermissionAsync(currentUserId, "REQUEST_OVERRIDE");
            bool canReassignGlobal = await _permissionService.HasPermissionAsync(currentUserId, "REQUEST_REASSIGN");

            var result = new List<WorkflowScopedRequestDTO>();

            foreach (var inst in instances)
            {
                var curStep = allSteps.FirstOrDefault(s => s.WorkflowInstanceId == inst.Id && s.StepOrder == inst.CurrentStepOrder && s.Status == "Pending");
                var approverIds = new List<long>();
                if (curStep != null)
                {
                    try
                    {
                        approverIds = JsonSerializer.Deserialize<List<long>>(curStep.EligibleApproverIdsJson) ?? new List<long>();
                    }
                    catch { }
                }

                employees.TryGetValue(inst.RequesterEmployeeId, out var reqEmp);
                string reqName = reqEmp != null ? $"{reqEmp.FirstName} {reqEmp.LastName}".Trim() : $"Employee #{inst.RequesterEmployeeId}";
                string? deptName = reqEmp?.DesignationId.HasValue == true && designations.TryGetValue(reqEmp.DesignationId.Value, out var desigD) ? desigD.DepartmentCategory : null;

                var pendingNames = approverIds
                    .Select(id => employees.TryGetValue(id, out var emp) ? $"{emp.FirstName} {emp.LastName}".Trim() : $"Emp #{id}")
                    .ToList();

                bool isPending = string.Equals(inst.Status, "Pending", StringComparison.OrdinalIgnoreCase);

                var payload = new Dictionary<string, object?>();
                string title = $"{inst.RequestType} Request #{inst.RequestId}";
                string summary = isPending && curStep != null ? $"Pending {curStep.StepName}" : inst.Status;

                bool isInDecisionChain = inst.RequesterEmployeeId == currentUserId ||
                                         approverIds.Contains(currentUserId) ||
                                         allSteps.Any(s => s.WorkflowInstanceId == inst.Id && s.ActionByEmployeeId == currentUserId) ||
                                         canOverrideGlobal;

                if (string.Equals(inst.RequestType, "Leave", StringComparison.OrdinalIgnoreCase) && leaves.TryGetValue(inst.RequestId, out var leave))
                {
                    string ltName = leaveTypes.TryGetValue(leave.LeaveTypeId, out var lt) ? (lt.Type ?? "Leave") : "Leave";
                    string? reason = MaskSensitiveDetailsIfNeeded(inst.RequestType, leave.LeaveReason, ltName, isInDecisionChain);
                    payload["StartDate"] = leave.StartDate.ToString("yyyy-MM-dd");
                    payload["EndDate"] = leave.EndDate.ToString("yyyy-MM-dd");
                    payload["TotalDays"] = leave.TotalDays;
                    payload["LeaveType"] = ltName;
                    payload["LeaveMode"] = leave.LeaveMode ?? "Full Day";
                    payload["LeaveReason"] = reason;

                    title = $"{ltName} - {reqName}";
                    summary = $"{leave.StartDate:yyyy-MM-dd} to {leave.EndDate:yyyy-MM-dd}";
                }

                result.Add(new WorkflowScopedRequestDTO
                {
                    WorkflowInstanceId = inst.Id,
                    RequestType = inst.RequestType,
                    RequestId = inst.RequestId,
                    RequesterId = inst.RequesterEmployeeId,
                    RequesterName = reqName,
                    RequesterDepartment = deptName,
                    Title = title,
                    Summary = summary,
                    SubmittedDate = inst.StartedDate,
                    Status = inst.Status,
                    CurrentStepOrder = inst.CurrentStepOrder,
                    CurrentStepName = curStep?.StepName ?? string.Empty,
                    PendingWith = string.Join(", ", pendingNames),
                    CanApprove = isPending && approverIds.Contains(currentUserId),
                    CanOverride = isPending && canOverrideGlobal && !approverIds.Contains(currentUserId),
                    CanReassign = isPending && canReassignGlobal,
                    CanCancel = isPending && inst.RequesterEmployeeId == currentUserId,
                    PayloadSummary = payload
                });
            }

            return result.OrderByDescending(r => r.SubmittedDate).ToList();
        }

        public async Task<WorkflowInstanceDetailDTO?> GetInstanceDetailsAsync(long instanceId, long currentUserId)
        {
            var instance = await _unitOfWork.WorkflowInstances.GetById(instanceId);
            if (instance == null || !instance.IsActive || instance.IsDeleted) return null;

            var steps = (await _unitOfWork.WorkflowInstanceSteps.GetAll())
                .Where(s => s.WorkflowInstanceId == instanceId && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.StepOrder)
                .ToList();

            var auditLogs = (await _unitOfWork.WorkflowAuditLogs.GetAll())
                .Where(a => a.WorkflowInstanceId == instanceId && a.IsActive && !a.IsDeleted)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            var curStep = steps.FirstOrDefault(s => s.StepOrder == instance.CurrentStepOrder && s.Status == "Pending");
            var approverIds = new List<long>();
            if (curStep != null)
            {
                try
                {
                    approverIds = JsonSerializer.Deserialize<List<long>>(curStep.EligibleApproverIdsJson) ?? new List<long>();
                }
                catch { }
            }

            bool isPending = string.Equals(instance.Status, "Pending", StringComparison.OrdinalIgnoreCase);
            bool canOverride = isPending && await _permissionService.HasPermissionAsync(currentUserId, "REQUEST_OVERRIDE");
            bool canReassign = isPending && await _permissionService.HasPermissionAsync(currentUserId, "REQUEST_REASSIGN");

            var pendingNames = approverIds.Select(id => employees.TryGetValue(id, out var n) ? n : $"Emp #{id}").ToList();

            bool isInDecisionChain = instance.RequesterEmployeeId == currentUserId ||
                                     approverIds.Contains(currentUserId) ||
                                     steps.Any(s => s.ActionByEmployeeId == currentUserId) ||
                                     canOverride;

            var payload = new Dictionary<string, object?>();
            if (string.Equals(instance.RequestType, "Leave", StringComparison.OrdinalIgnoreCase))
            {
                var leave = await _unitOfWork.LeaveRequest.GetById(instance.RequestId);
                if (leave != null)
                {
                    var lt = await _unitOfWork.LeaveType.GetById(leave.LeaveTypeId);
                    string ltName = lt?.Type ?? "Leave";
                    string? reason = MaskSensitiveDetailsIfNeeded(instance.RequestType, leave.LeaveReason, ltName, isInDecisionChain);
                    payload["StartDate"] = leave.StartDate.ToString("yyyy-MM-dd");
                    payload["EndDate"] = leave.EndDate.ToString("yyyy-MM-dd");
                    payload["TotalDays"] = leave.TotalDays;
                    payload["LeaveType"] = ltName;
                    payload["LeaveMode"] = leave.LeaveMode ?? "Full Day";
                    payload["LeaveReason"] = reason;
                }
            }

            return new WorkflowInstanceDetailDTO
            {
                WorkflowInstanceId = instance.Id,
                RequestType = instance.RequestType,
                RequestId = instance.RequestId,
                RequesterId = instance.RequesterEmployeeId,
                RequesterName = employees.TryGetValue(instance.RequesterEmployeeId, out var rName) ? rName : $"Employee #{instance.RequesterEmployeeId}",
                Status = instance.Status,
                DefinitionVersion = instance.DefinitionVersion,
                CurrentStepOrder = instance.CurrentStepOrder,
                StartedDate = instance.StartedDate,
                CompletedDate = instance.CompletedDate,
                PendingWith = string.Join(", ", pendingNames),
                CanApprove = isPending && approverIds.Contains(currentUserId),
                CanOverride = canOverride && !approverIds.Contains(currentUserId),
                CanReassign = canReassign,
                CanCancel = isPending && instance.RequesterEmployeeId == currentUserId,
                PayloadSummary = payload,
                Steps = steps.Select(s =>
                {
                    var stepAppIds = new List<long>();
                    try { stepAppIds = JsonSerializer.Deserialize<List<long>>(s.EligibleApproverIdsJson) ?? new List<long>(); } catch { }
                    return new WorkflowStepDetailDTO
                    {
                        Id = s.Id,
                        StepOrder = s.StepOrder,
                        StepName = s.StepName,
                        Status = s.Status,
                        ApproverRuleType = s.ApproverRuleType,
                        ResolutionReason = s.ResolutionReason ?? string.Empty,
                        EligibleApproverNames = stepAppIds.Select(id => employees.TryGetValue(id, out var n) ? n : $"Emp #{id}").ToList(),
                        ActionByName = s.ActionByEmployeeId.HasValue && employees.TryGetValue(s.ActionByEmployeeId.Value, out var aName) ? aName : null,
                        ActionCapacity = s.ActionCapacity,
                        ActionDecision = s.ActionDecision,
                        Comment = s.Comment,
                        StepStartTime = s.StepStartTime,
                        ActionTime = s.ActionTime,
                        DueTime = s.DueTime,
                        IsEscalated = s.IsEscalated
                    };
                }).ToList(),
                AuditTrail = auditLogs.Select(a => new WorkflowAuditItemDTO
                {
                    Id = a.Id,
                    EventType = a.EventType,
                    PerformedByName = a.PerformedByEmployeeId.HasValue && employees.TryGetValue(a.PerformedByEmployeeId.Value, out var pName) ? pName : "System",
                    ActionCapacity = a.ActionCapacity,
                    Details = a.Details,
                    OldStatus = a.OldStatus,
                    NewStatus = a.NewStatus,
                    Timestamp = a.Timestamp
                }).ToList()
            };
        }

        public async Task EscalateOverdueStepsAsync()
        {
            var now = DateTime.UtcNow;
            var overdueSteps = (await _unitOfWork.WorkflowInstanceSteps.GetAll())
                .Where(s => s.Status == "Pending" && !s.IsEscalated && s.DueTime.HasValue && s.DueTime.Value < now && s.IsActive && !s.IsDeleted)
                .ToList();

            if (overdueSteps.Count == 0) return;

            var allEmployees = (await _unitOfWork.Employees.GetAll()).Where(e => e.IsActive && !e.IsDeleted).ToList();
            var allInstances = (await _unitOfWork.WorkflowInstances.GetAll()).ToDictionary(i => i.Id);
            var allDefinitions = (await _unitOfWork.WorkflowDefinitions.GetAll()).ToDictionary(d => d.Id);
            var allDefSteps = (await _unitOfWork.WorkflowSteps.GetAll()).ToList();

            foreach (var step in overdueSteps)
            {
                if (!allInstances.TryGetValue(step.WorkflowInstanceId, out var instance) || instance.Status != "Pending")
                {
                    continue;
                }

                var defStep = allDefSteps.FirstOrDefault(s => s.WorkflowDefinitionId == instance.WorkflowDefinitionId && s.StepOrder == step.StepOrder);
                if (defStep == null || !defStep.IsEscalationEnabled || string.IsNullOrWhiteSpace(defStep.EscalationTarget))
                {
                    continue;
                }

                // Escalation Target resolution
                string target = defStep.EscalationTarget.Trim();
                var escalationEmployees = new List<Employee>();

                if (target.Equals("SkipLevelManager", StringComparison.OrdinalIgnoreCase))
                {
                    var requester = allEmployees.FirstOrDefault(e => e.Id == instance.RequesterEmployeeId);
                    long? mgrId = requester?.ManagerId;
                    var mgr = allEmployees.FirstOrDefault(e => e.Id == mgrId);
                    long? skipMgrId = mgr?.ManagerId;
                    var skipMgr = allEmployees.FirstOrDefault(e => e.Id == skipMgrId);
                    if (skipMgr != null && skipMgr.Id != instance.RequesterEmployeeId)
                    {
                        escalationEmployees.Add(skipMgr);
                    }
                }
                else if (target.StartsWith("Role:", StringComparison.OrdinalIgnoreCase))
                {
                    string roleName = target.Substring(5).Trim();
                    var roles = (await _unitOfWork.RoleMaster.GetAll()).Where(r => r.IsActive && !r.IsDeleted).ToList();
                    var matchedRole = roles.FirstOrDefault(r => string.Equals(r.RoleName, roleName, StringComparison.OrdinalIgnoreCase));
                    if (matchedRole != null)
                    {
                        var roleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                            .Where(m => m.RoleId == matchedRole.Id && m.IsActive == true && m.IsDeleted != true)
                            .Select(m => m.EmployeeId)
                            .ToHashSet();

                        escalationEmployees.AddRange(allEmployees.Where(e => e.Id != instance.RequesterEmployeeId && roleMappings.Contains(e.Id)));
                    }
                }
                else if (target.StartsWith("SpecificPerson:", StringComparison.OrdinalIgnoreCase))
                {
                    if (long.TryParse(target.Substring(15).Trim(), out var pId))
                    {
                        var person = allEmployees.FirstOrDefault(e => e.Id == pId && e.Id != instance.RequesterEmployeeId);
                        if (person != null) escalationEmployees.Add(person);
                    }
                }

                if (escalationEmployees.Count == 0)
                {
                    // Fallback to HR
                    var hrRole = (await _unitOfWork.RoleMaster.GetAll())
                        .FirstOrDefault(r => string.Equals(r.RoleName, "HR", StringComparison.OrdinalIgnoreCase) && r.IsActive && !r.IsDeleted);
                    if (hrRole != null)
                    {
                        var hrEmpIds = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                            .Where(m => m.RoleId == hrRole.Id && m.IsActive == true && m.IsDeleted != true)
                            .Select(m => m.EmployeeId)
                            .ToHashSet();
                        escalationEmployees.AddRange(allEmployees.Where(e => e.Id != instance.RequesterEmployeeId && hrEmpIds.Contains(e.Id)));
                    }
                }

                if (escalationEmployees.Count > 0)
                {
                    var existingEligible = new List<long>();
                    try
                    {
                        existingEligible = JsonSerializer.Deserialize<List<long>>(step.EligibleApproverIdsJson) ?? new List<long>();
                    }
                    catch { }

                    existingEligible.AddRange(escalationEmployees.Select(e => e.Id));
                    var uniqueEligible = existingEligible.Distinct().ToList();

                    step.EligibleApproverIdsJson = JsonSerializer.Serialize(uniqueEligible);
                    step.IsEscalated = true;
                    step.EscalatedTime = DateTime.UtcNow;
                    step.ResolutionReason += $" [Escalated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} to {string.Join(", ", escalationEmployees.Select(e => $"{e.FirstName} {e.LastName}"))}]";
                    step.UpdatedDate = DateTime.UtcNow;

                    _unitOfWork.WorkflowInstanceSteps.Update(step);
                    _unitOfWork.Save();

                    // Audit
                    await _unitOfWork.WorkflowAuditLogs.Add(new WorkflowAuditLog
                    {
                        WorkflowInstanceId = instance.Id,
                        WorkflowInstanceStepId = step.Id,
                        EventType = "StepEscalated",
                        ActionCapacity = "System",
                        Details = $"Step #{step.StepOrder} '{step.StepName}' timed out (Time Limit: {step.DueTime:yyyy-MM-dd HH:mm}) and was auto-escalated to target '{target}'.",
                        Timestamp = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                        UpdatedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false
                    });
                    _unitOfWork.Save();

                    // Notify escalation approvers
                    foreach (var escEmp in escalationEmployees)
                    {
                        try
                        {
                            await _unitOfWork.Notifications.Add(new Notification
                            {
                                EmployeeId = escEmp.Id,
                                Title = $"Escalated Approval: Overdue {instance.RequestType} Request",
                                Message = $"A {instance.RequestType} request #{instance.RequestId} is overdue at step '{step.StepName}' and has been escalated to you.",
                                Category = instance.RequestType,
                                Type = "alert",
                                Icon = "priority_high",
                                IconBg = "#fef2f2",
                                IconColor = "#dc2626",
                                Route = "/index/request",
                                QueryParams = $"tab={instance.RequestType.ToLowerInvariant()}",
                                IsRead = false,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedDate = DateTime.UtcNow,
                                IsActive = true,
                                IsDeleted = false
                            });
                        }
                        catch { }
                    }
                    _unitOfWork.Save();
                }
            }
        }

        private string GetModuleApprovalPermission(string requestType)
        {
            return requestType?.Trim().ToLowerInvariant() switch
            {
                "leave" => "LEAVE_APPROVE",
                "attendance" or "attendanceregularization" or "regularization" => "ATTENDANCE_REGULARIZE_APPROVE",
                "timesheet" => "TIMESHEET_APPROVE",
                "expense" or "expensereport" => "EXPENSE_APPROVE",
                "asset" or "assetrequest" => "ASSET_APPROVE",
                _ => $"{requestType.ToUpperInvariant()}_APPROVE"
            };
        }

        private async Task SyncDomainStatusAsync(WorkflowInstance instance, string status, long actorId, string? comment)
        {
            try
            {
                if (string.Equals(instance.RequestType, "Leave", StringComparison.OrdinalIgnoreCase))
                {
                    var leave = await _unitOfWork.LeaveRequest.GetById(instance.RequestId);
                    if (leave != null)
                    {
                        leave.Status = status;
                        leave.ActionBy = actorId;
                        leave.ActionDate = DateTime.Now;
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            leave.RejectionReason = comment;
                        }
                        _unitOfWork.LeaveRequest.Update(leave);
                        _unitOfWork.Save();
                    }
                }
                else if (string.Equals(instance.RequestType, "AttendanceRegularization", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(instance.RequestType, "Attendance", StringComparison.OrdinalIgnoreCase))
                {
                    var attReq = await _unitOfWork.AttendanceRequests.GetById(instance.RequestId);
                    if (attReq != null)
                    {
                        attReq.Status = status;
                        attReq.ActionBy = actorId;
                        attReq.LastActionBy = actorId;
                        attReq.ActionDate = DateTime.Now;
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            attReq.RejectionReason = comment;
                        }

                        if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
                        {
                            DateTime inTime = attReq.ClockInTime;
                            DateTime outTime = attReq.ClockOutTime ?? inTime.AddHours(9);
                            decimal duration = (decimal)(outTime - inTime).TotalHours;
                            if (duration <= 0)
                            {
                                duration = 8.5m;
                                outTime = inTime.AddHours(9);
                            }

                            var reqDateOnly = DateOnly.FromDateTime(attReq.RequestedDate);
                            var existingAttendance = (await _unitOfWork.EmployeeAttendance.GetAll())
                                .FirstOrDefault(r => r.EmployeeId == attReq.EmployeeId && r.AttendanceDate == reqDateOnly);

                            if (existingAttendance != null)
                            {
                                existingAttendance.ClockIn = inTime;
                                existingAttendance.ClockOut = outTime;
                                existingAttendance.TotalHours = duration;
                                existingAttendance.EffectiveHours = duration;
                                existingAttendance.UpdatedDate = DateTime.Now;
                                existingAttendance.UpdatedBy = (int)actorId;
                                _unitOfWork.EmployeeAttendance.Update(existingAttendance);
                            }
                            else
                            {
                                var newEmpAtt = new EmployeeAttendance
                                {
                                    EmployeeId = attReq.EmployeeId,
                                    ClockIn = inTime,
                                    ClockOut = outTime,
                                    TotalHours = duration,
                                    EffectiveHours = duration,
                                    AttendanceDate = reqDateOnly,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = (int)actorId,
                                    IsActive = true,
                                    IsDeleted = false
                                };
                                await _unitOfWork.EmployeeAttendance.Add(newEmpAtt);
                            }

                            var existingLog = (await _unitOfWork.AttendanceLog.GetAll())
                                .FirstOrDefault(l => l.EmployeeId == attReq.EmployeeId && l.AttendanceDate == reqDateOnly);
                            if (existingLog != null)
                            {
                                existingLog.InTime = inTime;
                                existingLog.OutTime = outTime;
                                existingLog.Duration = duration;
                                _unitOfWork.AttendanceLog.Update(existingLog);
                            }
                            else
                            {
                                await _unitOfWork.AttendanceLog.Add(new AttendanceLog
                                {
                                    EmployeeId = attReq.EmployeeId,
                                    AttendanceDate = reqDateOnly,
                                    InTime = inTime,
                                    OutTime = outTime,
                                    Duration = duration
                                });
                            }
                        }

                        _unitOfWork.AttendanceRequests.Update(attReq);
                        _unitOfWork.Save();
                    }
                }
                else if (string.Equals(instance.RequestType, "Timesheet", StringComparison.OrdinalIgnoreCase))
                {
                    var timesheet = await _unitOfWork.Timesheets.GetById(instance.RequestId);
                    if (timesheet != null)
                    {
                        timesheet.Status = status;
                        timesheet.ActionBy = actorId;
                        timesheet.ActionDate = DateTime.UtcNow;
                        timesheet.UpdatedBy = (int)actorId;
                        timesheet.UpdatedDate = DateTime.UtcNow;

                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            timesheet.RejectionReason = comment;
                        }

                        if (string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase))
                        {
                            var allEntries = await _unitOfWork.TimesheetEntries.GetAll();
                            var existingDrafts = allEntries.Where(e => !e.IsDeleted && e.EmployeeId == timesheet.EmployeeId &&
                                e.EntryDate.Date >= timesheet.StartDate.Date && e.EntryDate.Date <= timesheet.EndDate.Date &&
                                e.TimesheetId == null).ToList();

                            if (!existingDrafts.Any())
                            {
                                var rejectedEntries = allEntries.Where(e => !e.IsDeleted && e.TimesheetId == timesheet.Id).ToList();
                                foreach (var re in rejectedEntries)
                                {
                                    var clone = new TimesheetEntry
                                    {
                                        TimesheetId = null,
                                        EmployeeId = re.EmployeeId,
                                        ProjectId = re.ProjectId,
                                        EntryDate = re.EntryDate,
                                        TaskDescription = re.TaskDescription,
                                        Hours = re.Hours,
                                        ClockInRef = re.ClockInRef,
                                        ClockOutRef = re.ClockOutRef,
                                        IsRegularized = re.IsRegularized,
                                        CreatedBy = re.CreatedBy,
                                        CreatedDate = re.CreatedDate,
                                        UpdatedBy = (int)timesheet.EmployeeId,
                                        UpdatedDate = DateTime.UtcNow,
                                        IsActive = true,
                                        IsDeleted = false
                                    };
                                    await _unitOfWork.TimesheetEntries.Add(clone);
                                }
                            }
                        }

                        _unitOfWork.Timesheets.Update(timesheet);
                        _unitOfWork.Save();
                    }
                }
                else if (string.Equals(instance.RequestType, "Expense", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(instance.RequestType, "ExpenseReport", StringComparison.OrdinalIgnoreCase))
                {
                    var report = await _unitOfWork.ExpenseReports.GetById(instance.RequestId);
                    if (report != null)
                    {
                        if (string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase))
                        {
                            report.Status = "ManagerApproved";
                            report.ManagerId = actorId;
                            report.ManagerActionDate = DateTime.UtcNow;
                            report.ManagerRemarks = comment;
                            report.TotalApprovedAmount = report.TotalClaimedAmount;
                            report.NetPayableAmount = Math.Max(0, report.TotalApprovedAmount - report.AdvanceAdjustedAmount);
                        }
                        else if (string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase))
                        {
                            report.Status = "Rejected";
                            report.ManagerId = actorId;
                            report.ManagerActionDate = DateTime.UtcNow;
                            report.ManagerRemarks = comment;
                            report.TotalApprovedAmount = 0;
                            report.NetPayableAmount = 0;
                        }

                        report.UpdatedDate = DateTime.UtcNow;
                        _unitOfWork.ExpenseReports.Update(report);
                        _unitOfWork.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Could not sync domain entity for workflow instance #{InstanceId} ({RequestType} #{RequestId})", instance.Id, instance.RequestType, instance.RequestId);
            }
        }

        private string? MaskSensitiveDetailsIfNeeded(string requestType, string? originalDetails, string? typeName, bool isInDecisionChain)
        {
            if (isInDecisionChain || string.IsNullOrWhiteSpace(originalDetails))
            {
                return originalDetails;
            }

            bool isMedicalOrSick = (!string.IsNullOrWhiteSpace(typeName) &&
                                    (typeName.IndexOf("sick", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     typeName.IndexOf("medical", StringComparison.OrdinalIgnoreCase) >= 0)) ||
                                   originalDetails.IndexOf("medical", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   originalDetails.IndexOf("doctor", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   originalDetails.IndexOf("hospital", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                   originalDetails.IndexOf("surgery", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isMedicalOrSick)
            {
                return "[Confidential - Medical details restricted to decision chain]";
            }

            return originalDetails;
        }
    }
}
