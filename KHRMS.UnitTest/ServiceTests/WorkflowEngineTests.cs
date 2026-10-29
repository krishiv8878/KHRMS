using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using KHRMS.Services.Workflow.Services;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class WorkflowEngineTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IWorkflowInstanceRepository> _mockInstanceRepo;
        private readonly Mock<IWorkflowInstanceStepRepository> _mockInstanceStepRepo;
        private readonly Mock<IWorkflowDefinitionRepository> _mockDefRepo;
        private readonly Mock<IWorkflowStepRepository> _mockStepRepo;
        private readonly Mock<IWorkflowDelegationRepository> _mockDelegationRepo;
        private readonly Mock<IWorkflowAuditLogRepository> _mockAuditRepo;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly Mock<IEmployeeRepository> _mockEmpRepo;
        private readonly Mock<IApproverResolver> _mockResolver;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<IConditionEvaluator> _mockConditionEvaluator;

        private readonly WorkflowEngineService _engineService;

        public WorkflowEngineTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockInstanceRepo = new Mock<IWorkflowInstanceRepository>();
            _mockInstanceStepRepo = new Mock<IWorkflowInstanceStepRepository>();
            _mockDefRepo = new Mock<IWorkflowDefinitionRepository>();
            _mockStepRepo = new Mock<IWorkflowStepRepository>();
            _mockDelegationRepo = new Mock<IWorkflowDelegationRepository>();
            _mockAuditRepo = new Mock<IWorkflowAuditLogRepository>();
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _mockEmpRepo = new Mock<IEmployeeRepository>();

            _mockUow.Setup(u => u.WorkflowInstances).Returns(_mockInstanceRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstanceSteps).Returns(_mockInstanceStepRepo.Object);
            _mockUow.Setup(u => u.WorkflowDefinitions).Returns(_mockDefRepo.Object);
            _mockUow.Setup(u => u.WorkflowSteps).Returns(_mockStepRepo.Object);
            _mockUow.Setup(u => u.WorkflowDelegations).Returns(_mockDelegationRepo.Object);
            _mockUow.Setup(u => u.WorkflowAuditLogs).Returns(_mockAuditRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotificationRepo.Object);
            _mockUow.Setup(u => u.Employees).Returns(_mockEmpRepo.Object);
            _mockUow.Setup(u => u.Save()).Returns(1);

            _mockResolver = new Mock<IApproverResolver>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockConditionEvaluator = new Mock<IConditionEvaluator>();

            _engineService = new WorkflowEngineService(
                _mockUow.Object,
                _mockResolver.Object,
                _mockPermissionService.Object,
                _mockConditionEvaluator.Object
            );
        }

        // ==============================================================
        // 1. Two simultaneous approvals produce one winner (atomic single-winner)
        // ==============================================================
        [Fact]
        public async Task ActOnStepAsync_TwoSimultaneousApprovals_ProducesOneWinner()
        {
            // Arrange
            long instanceId = 1;
            long approver1 = 101;
            long approver2 = 102;

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                RequestId = 10,
                RequesterEmployeeId = 50,
                CurrentStepOrder = 1,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var step = new WorkflowInstanceStep
            {
                Id = 10,
                WorkflowInstanceId = instanceId,
                StepOrder = 1,
                StepName = "Manager Approval",
                ApproverRuleType = "ReportingManager",
                Status = "Pending",
                EligibleApproverIdsJson = JsonSerializer.Serialize(new List<long> { approver1, approver2 }),
                IsActive = true,
                IsDeleted = false
            };

            var stepsList = new List<WorkflowInstanceStep> { step };

            _mockInstanceRepo.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(stepsList);
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Employee>
            {
                new Employee { Id = approver1, FirstName = "Approver", LastName = "One", IsActive = true, IsDeleted = false },
                new Employee { Id = approver2, FirstName = "Approver", LastName = "Two", IsActive = true, IsDeleted = false }
            });
            _mockDelegationRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowDelegation>());

            _mockPermissionService.Setup(p => p.HasPermissionAsync(approver1, "LEAVE_APPROVE")).ReturnsAsync(true);
            _mockPermissionService.Setup(p => p.HasPermissionAsync(approver2, "LEAVE_APPROVE")).ReturnsAsync(true);

            // Act: First approver actions the step
            var action1 = new WorkflowActionRequest { Action = "Approve", Reason = "Looks good" };
            var result1 = await _engineService.ActOnStepAsync(instanceId, approver1, action1);

            // Assert: Approver 1 succeeded
            Assert.True(result1.Success);
            Assert.Equal("Approved", result1.WorkflowStatus);
            Assert.Equal("Approver", result1.ActionCapacity);

            // Now step status is no longer "Pending"
            step.Status = "Approved";
            instance.Status = "Approved";

            // Act: Second approver tries to action the same step
            var action2 = new WorkflowActionRequest { Action = "Approve", Reason = "I also approve" };
            var result2 = await _engineService.ActOnStepAsync(instanceId, approver2, action2);

            // Assert: Approver 2 gets clear AlreadyActioned response
            Assert.False(result2.Success);
            Assert.Equal("AlreadyActioned", result2.StatusCode);
            Assert.Contains("already", result2.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ==============================================================
        // 2. Permission without assignment is rejected
        // ==============================================================
        [Fact]
        public async Task ActOnStepAsync_PermissionWithoutAssignment_IsRejected()
        {
            // Arrange
            long instanceId = 2;
            long assignedApprover = 101;
            long unassignedUserWithPermission = 105;

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                RequestId = 11,
                RequesterEmployeeId = 50,
                CurrentStepOrder = 1,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var step = new WorkflowInstanceStep
            {
                Id = 20,
                WorkflowInstanceId = instanceId,
                StepOrder = 1,
                StepName = "Manager Approval",
                ApproverRuleType = "ReportingManager",
                Status = "Pending",
                EligibleApproverIdsJson = JsonSerializer.Serialize(new List<long> { assignedApprover }),
                IsActive = true,
                IsDeleted = false
            };

            _mockInstanceRepo.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });
            _mockDelegationRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowDelegation>());

            // User 105 holds LEAVE_APPROVE, but is NOT in EligibleApproverIdsJson!
            _mockPermissionService.Setup(p => p.HasPermissionAsync(unassignedUserWithPermission, "LEAVE_APPROVE")).ReturnsAsync(true);

            // Act
            var request = new WorkflowActionRequest { Action = "Approve", Reason = "Trying to approve without assignment" };
            var result = await _engineService.ActOnStepAsync(instanceId, unassignedUserWithPermission, request);

            // Assert: Backend rejects with Forbidden and clear reason
            Assert.False(result.Success);
            Assert.Equal("Forbidden", result.StatusCode);
            Assert.Contains("not an assigned approver", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ==============================================================
        // 3. Override requires permission (REQUEST_OVERRIDE)
        // ==============================================================
        [Fact]
        public async Task ActOnStepAsync_OverrideWithoutPermission_ReturnsForbidden()
        {
            // Arrange
            long instanceId = 3;
            long unauthorizedUser = 201;

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                CurrentStepOrder = 1,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var step = new WorkflowInstanceStep
            {
                Id = 30,
                WorkflowInstanceId = instanceId,
                StepOrder = 1,
                Status = "Pending",
                EligibleApproverIdsJson = "[101]",
                IsActive = true,
                IsDeleted = false
            };

            _mockInstanceRepo.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });
            // Does NOT have REQUEST_OVERRIDE permission
            _mockPermissionService.Setup(p => p.HasPermissionAsync(unauthorizedUser, "REQUEST_OVERRIDE")).ReturnsAsync(false);

            // Act
            var request = new WorkflowActionRequest { Action = "OverrideApprove", Reason = "Emergency" };
            var result = await _engineService.ActOnStepAsync(instanceId, unauthorizedUser, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Forbidden", result.StatusCode);
            Assert.Contains("REQUEST_OVERRIDE", result.Message);
        }

        // ==============================================================
        // 4. Override requires mandatory reason
        // ==============================================================
        [Fact]
        public async Task ActOnStepAsync_OverrideWithPermissionButMissingReason_ReturnsBadRequest()
        {
            // Arrange
            long instanceId = 4;
            long authorizedAdmin = 200;

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                CurrentStepOrder = 1,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var step = new WorkflowInstanceStep
            {
                Id = 40,
                WorkflowInstanceId = instanceId,
                StepOrder = 1,
                Status = "Pending",
                EligibleApproverIdsJson = "[101]",
                IsActive = true,
                IsDeleted = false
            };

            _mockInstanceRepo.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });
            _mockPermissionService.Setup(p => p.HasPermissionAsync(authorizedAdmin, "REQUEST_OVERRIDE")).ReturnsAsync(true);

            // Act: Missing reason
            var request = new WorkflowActionRequest { Action = "OverrideApprove", Reason = "" };
            var result = await _engineService.ActOnStepAsync(instanceId, authorizedAdmin, request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("BadRequest", result.StatusCode);
            Assert.Contains("mandatory override reason", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ==============================================================
        // 5. Override with permission and reason succeeds with Override capacity
        // ==============================================================
        [Fact]
        public async Task ActOnStepAsync_OverrideWithPermissionAndReason_SucceedsWithOverrideCapacity()
        {
            // Arrange
            long instanceId = 5;
            long authorizedAdmin = 200;

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                CurrentStepOrder = 1,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var step = new WorkflowInstanceStep
            {
                Id = 50,
                WorkflowInstanceId = instanceId,
                StepOrder = 1,
                StepName = "Department Head",
                Status = "Pending",
                EligibleApproverIdsJson = "[101]",
                IsActive = true,
                IsDeleted = false
            };

            _mockInstanceRepo.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });
            _mockPermissionService.Setup(p => p.HasPermissionAsync(authorizedAdmin, "REQUEST_OVERRIDE")).ReturnsAsync(true);
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Employee>
            {
                new Employee { Id = authorizedAdmin, FirstName = "Admin", LastName = "User", IsActive = true, IsDeleted = false }
            });

            // Act
            var request = new WorkflowActionRequest { Action = "OverrideApprove", Reason = "Dept head hospital emergency" };
            var result = await _engineService.ActOnStepAsync(instanceId, authorizedAdmin, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Override", result.ActionCapacity);
            Assert.Equal("Approved", result.WorkflowStatus);
            _mockAuditRepo.Verify(a => a.Add(It.Is<WorkflowAuditLog>(l => l.ActionCapacity == "Override" && l.EventType == "StepApproved")), Times.Once);
        }

        // ==============================================================
        // 6. Reassign moves pending step to target employee
        // ==============================================================
        [Fact]
        public async Task ActOnStepAsync_Reassign_MovesPendingStepToTargetEmployee()
        {
            // Arrange
            long instanceId = 6;
            long hrAdmin = 300;
            long originalApprover = 101;
            long targetApprover = 103;

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                RequesterEmployeeId = 50,
                CurrentStepOrder = 1,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var step = new WorkflowInstanceStep
            {
                Id = 60,
                WorkflowInstanceId = instanceId,
                StepOrder = 1,
                StepName = "Manager Approval",
                Status = "Pending",
                EligibleApproverIdsJson = JsonSerializer.Serialize(new List<long> { originalApprover }),
                ResolutionReason = "ReportingManager",
                IsActive = true,
                IsDeleted = false
            };

            _mockInstanceRepo.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });
            _mockPermissionService.Setup(p => p.HasPermissionAsync(hrAdmin, "REQUEST_REASSIGN")).ReturnsAsync(true);
            _mockEmpRepo.Setup(r => r.GetById(targetApprover)).ReturnsAsync(new Employee
            {
                Id = targetApprover,
                FirstName = "New",
                LastName = "Manager",
                IsActive = true,
                IsDeleted = false
            });

            // Act
            var request = new WorkflowActionRequest
            {
                Action = "Reassign",
                ReassignToEmployeeId = targetApprover,
                Reason = "Original manager on sabbatical"
            };
            var result = await _engineService.ActOnStepAsync(instanceId, hrAdmin, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Pending", result.WorkflowStatus);
            Assert.Equal("Reassign", result.ActionCapacity);

            // Step should now have targetApprover in EligibleApproverIdsJson
            var updatedEligible = JsonSerializer.Deserialize<List<long>>(step.EligibleApproverIdsJson);
            Assert.NotNull(updatedEligible);
            Assert.Contains(targetApprover, updatedEligible);
            Assert.DoesNotContain(originalApprover, updatedEligible);
            _mockAuditRepo.Verify(a => a.Add(It.Is<WorkflowAuditLog>(l => l.EventType == "StepReassigned" && l.ActionCapacity == "Reassign")), Times.Once);
        }

        // ==============================================================
        // 7. Active delegation records capacity as Delegate
        // ==============================================================
        [Fact]
        public async Task ActOnStepAsync_WhenActingAsActiveDelegate_RecordsCapacityAsDelegate()
        {
            // Arrange
            long instanceId = 7;
            long managerId = 101;
            long delegateId = 104;

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                CurrentStepOrder = 1,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var step = new WorkflowInstanceStep
            {
                Id = 70,
                WorkflowInstanceId = instanceId,
                StepOrder = 1,
                StepName = "Manager Approval",
                Status = "Pending",
                // Step was assigned to both or delegate was added
                EligibleApproverIdsJson = JsonSerializer.Serialize(new List<long> { managerId, delegateId }),
                IsActive = true,
                IsDeleted = false
            };

            var now = DateTime.UtcNow;
            var activeDelegation = new WorkflowDelegation
            {
                Id = 1,
                FromEmployeeId = managerId,
                ToEmployeeId = delegateId,
                StartDate = now.AddDays(-2),
                EndDate = now.AddDays(5),
                IsActive = true,
                IsDeleted = false
            };

            _mockInstanceRepo.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });
            _mockDelegationRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowDelegation> { activeDelegation });
            _mockPermissionService.Setup(p => p.HasPermissionAsync(delegateId, "LEAVE_APPROVE")).ReturnsAsync(true);
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<Employee>
            {
                new Employee { Id = delegateId, FirstName = "Delegate", LastName = "User", IsActive = true, IsDeleted = false }
            });

            // Act
            var request = new WorkflowActionRequest { Action = "Approve", Reason = "Approved on behalf of manager" };
            var result = await _engineService.ActOnStepAsync(instanceId, delegateId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Delegate", result.ActionCapacity);
            _mockAuditRepo.Verify(a => a.Add(It.Is<WorkflowAuditLog>(l => l.ActionCapacity == "Delegate" && l.EventType == "StepApproved")), Times.Once);
        }

        // ==============================================================
        // 8. Escalation moves overdue step per escalation target
        // ==============================================================
        [Fact]
        public async Task EscalateOverdueStepsAsync_WhenStepIsPastDue_ReassignsToEscalationTarget()
        {
            // Arrange
            long instanceId = 8;
            long originalApprover = 101;
            long escalationTargetManager = 102;

            var definition = new WorkflowDefinition
            {
                Id = 1,
                RequestType = "Leave",
                Title = "Leave Workflow",
                IsActive = true,
                IsDeleted = false
            };

            var defStep = new WorkflowStep
            {
                Id = 1,
                WorkflowDefinitionId = 1,
                StepOrder = 1,
                IsEscalationEnabled = true,
                EscalationTarget = "SkipLevelManager",
                IsActive = true,
                IsDeleted = false
            };

            var overdueStep = new WorkflowInstanceStep
            {
                Id = 80,
                WorkflowInstanceId = instanceId,
                StepOrder = 1,
                StepName = "Manager Approval",
                Status = "Pending",
                IsEscalated = false,
                DueTime = DateTime.UtcNow.AddHours(-2), // 2 hours overdue!
                EligibleApproverIdsJson = JsonSerializer.Serialize(new List<long> { originalApprover }),
                IsActive = true,
                IsDeleted = false
            };

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                RequestId = 10,
                RequesterEmployeeId = 50,
                WorkflowDefinitionId = 1,
                CurrentStepOrder = 1,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var employees = new List<Employee>
            {
                new Employee { Id = 50, FirstName = "Requester", LastName = "Emp", ManagerId = originalApprover, IsActive = true, IsDeleted = false },
                new Employee { Id = originalApprover, FirstName = "Original", LastName = "Approver", ManagerId = escalationTargetManager, IsActive = true, IsDeleted = false },
                new Employee { Id = escalationTargetManager, FirstName = "Escalated", LastName = "Manager", IsActive = true, IsDeleted = false }
            };

            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { overdueStep });
            _mockInstanceRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstance> { instance });
            _mockDefRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowDefinition> { definition });
            _mockStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowStep> { defStep });
            _mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);

            // Act
            await _engineService.EscalateOverdueStepsAsync();

            // Assert: Step should now be marked as escalated
            Assert.True(overdueStep.IsEscalated);
            var newEligible = JsonSerializer.Deserialize<List<long>>(overdueStep.EligibleApproverIdsJson);
            Assert.NotNull(newEligible);
            Assert.Contains(escalationTargetManager, newEligible);
            _mockAuditRepo.Verify(a => a.Add(It.Is<WorkflowAuditLog>(l => l.EventType == "StepEscalated" && l.ActionCapacity == "System")), Times.Once);
        }

        // ==============================================================
        // 9. Requester can cancel pending workflow; non-requester cannot
        // ==============================================================
        [Fact]
        public async Task CancelWorkflowAsync_RequesterCanCancel_NonRequesterCannot()
        {
            // Arrange
            long instanceId = 9;
            long requesterId = 50;
            long otherUser = 99;

            var instance = new WorkflowInstance
            {
                Id = instanceId,
                RequestType = "Leave",
                RequesterEmployeeId = requesterId,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            var step = new WorkflowInstanceStep
            {
                Id = 90,
                WorkflowInstanceId = instanceId,
                Status = "Pending",
                IsActive = true,
                IsDeleted = false
            };

            _mockInstanceRepo.Setup(r => r.GetById(instanceId)).ReturnsAsync(instance);
            _mockInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });

            // Act & Assert 1: Non-requester throws UnauthorizedAccessException
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await _engineService.CancelWorkflowAsync(instanceId, otherUser, "Cancelling someone else's request");
            });

            // Act 2: Requester cancels
            bool success = await _engineService.CancelWorkflowAsync(instanceId, requesterId, "Plans changed");

            // Assert 2: Succeeded, status is Cancelled
            Assert.True(success);
            Assert.Equal("Cancelled", instance.Status);
            Assert.Equal("Cancelled", step.Status);
            _mockAuditRepo.Verify(a => a.Add(It.Is<WorkflowAuditLog>(l => l.EventType == "WorkflowCancelled")), Times.Once);
        }

        // ==============================================================
        // 10. StartWorkflowAsync creates instance and snapshots resolved steps
        // ==============================================================
        [Fact]
        public async Task StartWorkflowAsync_CreatesInstanceAndPendingStepSnapshots()
        {
            // Arrange
            string requestType = "Leave";
            long requestId = 20;
            long requesterId = 50;
            var context = new WorkflowContext { RequesterId = requesterId, RequestType = requestType };

            var definition = new WorkflowDefinition
            {
                Id = 1,
                RequestType = requestType,
                Title = "Leave Standard",
                Version = 1,
                IsActive = true,
                IsDeleted = false
            };

            var resolvedSteps = new List<ResolvedStepApprovers>
            {
                new ResolvedStepApprovers
                {
                    StepOrder = 1,
                    StepName = "Manager Approval",
                    ApproverRuleType = "ReportingManager",
                    StepMode = "AnyOne",
                    ConditionSatisfied = true,
                    Approvers = new List<ResolvedApproverItem>
                    {
                        new ResolvedApproverItem { EmployeeId = 101, EmployeeName = "Manager John", ResolutionReason = "Direct Manager" }
                    }
                },
                new ResolvedStepApprovers
                {
                    StepOrder = 2,
                    StepName = "HR Review",
                    ApproverRuleType = "GroupQueue",
                    StepMode = "AnyOne",
                    ConditionSatisfied = true,
                    Approvers = new List<ResolvedApproverItem>
                    {
                        new ResolvedApproverItem { EmployeeId = 201, EmployeeName = "HR Alice", ResolutionReason = "HR Queue" }
                    }
                }
            };

            _mockResolver.Setup(r => r.ResolveDefinitionAsync(requestType, context)).ReturnsAsync(definition);
            _mockResolver.Setup(r => r.ResolveStepsAsync(requestType, requesterId, context, definition)).ReturnsAsync(resolvedSteps);
            _mockEmpRepo.Setup(r => r.GetById(requesterId)).ReturnsAsync(new Employee
            {
                Id = requesterId,
                FirstName = "John",
                LastName = "Doe",
                IsActive = true,
                IsDeleted = false
            });

            // Act
            var createdInstance = await _engineService.StartWorkflowAsync(requestType, requestId, requesterId, context);

            // Assert
            Assert.NotNull(createdInstance);
            Assert.Equal("Pending", createdInstance.Status);
            Assert.Equal(1, createdInstance.CurrentStepOrder);
            Assert.Equal(1, createdInstance.DefinitionVersion);

            _mockInstanceRepo.Verify(r => r.Add(It.IsAny<WorkflowInstance>()), Times.Once);
            _mockInstanceStepRepo.Verify(r => r.Add(It.Is<WorkflowInstanceStep>(s => s.StepOrder == 1 && s.Status == "Pending")), Times.Once);
            _mockInstanceStepRepo.Verify(r => r.Add(It.Is<WorkflowInstanceStep>(s => s.StepOrder == 2 && s.Status == "Waiting")), Times.Once);
            _mockAuditRepo.Verify(a => a.Add(It.Is<WorkflowAuditLog>(l => l.EventType == "WorkflowStarted")), Times.Once);
        }
    }
}
