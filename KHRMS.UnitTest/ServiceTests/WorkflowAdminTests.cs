using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using KHRMS.Services.Workflow.Services;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class WorkflowAdminTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IWorkflowDefinitionRepository> _mockDefRepo;
        private readonly Mock<IWorkflowStepRepository> _mockStepRepo;
        private readonly Mock<IWorkflowRoutingRuleRepository> _mockRoutingRepo;
        private readonly Mock<IWorkflowAuditLogRepository> _mockAuditRepo;
        private readonly Mock<IEmployeeRepository> _mockEmpRepo;
        private readonly Mock<IApproverResolver> _mockResolver;
        private readonly Mock<IConditionEvaluator> _mockConditionEvaluator;

        private readonly WorkflowAdminService _adminService;

        public WorkflowAdminTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockDefRepo = new Mock<IWorkflowDefinitionRepository>();
            _mockStepRepo = new Mock<IWorkflowStepRepository>();
            _mockRoutingRepo = new Mock<IWorkflowRoutingRuleRepository>();
            _mockAuditRepo = new Mock<IWorkflowAuditLogRepository>();
            _mockEmpRepo = new Mock<IEmployeeRepository>();

            _mockUow.Setup(u => u.WorkflowDefinitions).Returns(_mockDefRepo.Object);
            _mockUow.Setup(u => u.WorkflowSteps).Returns(_mockStepRepo.Object);
            _mockUow.Setup(u => u.WorkflowRoutingRules).Returns(_mockRoutingRepo.Object);
            _mockUow.Setup(u => u.WorkflowAuditLogs).Returns(_mockAuditRepo.Object);
            _mockUow.Setup(u => u.Employees).Returns(_mockEmpRepo.Object);
            _mockUow.Setup(u => u.Save()).Returns(1);

            _mockResolver = new Mock<IApproverResolver>();
            _mockConditionEvaluator = new Mock<IConditionEvaluator>();

            _adminService = new WorkflowAdminService(
                _mockUow.Object,
                _mockResolver.Object,
                _mockConditionEvaluator.Object
            );
        }

        // ==============================================================
        // 1. Validation: Every request type must have a default route
        // ==============================================================
        [Fact]
        public async Task ValidateDefinition_WhenNoDefaultRoute_ReturnsError()
        {
            // Arrange
            var dto = new WorkflowDefinitionSaveDTO
            {
                RequestType = "Leave",
                Title = "Leave Approval Workflow",
                Steps = new List<WorkflowStepConfigDTO>
                {
                    new() { StepOrder = 1, StepName = "Manager Approval", ApproverRuleType = "ReportingManager" }
                },
                RoutingRules = new List<WorkflowRoutingRuleConfigDTO>
                {
                    // Only department rule, missing Default fallback
                    new() { ScopeType = "Department", ScopeValue = "Engineering", Priority = 10, IsDefault = false }
                }
            };

            // Act
            var result = await _adminService.ValidateDefinitionAsync(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Default") || e.Contains("fallback"));
        }

        // ==============================================================
        // 2. Validation: Invalid ApproverRuleType returns error
        // ==============================================================
        [Fact]
        public async Task ValidateDefinition_WhenInvalidApproverRuleType_ReturnsError()
        {
            // Arrange
            var dto = new WorkflowDefinitionSaveDTO
            {
                RequestType = "Timesheet",
                Title = "Timesheet Approval",
                Steps = new List<WorkflowStepConfigDTO>
                {
                    new() { StepOrder = 1, StepName = "Invalid Step", ApproverRuleType = "MagicRule" }
                },
                RoutingRules = new List<WorkflowRoutingRuleConfigDTO>
                {
                    new() { ScopeType = "Default", Priority = 999, IsDefault = true }
                }
            };

            // Act
            var result = await _adminService.ValidateDefinitionAsync(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("Invalid or unsupported approver rule type"));
        }

        // ==============================================================
        // 3. Validation: SpecificPerson requires valid numeric employee id
        // ==============================================================
        [Fact]
        public async Task ValidateDefinition_WhenSpecificPersonWithoutValidId_ReturnsError()
        {
            // Arrange
            var dto = new WorkflowDefinitionSaveDTO
            {
                RequestType = "Leave",
                Title = "Leave Policy",
                Steps = new List<WorkflowStepConfigDTO>
                {
                    new() { StepOrder = 1, StepName = "CEO Signoff", ApproverRuleType = "SpecificPerson", RuleParameters = "" }
                },
                RoutingRules = new List<WorkflowRoutingRuleConfigDTO>
                {
                    new() { ScopeType = "Default", Priority = 999, IsDefault = true }
                }
            };

            // Act
            var result = await _adminService.ValidateDefinitionAsync(dto);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.Contains("SpecificPerson") && e.Contains("EmployeeId"));
        }

        // ==============================================================
        // 4. Validation: Warns on overlapping routing rules
        // ==============================================================
        [Fact]
        public async Task ValidateDefinition_WhenOverlappingRoutingRules_ReturnsWarning()
        {
            // Arrange
            var dto = new WorkflowDefinitionSaveDTO
            {
                RequestType = "Leave",
                Title = "Leave Policy",
                Steps = new List<WorkflowStepConfigDTO>
                {
                    new() { StepOrder = 1, StepName = "Manager Approval", ApproverRuleType = "ReportingManager" }
                },
                RoutingRules = new List<WorkflowRoutingRuleConfigDTO>
                {
                    new() { ScopeType = "Department", ScopeValue = "Sales", Priority = 10 },
                    new() { ScopeType = "Department", ScopeValue = "Sales", Priority = 20 }, // Duplicate scope
                    new() { ScopeType = "Default", Priority = 999, IsDefault = true }
                }
            };

            // Act
            var result = await _adminService.ValidateDefinitionAsync(dto);

            // Assert
            Assert.True(result.IsValid); // Still valid, but emits warning
            Assert.NotEmpty(result.Warnings);
            Assert.Contains(result.Warnings, w => w.Contains("Overlapping"));
        }

        // ==============================================================
        // 5. Save & Publish: Creates new version and deactivates previous
        // ==============================================================
        [Fact]
        public async Task SaveAndPublishDefinition_CreatesNewVersionAndDeactivatesPrevious()
        {
            // Arrange
            var existingV1 = new WorkflowDefinition
            {
                Id = 1,
                RequestType = "Leave",
                Version = 1,
                Title = "Old Policy",
                IsPublished = true,
                IsActive = true,
                IsDeleted = false
            };
            _mockDefRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowDefinition> { existingV1 });
            _mockStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowStep>());
            _mockRoutingRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowRoutingRule>());

            WorkflowDefinition? capturedNewDef = null;
            _mockDefRepo.Setup(r => r.Add(It.IsAny<WorkflowDefinition>()))
                .Callback<WorkflowDefinition>(d =>
                {
                    d.Id = 2;
                    capturedNewDef = d;
                })
                .Returns(Task.CompletedTask);

            var dto = new WorkflowDefinitionSaveDTO
            {
                RequestType = "Leave",
                Title = "Leave Policy v2",
                Steps = new List<WorkflowStepConfigDTO>
                {
                    new() { StepOrder = 1, StepName = "Manager Review", ApproverRuleType = "ReportingManager" },
                    new() { StepOrder = 2, StepName = "HR Approval", ApproverRuleType = "DepartmentHead" }
                },
                RoutingRules = new List<WorkflowRoutingRuleConfigDTO>
                {
                    new() { ScopeType = "Default", Priority = 999, IsDefault = true }
                }
            };

            // Act
            var saved = await _adminService.SaveAndPublishDefinitionAsync(dto, currentUserId: 99);

            // Assert
            Assert.NotNull(capturedNewDef);
            Assert.Equal(2, capturedNewDef.Version); // Version incremented to 2
            Assert.True(capturedNewDef.IsPublished);
            Assert.False(existingV1.IsPublished); // Old version unpublished
            _mockDefRepo.Verify(r => r.Update(existingV1), Times.Once);
            _mockAuditRepo.Verify(a => a.Add(It.Is<WorkflowAuditLog>(l => l.EventType == "PublishDefinition")), Times.Once);
        }

        // ==============================================================
        // 6. Rollback: Switches active published version
        // ==============================================================
        [Fact]
        public async Task RollbackVersion_SwitchesActivePublishedVersion()
        {
            // Arrange
            var v1 = new WorkflowDefinition { Id = 1, RequestType = "Leave", Version = 1, IsPublished = false, IsActive = true, IsDeleted = false };
            var v2 = new WorkflowDefinition { Id = 2, RequestType = "Leave", Version = 2, IsPublished = true, IsActive = true, IsDeleted = false };

            _mockDefRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowDefinition> { v1, v2 });

            // Act: Rollback to Version 1
            var success = await _adminService.RollbackVersionAsync("Leave", targetVersion: 1, currentUserId: 99);

            // Assert
            Assert.True(success);
            Assert.True(v1.IsPublished); // v1 is now active
            Assert.False(v2.IsPublished); // v2 is now inactive
            _mockDefRepo.Verify(r => r.Update(v1), Times.Once);
            _mockDefRepo.Verify(r => r.Update(v2), Times.Once);
            _mockAuditRepo.Verify(a => a.Add(It.Is<WorkflowAuditLog>(l => l.EventType == "RollbackDefinition")), Times.Once);
        }

        // ==============================================================
        // 7. Test Routing: Simulates step resolution and condition matching
        // ==============================================================
        [Fact]
        public async Task TestRouting_SimulatesStepResolutionAndConditions()
        {
            // Arrange
            long empId = 10;
            _mockEmpRepo.Setup(e => e.GetById(empId)).ReturnsAsync(new Employee
            {
                Id = empId,
                FirstName = "Alice",
                LastName = "Tester",
                Branch = "Main HQ"
            });
            _mockEmpRepo.Setup(e => e.GetAll()).ReturnsAsync(new List<Employee>
            {
                new Employee { Id = empId, FirstName = "Alice", LastName = "Tester" },
                new Employee { Id = 20, FirstName = "Bob", LastName = "Manager" }
            });

            var simulatedDefinition = new WorkflowDefinition
            {
                Id = 1,
                RequestType = "Leave",
                Version = 1,
                Title = "Leave Standard"
            };

            var simulatedSteps = new List<ResolvedStepApprovers>
            {
                new()
                {
                    StepOrder = 1,
                    StepName = "Manager Review",
                    ApproverRuleType = "ReportingManager",
                    StepMode = "AnyOne",
                    ConditionSatisfied = true,
                    ResolutionReason = "Resolved via direct ReportingManager rule",
                    Approvers = new List<ResolvedApproverItem>
                    {
                        new() { EmployeeId = 20, EmployeeName = "Bob Manager", Capacity = "Approver" }
                    }
                }
            };

            _mockResolver.Setup(r => r.ResolveDefinitionAsync("Leave", It.IsAny<WorkflowContext>()))
                .ReturnsAsync(simulatedDefinition);
            _mockResolver.Setup(r => r.ResolveStepsAsync("Leave", empId, It.IsAny<WorkflowContext>(), simulatedDefinition))
                .ReturnsAsync(simulatedSteps);

            var req = new TestRoutingRequestDTO
            {
                RequestType = "Leave",
                EmployeeId = empId,
                SampleContext = new Dictionary<string, object> { { "Days", 3 } }
            };

            // Act
            var result = await _adminService.TestRoutingAsync(req);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Steps);
            var step1 = result.Steps[0];
            Assert.Equal("Manager Review", step1.StepName);
            Assert.Equal("ReportingManager", step1.ApproverRuleType);
            Assert.True(step1.ConditionMet);
            Assert.Contains("Bob Manager", step1.EligibleApproverNames);
        }
    }
}
