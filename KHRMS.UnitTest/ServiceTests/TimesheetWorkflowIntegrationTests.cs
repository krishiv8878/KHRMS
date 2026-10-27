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
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class TimesheetWorkflowIntegrationTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<ITimesheetRepository> _mockTimesheetRepo;
        private readonly Mock<ITimesheetEntryRepository> _mockEntryRepo;
        private readonly Mock<IEmployeeRepository> _mockEmpRepo;
        private readonly Mock<IEmployeeAttendanceRepository> _mockAttRepo;
        private readonly Mock<IAttendanceRequestRepository> _mockAttReqRepo;
        private readonly Mock<IHolidayRepository> _mockHolidayRepo;
        private readonly Mock<ILeaveRequestRepository> _mockLeaveRepo;
        private readonly Mock<ILeaveRepository> _mockLeaveTypeRepo;
        private readonly Mock<IProjectMasterRepository> _mockProjectRepo;
        private readonly Mock<INotificationRepository> _mockNotificationRepo;
        private readonly Mock<IWorkflowInstanceRepository> _mockWfInstanceRepo;
        private readonly Mock<IWorkflowInstanceStepRepository> _mockWfStepRepo;
        private readonly Mock<IWorkflowEngineService> _mockWorkflowEngine;
        private readonly Mock<IUserContextService> _mockUserContext;

        public TimesheetWorkflowIntegrationTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockTimesheetRepo = new Mock<ITimesheetRepository>();
            _mockEntryRepo = new Mock<ITimesheetEntryRepository>();
            _mockEmpRepo = new Mock<IEmployeeRepository>();
            _mockAttRepo = new Mock<IEmployeeAttendanceRepository>();
            _mockAttReqRepo = new Mock<IAttendanceRequestRepository>();
            _mockHolidayRepo = new Mock<IHolidayRepository>();
            _mockLeaveRepo = new Mock<ILeaveRequestRepository>();
            _mockLeaveTypeRepo = new Mock<ILeaveRepository>();
            _mockProjectRepo = new Mock<IProjectMasterRepository>();
            _mockNotificationRepo = new Mock<INotificationRepository>();
            _mockWfInstanceRepo = new Mock<IWorkflowInstanceRepository>();
            _mockWfStepRepo = new Mock<IWorkflowInstanceStepRepository>();
            _mockWorkflowEngine = new Mock<IWorkflowEngineService>();
            _mockUserContext = new Mock<IUserContextService>();

            _mockUow.Setup(u => u.Timesheets).Returns(_mockTimesheetRepo.Object);
            _mockUow.Setup(u => u.TimesheetEntries).Returns(_mockEntryRepo.Object);
            _mockUow.Setup(u => u.Employees).Returns(_mockEmpRepo.Object);
            _mockUow.Setup(u => u.EmployeeAttendance).Returns(_mockAttRepo.Object);
            _mockUow.Setup(u => u.AttendanceRequests).Returns(_mockAttReqRepo.Object);
            _mockUow.Setup(u => u.Holidays).Returns(_mockHolidayRepo.Object);
            _mockUow.Setup(u => u.LeaveRequest).Returns(_mockLeaveRepo.Object);
            _mockUow.Setup(u => u.LeaveType).Returns(_mockLeaveTypeRepo.Object);
            _mockUow.Setup(u => u.ProjectMasters).Returns(_mockProjectRepo.Object);
            _mockUow.Setup(u => u.Notifications).Returns(_mockNotificationRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstances).Returns(_mockWfInstanceRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstanceSteps).Returns(_mockWfStepRepo.Object);
            _mockUow.Setup(u => u.Save()).Returns(1);

            _mockHolidayRepo.Setup(h => h.GetAll()).ReturnsAsync(new List<Holiday>());
            _mockLeaveRepo.Setup(l => l.GetAll()).ReturnsAsync(new List<LeaveRequest>());
            _mockLeaveTypeRepo.Setup(lt => lt.GetAll()).ReturnsAsync(new List<LeaveType>());
            _mockAttRepo.Setup(a => a.GetAll()).ReturnsAsync(new List<EmployeeAttendance>());
            _mockAttReqRepo.Setup(a => a.GetAll()).ReturnsAsync(new List<AttendanceRequest>());
            _mockTimesheetRepo.Setup(t => t.GetAll()).ReturnsAsync(new List<Timesheet>());
            _mockEntryRepo.Setup(e => e.GetAll()).ReturnsAsync(new List<TimesheetEntry>());
            _mockProjectRepo.Setup(p => p.GetAll()).ReturnsAsync(new List<ProjectMaster>());
            _mockWfInstanceRepo.Setup(w => w.GetAll()).ReturnsAsync(new List<WorkflowInstance>());
            _mockWfStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep>());

            _mockEmpRepo.Setup(e => e.GetById(It.IsAny<long>())).ReturnsAsync(new Employee
            {
                Id = 10,
                FirstName = "Alice",
                LastName = "Smith",
                EmailAddress = "alice@example.com",
                ManagerId = 20
            });

            _mockEmpRepo.Setup(e => e.GetAll()).ReturnsAsync(new List<Employee>
            {
                new Employee { Id = 10, FirstName = "Alice", LastName = "Smith", ManagerId = 20 },
                new Employee { Id = 20, FirstName = "Bob", LastName = "Manager" }
            });

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(10);
            _mockUserContext.Setup(u => u.IsAdmin()).Returns(false);
            _mockUserContext.Setup(u => u.IsHR()).Returns(false);
        }

        private IConfiguration CreateConfig(bool isTimesheetWorkflowEnabled)
        {
            var dict = new Dictionary<string, string?>
            {
                { "WorkflowEngine:Modules:Timesheet", isTimesheetWorkflowEnabled ? "true" : "false" }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        }

        [Fact]
        public async Task SubmitPeriodAsync_WhenWorkflowEnabled_StartsWorkflowAndSetsInstanceId()
        {
            // Arrange
            var config = CreateConfig(isTimesheetWorkflowEnabled: true);
            var service = new TimesheetService(
                _mockUow.Object,
                _mockWorkflowEngine.Object,
                config,
                _mockUserContext.Object);

            var startDate = DateTime.UtcNow.Date.AddDays(-7);
            var endDate = DateTime.UtcNow.Date.AddDays(-1);

            // Weekend entry so absent validation passes
            var weekendDate = startDate;
            while (weekendDate.DayOfWeek != DayOfWeek.Saturday && weekendDate <= endDate)
            {
                weekendDate = weekendDate.AddDays(1);
            }

            var entry = new TimesheetEntry
            {
                Id = 1,
                EmployeeId = 10,
                EntryDate = weekendDate,
                Hours = 8,
                TimesheetId = null
            };
            _mockEntryRepo.Setup(e => e.GetAll()).ReturnsAsync(new List<TimesheetEntry> { entry });

            _mockWorkflowEngine
                .Setup(w => w.StartWorkflowAsync("Timesheet", It.IsAny<long>(), 10, It.IsAny<WorkflowContext>()))
                .ReturnsAsync(new WorkflowInstance { Id = 202, RequestType = "Timesheet", Status = "Pending" });

            var dto = new TimesheetSubmitDTO
            {
                StartDate = startDate,
                EndDate = endDate,
                PeriodType = "Weekly"
            };

            // Act
            var result = await service.SubmitPeriodAsync(dto, 10);

            // Assert
            _mockWorkflowEngine.Verify(w => w.StartWorkflowAsync("Timesheet", It.IsAny<long>(), 10, It.IsAny<WorkflowContext>()), Times.Once);
            _mockTimesheetRepo.Verify(t => t.Add(It.IsAny<Timesheet>()), Times.Once);
            _mockTimesheetRepo.Verify(t => t.Update(It.Is<Timesheet>(ts => ts.WorkflowInstanceId == 202)), Times.Once);
        }

        [Fact]
        public async Task ApproveOrRejectAsync_WithWorkflowInstanceId_RoutesThroughWorkflowEngine()
        {
            // Arrange
            var config = CreateConfig(isTimesheetWorkflowEnabled: true);
            var service = new TimesheetService(
                _mockUow.Object,
                _mockWorkflowEngine.Object,
                config,
                _mockUserContext.Object);

            var existingTs = new Timesheet
            {
                Id = 44,
                EmployeeId = 10,
                ManagerId = 20,
                Status = "Submitted",
                WorkflowInstanceId = 303
            };
            _mockTimesheetRepo.Setup(t => t.GetById(44)).ReturnsAsync(existingTs);

            _mockWorkflowEngine
                .Setup(w => w.ActOnStepAsync(303, 20, It.Is<WorkflowActionRequest>(r => r.Action == "Approve")))
                .ReturnsAsync(new WorkflowActionResult { Success = true, WorkflowStatus = "Approved" });

            var dto = new TimesheetApprovalDTO
            {
                TimesheetId = 44,
                Status = "Approved"
            };

            // Act
            var ok = await service.ApproveOrRejectAsync(dto, 20);

            // Assert
            Assert.True(ok);
            _mockWorkflowEngine.Verify(w => w.ActOnStepAsync(303, 20, It.Is<WorkflowActionRequest>(r => r.Action == "Approve")), Times.Once);
        }

        [Fact]
        public async Task ApproveOrRejectAsync_WithoutWorkflowInstanceId_UsesLegacyApproval()
        {
            // Arrange
            var config = CreateConfig(isTimesheetWorkflowEnabled: true);
            var service = new TimesheetService(
                _mockUow.Object,
                _mockWorkflowEngine.Object,
                config,
                _mockUserContext.Object);

            var existingTs = new Timesheet
            {
                Id = 45,
                EmployeeId = 10,
                ManagerId = 20,
                Status = "Submitted",
                WorkflowInstanceId = null // Legacy record
            };
            _mockTimesheetRepo.Setup(t => t.GetById(45)).ReturnsAsync(existingTs);

            var dto = new TimesheetApprovalDTO
            {
                TimesheetId = 45,
                Status = "Approved"
            };

            // Act
            var ok = await service.ApproveOrRejectAsync(dto, 20);

            // Assert
            Assert.True(ok);
            _mockWorkflowEngine.Verify(w => w.ActOnStepAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<WorkflowActionRequest>()), Times.Never);
            _mockTimesheetRepo.Verify(t => t.Update(It.Is<Timesheet>(ts => ts.Status == "Approved")), Times.Once);
        }
    }
}
