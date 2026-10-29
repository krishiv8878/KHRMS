using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class ExpenseWorkflowIntegrationTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IExpenseReportRepository> _mockReportRepo;
        private readonly Mock<IExpenseItemRepository> _mockItemRepo;
        private readonly Mock<IExpenseCategoryRepository> _mockCatRepo;
        private readonly Mock<IExpenseMileageRepository> _mockMilRepo;
        private readonly Mock<IExpenseAdvanceRepository> _mockAdvRepo;
        private readonly Mock<IEmployeeRepository> _mockEmpRepo;
        private readonly Mock<IDesignationRepository> _mockDesigRepo;
        private readonly Mock<IProjectMasterRepository> _mockProjRepo;
        private readonly Mock<IWorkflowInstanceRepository> _mockWfInstanceRepo;
        private readonly Mock<IWorkflowInstanceStepRepository> _mockWfStepRepo;
        private readonly Mock<IWorkflowEngineService> _mockWorkflowEngine;
        private readonly Mock<IUserContextService> _mockUserContext;

        public ExpenseWorkflowIntegrationTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockReportRepo = new Mock<IExpenseReportRepository>();
            _mockItemRepo = new Mock<IExpenseItemRepository>();
            _mockCatRepo = new Mock<IExpenseCategoryRepository>();
            _mockMilRepo = new Mock<IExpenseMileageRepository>();
            _mockAdvRepo = new Mock<IExpenseAdvanceRepository>();
            _mockEmpRepo = new Mock<IEmployeeRepository>();
            _mockDesigRepo = new Mock<IDesignationRepository>();
            _mockProjRepo = new Mock<IProjectMasterRepository>();
            _mockWfInstanceRepo = new Mock<IWorkflowInstanceRepository>();
            _mockWfStepRepo = new Mock<IWorkflowInstanceStepRepository>();
            _mockWorkflowEngine = new Mock<IWorkflowEngineService>();
            _mockUserContext = new Mock<IUserContextService>();

            _mockUow.Setup(u => u.ExpenseReports).Returns(_mockReportRepo.Object);
            _mockUow.Setup(u => u.ExpenseItems).Returns(_mockItemRepo.Object);
            _mockUow.Setup(u => u.ExpenseCategories).Returns(_mockCatRepo.Object);
            _mockUow.Setup(u => u.ExpenseMileages).Returns(_mockMilRepo.Object);
            _mockUow.Setup(u => u.ExpenseAdvances).Returns(_mockAdvRepo.Object);
            _mockUow.Setup(u => u.Employees).Returns(_mockEmpRepo.Object);
            _mockUow.Setup(u => u.Designations).Returns(_mockDesigRepo.Object);
            _mockUow.Setup(u => u.ProjectMasters).Returns(_mockProjRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstances).Returns(_mockWfInstanceRepo.Object);
            _mockUow.Setup(u => u.WorkflowInstanceSteps).Returns(_mockWfStepRepo.Object);
            _mockUow.Setup(u => u.Save()).Returns(1);

            _mockReportRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<ExpenseReport>());
            _mockItemRepo.Setup(i => i.GetAll()).ReturnsAsync(new List<ExpenseItem>());
            _mockCatRepo.Setup(c => c.GetAll()).ReturnsAsync(new List<ExpenseCategory>());
            _mockMilRepo.Setup(m => m.GetAll()).ReturnsAsync(new List<ExpenseMileage>());
            _mockAdvRepo.Setup(a => a.GetAll()).ReturnsAsync(new List<ExpenseAdvance>());
            _mockEmpRepo.Setup(e => e.GetAll()).ReturnsAsync(new List<Employee>());
            _mockDesigRepo.Setup(d => d.GetAll()).ReturnsAsync(new List<Designation>());
            _mockProjRepo.Setup(p => p.GetAll()).ReturnsAsync(new List<ProjectMaster>());
            _mockWfInstanceRepo.Setup(w => w.GetAll()).ReturnsAsync(new List<WorkflowInstance>());
            _mockWfStepRepo.Setup(s => s.GetAll()).ReturnsAsync(new List<WorkflowInstanceStep>());

            _mockEmpRepo.Setup(e => e.GetById(It.IsAny<long>())).ReturnsAsync(new Employee
            {
                Id = 10,
                FirstName = "Alice",
                LastName = "Smith",
                ManagerId = 20
            });

            _mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(10);
            _mockUserContext.Setup(u => u.IsAdmin()).Returns(false);
            _mockUserContext.Setup(u => u.IsHR()).Returns(false);
        }

        private IConfiguration CreateConfig(bool isExpenseWorkflowEnabled)
        {
            var dict = new Dictionary<string, string?>
            {
                { "WorkflowEngine:Modules:Expense", isExpenseWorkflowEnabled ? "true" : "false" }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        }

        [Fact]
        public async Task SubmitReportAsync_WhenWorkflowEnabled_StartsWorkflowAndSetsInstanceId()
        {
            // Arrange
            var config = CreateConfig(isExpenseWorkflowEnabled: true);
            var service = new ExpenseService(
                _mockUow.Object,
                null!, // DbContext unused in this flow
                _mockUserContext.Object,
                _mockWorkflowEngine.Object,
                config);

            var report = new ExpenseReport
            {
                Id = 100,
                EmployeeId = 10,
                Title = "Client Onsite Travel Claim",
                TotalClaimedAmount = 4500m,
                Status = "Draft"
            };
            _mockReportRepo.Setup(r => r.GetById(100)).ReturnsAsync(report);
            _mockReportRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<ExpenseReport> { report });

            _mockWorkflowEngine
                .Setup(w => w.StartWorkflowAsync("Expense", 100, 10, It.IsAny<WorkflowContext>()))
                .ReturnsAsync(new WorkflowInstance { Id = 404, RequestType = "Expense", Status = "Pending" });

            // Act
            var res = await service.SubmitReportAsync(100);

            // Assert
            _mockWorkflowEngine.Verify(w => w.StartWorkflowAsync("Expense", 100, 10, It.IsAny<WorkflowContext>()), Times.Once);
            _mockReportRepo.Verify(r => r.Update(It.Is<ExpenseReport>(rep => rep.WorkflowInstanceId == 404 && rep.Status == "Submitted")), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ReviewReportAsync_WithWorkflowInstanceId_RoutesThroughWorkflowEngine()
        {
            // Arrange
            var config = CreateConfig(isExpenseWorkflowEnabled: true);
            var service = new ExpenseService(
                _mockUow.Object,
                null!,
                _mockUserContext.Object,
                _mockWorkflowEngine.Object,
                config);

            var report = new ExpenseReport
            {
                Id = 101,
                EmployeeId = 10,
                Status = "Submitted",
                WorkflowInstanceId = 505
            };
            _mockReportRepo.Setup(r => r.GetById(101)).ReturnsAsync(report);
            _mockReportRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<ExpenseReport> { report });

            _mockWorkflowEngine
                .Setup(w => w.ActOnStepAsync(505, It.IsAny<long>(), It.Is<WorkflowActionRequest>(req => req.Action == "Approve")))
                .ReturnsAsync(new WorkflowActionResult { Success = true, WorkflowStatus = "Approved" });

            var reviewReq = new ReviewExpenseReportRequest
            {
                ReportId = 101,
                Action = "ManagerApprove",
                Remarks = "Approved travel expense"
            };

            // Act
            var res = await service.ReviewReportAsync(reviewReq);

            // Assert
            _mockWorkflowEngine.Verify(w => w.ActOnStepAsync(505, It.IsAny<long>(), It.Is<WorkflowActionRequest>(req => req.Action == "Approve")), Times.Once);
        }

        [Fact]
        public async Task ReviewReportAsync_WithoutWorkflowInstanceId_UsesLegacyReview()
        {
            // Arrange
            var config = CreateConfig(isExpenseWorkflowEnabled: true);
            var service = new ExpenseService(
                _mockUow.Object,
                null!,
                _mockUserContext.Object,
                _mockWorkflowEngine.Object,
                config);

            var report = new ExpenseReport
            {
                Id = 102,
                EmployeeId = 10,
                Status = "Submitted",
                TotalClaimedAmount = 2000m,
                WorkflowInstanceId = null // Legacy
            };
            _mockReportRepo.Setup(r => r.GetById(102)).ReturnsAsync(report);
            _mockReportRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<ExpenseReport> { report });

            var reviewReq = new ReviewExpenseReportRequest
            {
                ReportId = 102,
                Action = "ManagerApprove",
                Remarks = "Approved"
            };

            // Act
            var res = await service.ReviewReportAsync(reviewReq);

            // Assert
            _mockWorkflowEngine.Verify(w => w.ActOnStepAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<WorkflowActionRequest>()), Times.Never);
            _mockReportRepo.Verify(r => r.Update(It.Is<ExpenseReport>(rep => rep.Status == "ManagerApproved")), Times.Once);
        }
    }
}
