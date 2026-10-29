using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using KHRMS.Services.Workflow.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class LeaveWorkflowIntegrationTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ILeaveRequestRepository> _mockLeaveRequestRepo;
        private readonly Mock<ILeaveRepository> _mockLeaveTypeRepo;
        private readonly Mock<IHolidayRepository> _mockHolidayRepo;
        private readonly Mock<IEmployeeRepository> _mockEmpRepo;
        private readonly Mock<IDesignationRepository> _mockDesignationRepo;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly Mock<ISendEmailService> _mockEmailService;
        private readonly Mock<IUserContextService> _mockUserContext;
        private readonly Mock<IHttpContextAccessor> _mockHttpAccessor;
        private readonly Mock<IWorkflowEngineService> _mockWorkflowEngine;
        private readonly Mock<IWorkflowInstanceRepository> _mockWorkflowInstanceRepo;
        private readonly Mock<IWorkflowInstanceStepRepository> _mockWorkflowInstanceStepRepo;

        public LeaveWorkflowIntegrationTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockLeaveRequestRepo = new Mock<ILeaveRequestRepository>();
            _mockLeaveTypeRepo = new Mock<ILeaveRepository>();
            _mockHolidayRepo = new Mock<IHolidayRepository>();
            _mockEmpRepo = new Mock<IEmployeeRepository>();
            _mockDesignationRepo = new Mock<IDesignationRepository>();
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _mockWorkflowInstanceRepo = new Mock<IWorkflowInstanceRepository>();
            _mockWorkflowInstanceStepRepo = new Mock<IWorkflowInstanceStepRepository>();

            _mockUow.Setup(u => u.LeaveRequest).Returns(_mockLeaveRequestRepo.Object);
            _mockUow.Setup(u => u.LeaveType).Returns(_mockLeaveTypeRepo.Object);
            _mockUow.Setup(u => u.Holidays).Returns(_mockHolidayRepo.Object);
            _mockUow.Setup(u => u.Employees).Returns(_mockEmpRepo.Object);
            _mockUow.Setup(u => u.Designations).Returns(_mockDesignationRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotificationRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstances).Returns(_mockWorkflowInstanceRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstanceSteps).Returns(_mockWorkflowInstanceStepRepo.Object);
            _mockUow.Setup(u => u.Save()).Returns(1);

            _mockWorkflowInstanceRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstance>());
            _mockWorkflowInstanceStepRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep>());

            _mockEmailService = new Mock<ISendEmailService>();
            _mockUserContext = new Mock<IUserContextService>();
            _mockHttpAccessor = new Mock<IHttpContextAccessor>();
            _mockWorkflowEngine = new Mock<IWorkflowEngineService>();

            // Defaults: holidays empty, no existing leaves
            _mockHolidayRepo.Setup(h => h.GetAll()).ReturnsAsync(new List<Holiday>());
            _mockLeaveRequestRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<LeaveRequest>());
            _mockLeaveTypeRepo.Setup(lt => lt.GetById(It.IsAny<int>())).ReturnsAsync(new LeaveType { Id = 1, Type = "Casual Leave" });
            _mockEmpRepo.Setup(e => e.GetById(It.IsAny<long>())).ReturnsAsync(new Employee
            {
                Id = 10,
                FirstName = "Alice",
                LastName = "Smith",
                EmailAddress = "alice@example.com",
                ManagerId = 20,
                Branch = "Headquarters"
            });
            _mockEmpRepo.Setup(e => e.GetAll()).ReturnsAsync(new List<Employee>
            {
                new Employee { Id = 10, FirstName = "Alice", LastName = "Smith", EmailAddress = "alice@example.com", ManagerId = 20 },
                new Employee { Id = 20, FirstName = "Bob", LastName = "Manager", EmailAddress = "bob@example.com" }
            });

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(10);
            _mockUserContext.Setup(u => u.IsAdmin()).Returns(false);
            _mockUserContext.Setup(u => u.IsHR()).Returns(false);
        }

        private IConfiguration CreateConfig(bool isLeaveWorkflowEnabled)
        {
            var dict = new Dictionary<string, string?>
            {
                { "WorkflowEngine:Modules:Leave", isLeaveWorkflowEnabled ? "true" : "false" }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        }

        // ==============================================================
        // 1. AddLeaveRequest with Feature Flag ENABLED starts workflow
        // ==============================================================
        [Fact]
        public async Task AddLeaveRequest_WhenWorkflowFeatureFlagEnabled_StartsWorkflowAndSetsInstanceId()
        {
            // Arrange
            var config = CreateConfig(isLeaveWorkflowEnabled: true);
            var service = new LeaveRequestTypeService(
                _mockUow.Object,
                _mockEmailService.Object,
                _mockUserContext.Object,
                _mockHttpAccessor.Object,
                _mockWorkflowEngine.Object,
                config
            );

            // Upcoming Monday to Tuesday (working days)
            var start = DateTime.Today.AddDays(7);
            while (start.DayOfWeek != DayOfWeek.Monday) start = start.AddDays(1);
            var end = start.AddDays(1);

            var leaveInput = new LeaveRequest
            {
                EmployeeId = 10,
                LeaveTypeId = 1,
                StartDate = start,
                EndDate = end,
                LeaveReason = "Attending family function",
                LeaveMode = "Full Day"
            };

            LeaveRequest? capturedSavedLeave = null;
            _mockLeaveRequestRepo.Setup(r => r.Add(It.IsAny<LeaveRequest>()))
                .Callback<LeaveRequest>(lr =>
                {
                    lr.Id = 101;
                    capturedSavedLeave = lr;
                })
                .Returns(Task.CompletedTask);

            _mockWorkflowEngine.Setup(w => w.StartWorkflowAsync(
                "Leave",
                101,
                10,
                It.IsAny<WorkflowContext>()
            )).ReturnsAsync(new WorkflowInstance
            {
                Id = 555,
                RequestType = "Leave",
                RequestId = 101,
                RequesterEmployeeId = 10,
                Status = "Pending"
            });

            // Act
            var result = await service.AddLeaveRequestType(leaveInput);

            // Assert
            Assert.True(result);
            _mockWorkflowEngine.Verify(w => w.StartWorkflowAsync("Leave", 101, 10, It.IsAny<WorkflowContext>()), Times.Once);
            Assert.NotNull(capturedSavedLeave);
            Assert.Equal(555, capturedSavedLeave.WorkflowInstanceId);
        }

        // ==============================================================
        // 2. AddLeaveRequest with Feature Flag DISABLED uses legacy path
        // ==============================================================
        [Fact]
        public async Task AddLeaveRequest_WhenWorkflowFeatureFlagDisabled_FallsBackToLegacyAndDoesNotStartWorkflow()
        {
            // Arrange
            var config = CreateConfig(isLeaveWorkflowEnabled: false);
            var service = new LeaveRequestTypeService(
                _mockUow.Object,
                _mockEmailService.Object,
                _mockUserContext.Object,
                _mockHttpAccessor.Object,
                _mockWorkflowEngine.Object,
                config
            );

            var start = DateTime.Today.AddDays(7);
            while (start.DayOfWeek != DayOfWeek.Monday) start = start.AddDays(1);
            var end = start.AddDays(1);

            var leaveInput = new LeaveRequest
            {
                EmployeeId = 10,
                LeaveTypeId = 1,
                StartDate = start,
                EndDate = end,
                LeaveReason = "Personal work",
                LeaveMode = "Full Day"
            };

            LeaveRequest? capturedSavedLeave = null;
            _mockLeaveRequestRepo.Setup(r => r.Add(It.IsAny<LeaveRequest>()))
                .Callback<LeaveRequest>(lr =>
                {
                    lr.Id = 102;
                    capturedSavedLeave = lr;
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await service.AddLeaveRequestType(leaveInput);

            // Assert
            Assert.True(result);
            // Workflow engine should NOT be invoked
            _mockWorkflowEngine.Verify(w => w.StartWorkflowAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<WorkflowContext>()), Times.Never);
            Assert.NotNull(capturedSavedLeave);
            Assert.Null(capturedSavedLeave.WorkflowInstanceId);
        }

        // ==============================================================
        // 3. ApproveLeaveRequestAsync delegates to Workflow Engine when InstanceId present
        // ==============================================================
        [Fact]
        public async Task ApproveLeaveRequestAsync_WhenWorkflowInstanceIdPresent_DelegatesToWorkflowEngine()
        {
            // Arrange
            var config = CreateConfig(isLeaveWorkflowEnabled: true);
            var service = new LeaveRequestTypeService(
                _mockUow.Object,
                _mockEmailService.Object,
                _mockUserContext.Object,
                _mockHttpAccessor.Object,
                _mockWorkflowEngine.Object,
                config
            );

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(20); // Approver is Bob (Manager)

            var existingLeave = new LeaveRequest
            {
                Id = 101,
                EmployeeId = 10,
                WorkflowInstanceId = 888,
                Status = "Pending",
                StartDate = DateTime.Today.AddDays(3),
                EndDate = DateTime.Today.AddDays(4)
            };
            _mockLeaveRequestRepo.Setup(r => r.GetById(101)).ReturnsAsync(existingLeave);

            _mockWorkflowEngine.Setup(w => w.ActOnStepAsync(
                888,
                20,
                It.Is<WorkflowActionRequest>(a => a.Action == "Approve")
            )).ReturnsAsync(new WorkflowActionResult { Success = true, Message = "Approved successfully", WorkflowStatus = "Approved", IsWorkflowCompleted = true });

            // Act
            var approveDto = new ApproveLeaveRequest
            {
                Id = 101,
                Status = "Approved"
            };
            var result = await service.ApproveLeaveRequestAsync(approveDto);

            // Assert
            Assert.True(result);
            _mockWorkflowEngine.Verify(w => w.ActOnStepAsync(888, 20, It.Is<WorkflowActionRequest>(a => a.Action == "Approve")), Times.Once);
        }

        // ==============================================================
        // 4. RejectLeaveRequestAsync delegates to Workflow Engine with reason
        // ==============================================================
        [Fact]
        public async Task RejectLeaveRequestAsync_WhenWorkflowInstanceIdPresent_DelegatesToWorkflowEngineWithReason()
        {
            // Arrange
            var config = CreateConfig(isLeaveWorkflowEnabled: true);
            var service = new LeaveRequestTypeService(
                _mockUow.Object,
                _mockEmailService.Object,
                _mockUserContext.Object,
                _mockHttpAccessor.Object,
                _mockWorkflowEngine.Object,
                config
            );

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(20);

            var existingLeave = new LeaveRequest
            {
                Id = 101,
                EmployeeId = 10,
                WorkflowInstanceId = 888,
                Status = "Pending",
                StartDate = DateTime.Today.AddDays(3),
                EndDate = DateTime.Today.AddDays(4)
            };
            _mockLeaveRequestRepo.Setup(r => r.GetById(101)).ReturnsAsync(existingLeave);

            _mockWorkflowEngine.Setup(w => w.ActOnStepAsync(
                888,
                20,
                It.Is<WorkflowActionRequest>(a => a.Action == "Reject" && a.Reason == "Project critical deadline")
            )).ReturnsAsync(new WorkflowActionResult { Success = true, Message = "Rejected successfully", WorkflowStatus = "Rejected", IsWorkflowCompleted = true });

            // Act
            var rejectDto = new ApproveLeaveRequest
            {
                Id = 101,
                Status = "Rejected",
                RejectionReason = "Project critical deadline"
            };
            var result = await service.ApproveLeaveRequestAsync(rejectDto);

            // Assert
            Assert.True(result);
            _mockWorkflowEngine.Verify(w => w.ActOnStepAsync(888, 20, It.Is<WorkflowActionRequest>(a => a.Action == "Reject" && a.Reason == "Project critical deadline")), Times.Once);
        }

        // ==============================================================
        // 5. CancelLeaveRequestAsync delegates to Workflow Engine Cancel
        // ==============================================================
        [Fact]
        public async Task CancelLeaveRequestAsync_WhenWorkflowInstanceIdPresent_DelegatesToWorkflowEngineCancel()
        {
            // Arrange
            var config = CreateConfig(isLeaveWorkflowEnabled: true);
            var service = new LeaveRequestTypeService(
                _mockUow.Object,
                _mockEmailService.Object,
                _mockUserContext.Object,
                _mockHttpAccessor.Object,
                _mockWorkflowEngine.Object,
                config
            );

            var existingLeave = new LeaveRequest
            {
                Id = 101,
                EmployeeId = 10,
                WorkflowInstanceId = 888,
                Status = "Pending",
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(6)
            };
            _mockLeaveRequestRepo.Setup(r => r.GetById(101)).ReturnsAsync(existingLeave);

            _mockWorkflowEngine.Setup(w => w.CancelWorkflowAsync(
                888,
                10,
                "Change of travel plans"
            )).ReturnsAsync(true);

            // Act
            var result = await service.CancelLeaveRequestAsync(101, "Change of travel plans", 10, isAdminOrManager: false);

            // Assert
            Assert.True(result);
            _mockWorkflowEngine.Verify(w => w.CancelWorkflowAsync(888, 10, "Change of travel plans"), Times.Once);
        }

        // ==============================================================
        // 6. Sensitive Medical Details are Masked for Viewers Outside Decision Chain
        // ==============================================================
        [Fact]
        public async Task SensitiveDetails_WhenViewerOutsideDecisionChain_MasksSickReason()
        {
            // Arrange
            var mockDefRepo = new Mock<IWorkflowDefinitionRepository>();
            var mockStepRepo = new Mock<IWorkflowStepRepository>();
            var mockInstanceRepo = new Mock<IWorkflowInstanceRepository>();
            var mockInstanceStepRepo = new Mock<IWorkflowInstanceStepRepository>();
            var mockDelegationRepo = new Mock<IWorkflowDelegationRepository>();
            var mockAuditRepo = new Mock<IWorkflowAuditLogRepository>();
            var mockResolver = new Mock<IApproverResolver>();
            var mockPermissionService = new Mock<IPermissionService>();
            var mockConditionEvaluator = new Mock<IConditionEvaluator>();

            _mockUow.Setup(u => u.WorkflowInstances).Returns(mockInstanceRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstanceSteps).Returns(mockInstanceStepRepo.Object);
            _mockUow.Setup(u => u.WorkflowDefinitions).Returns(mockDefRepo.Object);
            _mockUow.Setup(u => u.WorkflowSteps).Returns(mockStepRepo.Object);
            _mockUow.Setup(u => u.WorkflowDelegations).Returns(mockDelegationRepo.Object);
            _mockUow.Setup(u => u.WorkflowAuditLogs).Returns(mockAuditRepo.Object);

            var engineService = new WorkflowEngineService(
                _mockUow.Object,
                mockResolver.Object,
                mockPermissionService.Object,
                mockConditionEvaluator.Object
            );

            // Leave request #101: Sick leave with confidential medical reason
            var leaves = new List<LeaveRequest>
            {
                new LeaveRequest
                {
                    Id = 101,
                    EmployeeId = 10, // Alice
                    WorkflowInstanceId = 999,
                    LeaveReason = "Sensitive surgery recovery at hospital",
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = "Pending"
                }
            };
            _mockLeaveRequestRepo.Setup(r => r.GetAll()).ReturnsAsync(leaves);
            _mockLeaveRequestRepo.Setup(r => r.GetById(101)).ReturnsAsync(leaves[0]);
            _mockLeaveTypeRepo.Setup(lt => lt.GetAll()).ReturnsAsync(new List<LeaveType>
            {
                new LeaveType { Id = 2, Type = "Sick Leave" }
            });
            _mockLeaveTypeRepo.Setup(lt => lt.GetById(It.IsAny<int>())).ReturnsAsync(new LeaveType { Id = 2, Type = "Sick Leave" });

            var instance = new WorkflowInstance
            {
                Id = 999,
                RequestType = "Leave",
                RequestId = 101,
                RequesterEmployeeId = 10,
                Status = "Pending",
                CurrentStepOrder = 1,
                IsActive = true,
                IsDeleted = false
            };
            mockInstanceRepo.Setup(r => r.GetById(999)).ReturnsAsync(instance);

            var step = new WorkflowInstanceStep
            {
                Id = 1,
                WorkflowInstanceId = 999,
                StepOrder = 1,
                Status = "Pending",
                EligibleApproverIdsJson = "[20]", // Approver is Bob
                IsActive = true,
                IsDeleted = false
            };
            mockInstanceStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });
            mockAuditRepo.Setup(a => a.GetAll()).ReturnsAsync(new List<WorkflowAuditLog>());

            // Viewer 99 is neither Alice (10) nor Bob (20), and has no REQUEST_OVERRIDE permission
            mockPermissionService.Setup(p => p.HasPermissionAsync(99, "REQUEST_OVERRIDE")).ReturnsAsync(false);

            // Act
            var details = await engineService.GetInstanceDetailsAsync(999, 99);

            // Assert
            Assert.NotNull(details);
            var reason = details.PayloadSummary["LeaveReason"]?.ToString();
            Assert.Contains("[Confidential - Medical details restricted to decision chain]", reason);
            Assert.DoesNotContain("surgery recovery", reason);
        }

        // ==============================================================
        // 7. Sensitive Medical Details are UNMASKED for Viewer Inside Decision Chain
        // ==============================================================
        [Fact]
        public async Task SensitiveDetails_WhenViewerInsideDecisionChain_ReasonIsVisible()
        {
            // Arrange
            var mockDefRepo = new Mock<IWorkflowDefinitionRepository>();
            var mockStepRepo = new Mock<IWorkflowStepRepository>();
            var mockInstanceRepo = new Mock<IWorkflowInstanceRepository>();
            var mockInstanceStepRepo = new Mock<IWorkflowInstanceStepRepository>();
            var mockDelegationRepo = new Mock<IWorkflowDelegationRepository>();
            var mockAuditRepo = new Mock<IWorkflowAuditLogRepository>();
            var mockResolver = new Mock<IApproverResolver>();
            var mockPermissionService = new Mock<IPermissionService>();
            var mockConditionEvaluator = new Mock<IConditionEvaluator>();

            _mockUow.Setup(u => u.WorkflowInstances).Returns(mockInstanceRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstanceSteps).Returns(mockInstanceStepRepo.Object);
            _mockUow.Setup(u => u.WorkflowDefinitions).Returns(mockDefRepo.Object);
            _mockUow.Setup(u => u.WorkflowSteps).Returns(mockStepRepo.Object);
            _mockUow.Setup(u => u.WorkflowDelegations).Returns(mockDelegationRepo.Object);
            _mockUow.Setup(u => u.WorkflowAuditLogs).Returns(mockAuditRepo.Object);

            var engineService = new WorkflowEngineService(
                _mockUow.Object,
                mockResolver.Object,
                mockPermissionService.Object,
                mockConditionEvaluator.Object
            );

            var leaves = new List<LeaveRequest>
            {
                new LeaveRequest
                {
                    Id = 101,
                    EmployeeId = 10,
                    WorkflowInstanceId = 999,
                    LeaveReason = "Sensitive surgery recovery at hospital",
                    StartDate = DateTime.Today.AddDays(2),
                    EndDate = DateTime.Today.AddDays(5),
                    Status = "Pending"
                }
            };
            _mockLeaveRequestRepo.Setup(r => r.GetAll()).ReturnsAsync(leaves);
            _mockLeaveRequestRepo.Setup(r => r.GetById(101)).ReturnsAsync(leaves[0]);
            _mockLeaveTypeRepo.Setup(lt => lt.GetAll()).ReturnsAsync(new List<LeaveType>
            {
                new LeaveType { Id = 2, Type = "Sick Leave" }
            });
            _mockLeaveTypeRepo.Setup(lt => lt.GetById(It.IsAny<int>())).ReturnsAsync(new LeaveType { Id = 2, Type = "Sick Leave" });

            var instance = new WorkflowInstance
            {
                Id = 999,
                RequestType = "Leave",
                RequestId = 101,
                RequesterEmployeeId = 10,
                Status = "Pending",
                CurrentStepOrder = 1,
                IsActive = true,
                IsDeleted = false
            };
            mockInstanceRepo.Setup(r => r.GetById(999)).ReturnsAsync(instance);

            var step = new WorkflowInstanceStep
            {
                Id = 1,
                WorkflowInstanceId = 999,
                StepOrder = 1,
                Status = "Pending",
                EligibleApproverIdsJson = "[20]", // Approver is Bob
                IsActive = true,
                IsDeleted = false
            };
            mockInstanceStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });
            mockAuditRepo.Setup(a => a.GetAll()).ReturnsAsync(new List<WorkflowAuditLog>());

            // Approver Bob (20) is in the decision chain
            // Act
            var details = await engineService.GetInstanceDetailsAsync(999, 20);

            // Assert
            Assert.NotNull(details);
            var reason = details.PayloadSummary["LeaveReason"]?.ToString();
            Assert.Contains("Sensitive surgery recovery at hospital", reason);
        }

        // ==============================================================
        // 8. Fix A: Approver Visibility in GetAllEmployeesLeaveRequest
        // ==============================================================
        [Fact]
        public async Task GetAllEmployeesLeaveRequest_WhenViewerIsEligibleApproverButNotDirectManager_IncludesRequest()
        {
            // Arrange
            var config = CreateConfig(isLeaveWorkflowEnabled: true);
            var service = new LeaveRequestTypeService(
                _mockUow.Object,
                _mockEmailService.Object,
                _mockUserContext.Object,
                _mockHttpAccessor.Object,
                _mockWorkflowEngine.Object,
                config
            );

            // Viewer is Carol (Emp #30), not an Admin or HR, and not Alice's direct manager
            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(30);
            _mockUserContext.Setup(u => u.IsAdmin()).Returns(false);
            _mockUserContext.Setup(u => u.IsHR()).Returns(false);

            var employees = new List<Employee>
            {
                new Employee { Id = 10, FirstName = "Alice", LastName = "Smith", EmailAddress = "alice@example.com", ManagerId = 20 },
                new Employee { Id = 20, FirstName = "Bob", LastName = "Manager", EmailAddress = "bob@example.com" },
                new Employee { Id = 30, FirstName = "Carol", LastName = "DeptHead", EmailAddress = "carol@example.com" }
            };
            _mockEmpRepo.Setup(e => e.GetAll()).ReturnsAsync(employees);

            var leave = new LeaveRequest
            {
                Id = 101,
                EmployeeId = 10,
                LeaveTypeId = 1,
                WorkflowInstanceId = 500,
                Status = "Pending",
                StartDate = DateTime.Today.AddDays(2),
                EndDate = DateTime.Today.AddDays(3),
                IsActive = true,
                IsDeleted = false
            };
            _mockLeaveRequestRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<LeaveRequest> { leave });
            _mockLeaveTypeRepo.Setup(lt => lt.GetAll()).ReturnsAsync(new List<LeaveType> { new LeaveType { Id = 1, Type = "Casual Leave" } });

            var instance = new WorkflowInstance
            {
                Id = 500,
                RequestType = "Leave",
                RequestId = 101,
                RequesterEmployeeId = 10,
                Status = "Pending",
                CurrentStepOrder = 1,
                IsActive = true,
                IsDeleted = false
            };
            _mockWorkflowInstanceRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<WorkflowInstance> { instance });

            var step = new WorkflowInstanceStep
            {
                Id = 1,
                WorkflowInstanceId = 500,
                StepOrder = 1,
                Status = "Pending",
                EligibleApproverIdsJson = "[30]", // Carol is the approver
                IsActive = true,
                IsDeleted = false
            };
            _mockWorkflowInstanceStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });

            // Act
            var result = (await service.GetAllEmployeesLeaveRequest()).ToList();

            // Assert
            Assert.Single(result);
            var item = result[0];
            Assert.Equal(101, item.Id);
            Assert.True(item.CanApprove);
            Assert.Contains("Carol DeptHead", item.PendingWith);
        }

        // ==============================================================
        // 9. Fix B: Template Email Dispatched to Approvers on Submission
        // ==============================================================
        [Fact]
        public async Task AddLeaveRequestType_WhenWorkflowStarted_DispatchesTemplateEmailToEligibleApprovers()
        {
            // Arrange
            var config = CreateConfig(isLeaveWorkflowEnabled: true);
            var service = new LeaveRequestTypeService(
                _mockUow.Object,
                _mockEmailService.Object,
                _mockUserContext.Object,
                _mockHttpAccessor.Object,
                _mockWorkflowEngine.Object,
                config
            );

            var start = DateTime.Today.AddDays(7);
            while (start.DayOfWeek != DayOfWeek.Monday) start = start.AddDays(1);
            var end = start.AddDays(1);

            var leaveInput = new LeaveRequest
            {
                EmployeeId = 10,
                LeaveTypeId = 1,
                StartDate = start,
                EndDate = end,
                LeaveReason = "Conference attendance",
                LeaveMode = "Full Day"
            };

            _mockLeaveRequestRepo.Setup(r => r.Add(It.IsAny<LeaveRequest>()))
                .Callback<LeaveRequest>(lr => lr.Id = 201)
                .Returns(Task.CompletedTask);

            _mockWorkflowEngine.Setup(w => w.StartWorkflowAsync("Leave", 201, 10, It.IsAny<WorkflowContext>()))
                .ReturnsAsync(new WorkflowInstance
                {
                    Id = 600,
                    RequestType = "Leave",
                    RequestId = 201,
                    RequesterEmployeeId = 10,
                    Status = "Pending"
                });

            var step = new WorkflowInstanceStep
            {
                Id = 1,
                WorkflowInstanceId = 600,
                StepOrder = 1,
                Status = "Pending",
                EligibleApproverIdsJson = "[20]", // Bob
                IsActive = true,
                IsDeleted = false
            };
            _mockWorkflowInstanceStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });

            // Act
            var success = await service.AddLeaveRequestType(leaveInput);

            // Assert
            Assert.True(success);
            _mockEmailService.Verify(s => s.SendTemplateEmailAsync(
                "bob@example.com",
                It.Is<string>(sub => sub.Contains("Leave Request from Alice Smith")),
                It.Is<Dictionary<string, string>>(d => d["ManagerName"] == "Bob Manager" && d["LeaveDescription"] == "Conference attendance"),
                "LeaveRequest"
            ), Times.Once);
        }

        // ==============================================================
        // 10. Fix C: PendingWith Populated in GetAllLeaveRequestType
        // ==============================================================
        [Fact]
        public async Task GetAllLeaveRequestType_WhenWorkflowActive_PopulatesPendingWith()
        {
            // Arrange
            var config = CreateConfig(isLeaveWorkflowEnabled: true);
            var service = new LeaveRequestTypeService(
                _mockUow.Object,
                _mockEmailService.Object,
                _mockUserContext.Object,
                _mockHttpAccessor.Object,
                _mockWorkflowEngine.Object,
                config
            );

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(10); // Alice

            var leave = new LeaveRequest
            {
                Id = 301,
                EmployeeId = 10,
                LeaveTypeId = 1,
                WorkflowInstanceId = 700,
                Status = "Pending",
                StartDate = DateTime.Today.AddDays(5),
                EndDate = DateTime.Today.AddDays(6),
                LeaveMode = "Full Day",
                IsActive = true,
                IsDeleted = false
            };
            _mockLeaveRequestRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<LeaveRequest> { leave });
            _mockLeaveTypeRepo.Setup(lt => lt.GetAll()).ReturnsAsync(new List<LeaveType> { new LeaveType { Id = 1, Type = "Casual Leave" } });

            var step = new WorkflowInstanceStep
            {
                Id = 1,
                WorkflowInstanceId = 700,
                StepOrder = 1,
                Status = "Pending",
                EligibleApproverIdsJson = "[20]", // Bob
                IsActive = true,
                IsDeleted = false
            };
            _mockWorkflowInstanceStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep> { step });

            // Act
            var myLeaves = (await service.GetAllLeaveRequestType()).ToList();

            // Assert
            Assert.Single(myLeaves);
            var myLeave = myLeaves[0];
            Assert.Equal("Bob Manager", myLeave.PendingWith);
            Assert.True(myLeave.CanCancel);
            Assert.False(myLeave.CanApprove);
        }
    }
}
