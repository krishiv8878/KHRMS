using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class AttendanceWorkflowIntegrationTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IAttendanceRequestRepository> _mockAttRepo;
        private readonly Mock<IEmployeeAttendanceRepository> _mockEmpAttRepo;
        private readonly Mock<IAttendanceLogRepository> _mockAttLogRepo;
        private readonly Mock<IEmployeeRepository> _mockEmpRepo;
        private readonly Mock<IHolidayRepository> _mockHolidayRepo;
        private readonly Mock<ILeaveRequestRepository> _mockLeaveRepo;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly Mock<IWorkflowInstanceRepository> _mockWfInstanceRepo;
        private readonly Mock<IWorkflowInstanceStepRepository> _mockWfStepRepo;
        private readonly Mock<IEmployeeAttendanceService> _mockEmpAttService;
        private readonly Mock<IUserContextService> _mockUserContext;
        private readonly Mock<ISendEmailService> _mockEmailService;
        private readonly Mock<IAttendanceLogService> _mockAttLogService;
        private readonly Mock<IWorkflowEngineService> _mockWorkflowEngine;

        public AttendanceWorkflowIntegrationTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockAttRepo = new Mock<IAttendanceRequestRepository>();
            _mockEmpAttRepo = new Mock<IEmployeeAttendanceRepository>();
            _mockAttLogRepo = new Mock<IAttendanceLogRepository>();
            _mockEmpRepo = new Mock<IEmployeeRepository>();
            _mockHolidayRepo = new Mock<IHolidayRepository>();
            _mockLeaveRepo = new Mock<ILeaveRequestRepository>();
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _mockWfInstanceRepo = new Mock<IWorkflowInstanceRepository>();
            _mockWfStepRepo = new Mock<IWorkflowInstanceStepRepository>();

            _mockUow.Setup(u => u.AttendanceRequests).Returns(_mockAttRepo.Object);
            _mockUow.Setup(u => u.EmployeeAttendance).Returns(_mockEmpAttRepo.Object);
            _mockUow.Setup(u => u.AttendanceLog).Returns(_mockAttLogRepo.Object);
            _mockUow.Setup(u => u.Employees).Returns(_mockEmpRepo.Object);
            _mockUow.Setup(u => u.Holidays).Returns(_mockHolidayRepo.Object);
            _mockUow.Setup(u => u.LeaveRequest).Returns(_mockLeaveRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotificationRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstances).Returns(_mockWfInstanceRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstanceSteps).Returns(_mockWfStepRepo.Object);
            _mockUow.Setup(u => u.Save()).Returns(1);

            _mockEmpAttService = new Mock<IEmployeeAttendanceService>();
            _mockUserContext = new Mock<IUserContextService>();
            _mockEmailService = new Mock<ISendEmailService>();
            _mockAttLogService = new Mock<IAttendanceLogService>();
            _mockWorkflowEngine = new Mock<IWorkflowEngineService>();

            _mockHolidayRepo.Setup(h => h.GetAll()).ReturnsAsync(new List<Holiday>());
            _mockLeaveRepo.Setup(l => l.GetAll()).ReturnsAsync(new List<LeaveRequest>());
            _mockAttRepo.Setup(a => a.GetAll()).ReturnsAsync(new List<AttendanceRequest>());
            _mockEmpAttRepo.Setup(ea => ea.GetAll()).ReturnsAsync(new List<EmployeeAttendance>());
            _mockAttLogRepo.Setup(al => al.GetAll()).ReturnsAsync(new List<AttendanceLog>());
            _mockWfInstanceRepo.Setup(w => w.GetAll()).ReturnsAsync(new List<WorkflowInstance>());
            _mockWfStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep>());

            _mockEmpRepo.Setup(e => e.GetById(It.IsAny<long>())).ReturnsAsync(new Employee
            {
                Id = 10,
                FirstName = "Alice",
                LastName = "Smith",
                EmailAddress = "alice@example.com",
                ManagerId = 20,
                DateOfJoining = DateTime.UtcNow.AddYears(-1)
            });

            _mockEmpRepo.Setup(e => e.GetAll()).ReturnsAsync(new List<Employee>
            {
                new Employee { Id = 10, FirstName = "Alice", LastName = "Smith", EmailAddress = "alice@example.com", ManagerId = 20 },
                new Employee { Id = 20, FirstName = "Bob", LastName = "Manager", EmailAddress = "bob@example.com" }
            });

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(10);
            _mockUserContext.Setup(u => u.IsAdmin()).Returns(false);
            _mockUserContext.Setup(u => u.IsHR()).Returns(false);
            _mockUserContext.Setup(u => u.IsManager()).Returns(false);
        }

        private IConfiguration CreateConfig(bool isAttendanceWorkflowEnabled)
        {
            var dict = new Dictionary<string, string?>
            {
                { "WorkflowEngine:Modules:AttendanceRegularization", isAttendanceWorkflowEnabled ? "true" : "false" }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        }

        [Fact]
        public async Task AddAttendanceRequest_WhenWorkflowFeatureFlagEnabled_StartsWorkflowAndSetsInstanceId()
        {
            // Arrange
            var config = CreateConfig(isAttendanceWorkflowEnabled: true);
            var service = new AttendanceRequestService(
                _mockUow.Object,
                _mockEmpAttService.Object,
                _mockUserContext.Object,
                _mockEmailService.Object,
                _mockAttLogService.Object,
                _mockWorkflowEngine.Object,
                config);

            _mockWorkflowEngine
                .Setup(w => w.StartWorkflowAsync("AttendanceRegularization", It.IsAny<long>(), 10, It.IsAny<WorkflowContext>()))
                .ReturnsAsync(new WorkflowInstance { Id = 88, RequestType = "AttendanceRegularization", Status = "Pending" });

            var dto = new AttendanceRequestDTO
            {
                EmployeeId = 10,
                RequestType = "Regularization",
                RequestedDate = DateTime.Today.AddDays(-2),
                clockIn = DateTime.Today.AddDays(-2).AddHours(9),
                clockOut = DateTime.Today.AddDays(-2).AddHours(18),
                Reason = "Network connectivity issue prevented biometric clock-in"
            };

            // Act
            await service.AddAsync(dto, new ClaimsPrincipal());

            // Assert
            _mockWorkflowEngine.Verify(w => w.StartWorkflowAsync("AttendanceRegularization", It.IsAny<long>(), 10, It.IsAny<WorkflowContext>()), Times.Once);
            _mockAttRepo.Verify(a => a.Add(It.IsAny<AttendanceRequest>()), Times.Once);
            _mockAttRepo.Verify(a => a.Update(It.Is<AttendanceRequest>(r => r.WorkflowInstanceId == 88)), Times.Once);
        }

        [Fact]
        public async Task AddAttendanceRequest_WhenWorkflowFeatureFlagDisabled_DoesNotStartWorkflow()
        {
            // Arrange
            var config = CreateConfig(isAttendanceWorkflowEnabled: false);
            var service = new AttendanceRequestService(
                _mockUow.Object,
                _mockEmpAttService.Object,
                _mockUserContext.Object,
                _mockEmailService.Object,
                _mockAttLogService.Object,
                _mockWorkflowEngine.Object,
                config);

            var dto = new AttendanceRequestDTO
            {
                EmployeeId = 10,
                RequestType = "Regularization",
                RequestedDate = DateTime.Today.AddDays(-2),
                clockIn = DateTime.Today.AddDays(-2).AddHours(9),
                clockOut = DateTime.Today.AddDays(-2).AddHours(18),
                Reason = "Forgot punch"
            };

            // Act
            await service.AddAsync(dto, new ClaimsPrincipal());

            // Assert
            _mockWorkflowEngine.Verify(w => w.StartWorkflowAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<WorkflowContext>()), Times.Never);
            _mockAttRepo.Verify(a => a.Add(It.IsAny<AttendanceRequest>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAttendanceRequest_WithWorkflowInstanceId_RoutesThroughWorkflowEngine()
        {
            // Arrange
            var config = CreateConfig(isAttendanceWorkflowEnabled: true);
            var service = new AttendanceRequestService(
                _mockUow.Object,
                _mockEmpAttService.Object,
                _mockUserContext.Object,
                _mockEmailService.Object,
                _mockAttLogService.Object,
                _mockWorkflowEngine.Object,
                config);

            var existingRequest = new AttendanceRequest
            {
                Id = 55,
                EmployeeId = 10,
                ManagerId = 20,
                Status = "Pending",
                RequestedDate = DateTime.Today.AddDays(-3),
                ClockInTime = DateTime.Today.AddDays(-3).AddHours(9),
                ClockOutTime = DateTime.Today.AddDays(-3).AddHours(17.5),
                WorkflowInstanceId = 99
            };
            _mockAttRepo.Setup(a => a.GetById(55)).ReturnsAsync(existingRequest);

            _mockWorkflowEngine
                .Setup(w => w.ActOnStepAsync(99, 20, It.Is<WorkflowActionRequest>(r => r.Action == "Approve")))
                .ReturnsAsync(new WorkflowActionResult { Success = true, WorkflowStatus = "Approved" });

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(20);

            var updateDto = new AttendanceRequestUpdateDTO
            {
                Id = 55,
                Status = "Approved",
                ActionBy = 20
            };

            // Act
            await service.UpdateAsync(updateDto);

            // Assert
            _mockWorkflowEngine.Verify(w => w.ActOnStepAsync(99, 20, It.Is<WorkflowActionRequest>(r => r.Action == "Approve")), Times.Once);
        }

        [Fact]
        public async Task UpdateAttendanceRequest_WithoutWorkflowInstanceId_UsesLegacyDirectUpdate()
        {
            // Arrange
            var config = CreateConfig(isAttendanceWorkflowEnabled: true);
            var service = new AttendanceRequestService(
                _mockUow.Object,
                _mockEmpAttService.Object,
                _mockUserContext.Object,
                _mockEmailService.Object,
                _mockAttLogService.Object,
                _mockWorkflowEngine.Object,
                config);

            var existingRequest = new AttendanceRequest
            {
                Id = 60,
                EmployeeId = 10,
                ManagerId = 20,
                Status = "Pending",
                RequestedDate = DateTime.Today.AddDays(-3),
                ClockInTime = DateTime.Today.AddDays(-3).AddHours(9),
                ClockOutTime = DateTime.Today.AddDays(-3).AddHours(17.5),
                WorkflowInstanceId = null // Legacy record
            };
            _mockAttRepo.Setup(a => a.GetById(60)).ReturnsAsync(existingRequest);

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(20);

            var updateDto = new AttendanceRequestUpdateDTO
            {
                Id = 60,
                Status = "Approved",
                ActionBy = 20
            };

            // Act
            await service.UpdateAsync(updateDto);

            // Assert
            _mockWorkflowEngine.Verify(w => w.ActOnStepAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<WorkflowActionRequest>()), Times.Never);
            _mockAttRepo.Verify(a => a.Update(It.Is<AttendanceRequest>(r => r.Status == "Approved")), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_PopulatesWorkflowCapabilityFlagsAndPendingWith()
        {
            // Arrange
            var config = CreateConfig(isAttendanceWorkflowEnabled: true);
            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(20); // Manager Bob
            _mockUserContext.Setup(u => u.IsManager()).Returns(true);

            var service = new AttendanceRequestService(
                _mockUow.Object,
                _mockEmpAttService.Object,
                _mockUserContext.Object,
                _mockEmailService.Object,
                _mockAttLogService.Object,
                _mockWorkflowEngine.Object,
                config);

            var att1 = new AttendanceRequest
            {
                Id = 1,
                EmployeeId = 10,
                ManagerId = 20,
                Status = "Pending",
                RequestedDate = DateTime.Today.AddDays(-2),
                WorkflowInstanceId = 101
            };
            _mockAttRepo.Setup(a => a.GetAll()).ReturnsAsync(new List<AttendanceRequest> { att1 });

            _mockWfInstanceRepo.Setup(w => w.GetAll()).ReturnsAsync(new List<WorkflowInstance>
            {
                new WorkflowInstance { Id = 101, CurrentStepOrder = 1, Status = "Pending" }
            });

            _mockWfStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep>
            {
                new WorkflowInstanceStep
                {
                    WorkflowInstanceId = 101,
                    StepOrder = 1,
                    Status = "Pending",
                    EligibleApproverIdsJson = "[20]"
                }
            });

            // Act
            var results = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(results);
            var item = results[0];
            Assert.Equal(101, item.WorkflowInstanceId);
            Assert.True(item.CanApprove);
            Assert.Equal("Bob Manager", item.PendingWith);
        }

        [Fact]
        public async Task GetAllAsync_WhenViewerIsHR_AndWorkflowPendingWithManager_SetsCanApproveFalse()
        {
            // Arrange
            var config = CreateConfig(isAttendanceWorkflowEnabled: true);
            var service = new AttendanceRequestService(
                _mockUow.Object,
                _mockEmpAttService.Object,
                _mockUserContext.Object,
                _mockEmailService.Object,
                _mockAttLogService.Object,
                _mockWorkflowEngine.Object,
                config);

            // Current user is HR (Emp #10005)
            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(10005);
            _mockUserContext.Setup(u => u.IsAdmin()).Returns(false);
            _mockUserContext.Setup(u => u.IsHR()).Returns(true);
            _mockUserContext.Setup(u => u.HasPermissionAsync("REQUEST_OVERRIDE")).ReturnsAsync(false);

            var existingRequest = new AttendanceRequest
            {
                Id = 70,
                EmployeeId = 10, // Alice
                ManagerId = 20,  // Bob
                Status = "Pending",
                RequestedDate = DateTime.Today.AddDays(-2),
                WorkflowInstanceId = 202
            };
            _mockAttRepo.Setup(a => a.GetAll()).ReturnsAsync(new List<AttendanceRequest> { existingRequest });

            _mockWfInstanceRepo.Setup(i => i.GetAll()).ReturnsAsync(new List<WorkflowInstance>
            {
                new WorkflowInstance
                {
                    Id = 202,
                    RequestType = "AttendanceRegularization",
                    RequestId = 70,
                    RequesterEmployeeId = 10,
                    CurrentStepOrder = 1,
                    Status = "Pending"
                }
            });

            _mockWfStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep>
            {
                new WorkflowInstanceStep
                {
                    WorkflowInstanceId = 202,
                    StepOrder = 1,
                    Status = "Pending",
                    EligibleApproverIdsJson = "[20]" // Assigned to Bob only!
                }
            });

            // Act
            var results = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Single(results);
            var item = results[0];
            Assert.False(item.CanApprove, "HR must NOT have CanApprove=true when workflow is assigned to Reporting Manager.");
            Assert.Equal("Bob Manager", item.PendingWith);
        }

        [Fact]
        public async Task UpdateAsync_WhenWorkflowEngineRejectsUnauthorizedUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var config = CreateConfig(isAttendanceWorkflowEnabled: true);
            var service = new AttendanceRequestService(
                _mockUow.Object,
                _mockEmpAttService.Object,
                _mockUserContext.Object,
                _mockEmailService.Object,
                _mockAttLogService.Object,
                _mockWorkflowEngine.Object,
                config);

            // HR user (10005) attempting to approve
            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(10005);

            var existingRequest = new AttendanceRequest
            {
                Id = 75,
                EmployeeId = 10,
                ManagerId = 20,
                Status = "Pending",
                WorkflowInstanceId = 303
            };
            _mockAttRepo.Setup(a => a.GetById(75)).ReturnsAsync(existingRequest);

            // Workflow engine returns Forbidden
            _mockWorkflowEngine
                .Setup(w => w.ActOnStepAsync(303, 10005, It.IsAny<WorkflowActionRequest>()))
                .ReturnsAsync(new WorkflowActionResult
                {
                    Success = false,
                    StatusCode = "Forbidden",
                    Message = "Access Denied: You are not an assigned approver for this workflow step."
                });

            var updateDto = new AttendanceRequestUpdateDTO
            {
                Id = 75,
                Status = "Approved"
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(updateDto));
            Assert.Contains("Access Denied", ex.Message);
        }
    }
}
