using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Authorization;
using KHRMS.Controllers;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KHRMS.UnitTest
{
    public class Phase3ServiceTests
    {
        // =========================================================================
        // 1. HELPDESK CONTROLLER TESTS
        // =========================================================================

        [Fact]
        public async Task HelpdeskController_GetTickets_ReturnsOkWithData()
        {
            var mockService = new Mock<IHelpdeskService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(101);
            mockUserContext.Setup(u => u.IsAdmin()).Returns(false);
            mockUserContext.Setup(u => u.IsHR()).Returns(false);

            var sampleTickets = new List<HelpdeskTicketDto>
            {
                new HelpdeskTicketDto { Id = 1, TicketNumber = "HD-2026-0001", Subject = "Laptop battery issue", Status = "Open" }
            };
            mockService.Setup(s => s.GetTicketsAsync(null, null, null, null, null))
                .ReturnsAsync(sampleTickets);

            var controller = new HelpdeskController(mockService.Object, mockUserContext.Object);

            var result = await controller.GetTickets(null, null, null, null, null) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<List<HelpdeskTicketDto>>>(result.Value);
            Assert.Single(apiRes.Data);
            Assert.Equal("HD-2026-0001", apiRes.Data[0].TicketNumber);
        }

        [Fact]
        public async Task HelpdeskController_CreateTicket_ReturnsCreated()
        {
            var mockService = new Mock<IHelpdeskService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(105);

            var req = new CreateHelpdeskTicketRequest
            {
                Subject = "VPN access error",
                Description = "Cannot connect to staging VPN gateway",
                DepartmentCategory = "IT Support",
                Priority = "High"
            };

            var createdTicketDto = new HelpdeskTicketDto
            {
                Id = 42,
                TicketNumber = "HD-2026-0042",
                Subject = req.Subject,
                RequesterEmployeeId = 105,
                Priority = req.Priority,
                Status = "Open"
            };

            mockService.Setup(s => s.CreateTicketAsync(req, 105))
                .ReturnsAsync(createdTicketDto);

            var controller = new HelpdeskController(mockService.Object, mockUserContext.Object);

            var result = await controller.CreateTicket(req) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<HelpdeskTicketDto>>(result.Value);
            Assert.Equal(42, apiRes.Data.Id);
            Assert.Equal("HD-2026-0042", apiRes.Data.TicketNumber);
        }

        [Fact]
        public async Task HelpdeskController_CreateTicket_MissingRequiredFields_ReturnsBadRequest()
        {
            var mockService = new Mock<IHelpdeskService>();
            var mockUserContext = new Mock<IUserContextService>();
            var controller = new HelpdeskController(mockService.Object, mockUserContext.Object);

            var resultNull = await controller.CreateTicket(null!) as BadRequestObjectResult;
            var resultEmptySubject = await controller.CreateTicket(new CreateHelpdeskTicketRequest
            {
                Subject = "",
                Description = "desc"
            }) as BadRequestObjectResult;

            Assert.NotNull(resultNull);
            Assert.NotNull(resultEmptySubject);
            Assert.Equal((int)HttpStatusCode.BadRequest, resultNull.StatusCode);
            Assert.Equal((int)HttpStatusCode.BadRequest, resultEmptySubject.StatusCode);
        }

        [Fact]
        public async Task HelpdeskController_UpdateStatus_ReturnsOk()
        {
            var mockService = new Mock<IHelpdeskService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(99);

            var req = new UpdateHelpdeskTicketStatusRequest
            {
                TicketId = 12,
                Status = "Resolved",
                ResolutionNotes = "Driver updated and tested"
            };

            var updatedTicket = new HelpdeskTicketDto
            {
                Id = 12,
                TicketNumber = "HD-2026-0012",
                Status = "Resolved",
                ResolutionNotes = req.ResolutionNotes
            };

            mockService.Setup(s => s.UpdateTicketStatusAsync(req, 99))
                .ReturnsAsync(updatedTicket);

            var controller = new HelpdeskController(mockService.Object, mockUserContext.Object);

            var result = await controller.UpdateTicketStatus(req) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<HelpdeskTicketDto>>(result.Value);
            Assert.Equal("Resolved", apiRes.Data.Status);
        }

        [Fact]
        public async Task HelpdeskController_GetMetrics_ReturnsOk()
        {
            var mockService = new Mock<IHelpdeskService>();
            var mockUserContext = new Mock<IUserContextService>();

            var expectedMetrics = new HelpdeskMetricsDto
            {
                TotalTickets = 50,
                OpenTickets = 10,
                ResolvedTickets = 35,
                SlaCompliancePercent = 94.5m
            };

            mockService.Setup(s => s.GetHelpdeskMetricsAsync(null))
                .ReturnsAsync(expectedMetrics);

            var controller = new HelpdeskController(mockService.Object, mockUserContext.Object);

            var result = await controller.GetMetrics(null) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<HelpdeskMetricsDto>>(result.Value);
            Assert.Equal(50, apiRes.Data.TotalTickets);
            Assert.Equal(94.5m, apiRes.Data.SlaCompliancePercent);
        }

        // =========================================================================
        // 2. COMPANY POLICY CONTROLLER TESTS
        // =========================================================================

        [Fact]
        public async Task CompanyPolicyController_GetAll_ReturnsOk()
        {
            var mockService = new Mock<ICompanyPolicyService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(55);

            var list = new List<PolicyDto>
            {
                new PolicyDto { Id = 1, Title = "Information Security Policy", Category = "IT", IsActive = true }
            };

            mockService.Setup(s => s.GetAllPoliciesAsync(55, null, true))
                .ReturnsAsync(list);

            var controller = new CompanyPolicyController(mockService.Object, mockUserContext.Object);

            var result = await controller.GetAll(null, true) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<List<PolicyDto>>>(result.Value);
            Assert.Single(apiRes.Data);
            Assert.Equal("Information Security Policy", apiRes.Data[0].Title);
        }

        [Fact]
        public async Task CompanyPolicyController_Create_ReturnsCreatedAtAction()
        {
            var mockService = new Mock<ICompanyPolicyService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(1);

            var req = new CreateCompanyPolicyRequest
            {
                Title = "Anti-Harassment Policy",
                Category = "Code of Conduct",
                ContentHtml = "<p>Standard policy content</p>",
                IsMandatoryAcknowledgement = true
            };

            var createdPolicy = new CompanyPolicy
            {
                Id = 10,
                Title = req.Title,
                Category = req.Category,
                ContentHtml = req.ContentHtml,
                IsMandatoryAcknowledgement = true
            };

            mockService.Setup(s => s.CreatePolicyAsync(req, 1))
                .ReturnsAsync(createdPolicy);

            var controller = new CompanyPolicyController(mockService.Object, mockUserContext.Object);

            var result = await controller.Create(req) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.Created, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<CompanyPolicy>>(result.Value);
            Assert.Equal(10, apiRes.Data.Id);
            Assert.Equal("Anti-Harassment Policy", apiRes.Data.Title);
        }

        [Fact]
        public async Task CompanyPolicyController_Acknowledge_RecordsAcknowledgement()
        {
            var mockService = new Mock<ICompanyPolicyService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.GetCurrentEmployeeId()).Returns(123);

            var req = new AcknowledgePolicyRequest
            {
                PolicyId = 5,
                PolicyVersion = 1
            };

            var ackRecord = new PolicyAcknowledgement
            {
                Id = 88,
                PolicyId = 5,
                PolicyVersion = 1,
                EmployeeId = 123,
                AcknowledgedDate = DateTime.UtcNow
            };

            mockService.Setup(s => s.AcknowledgePolicyAsync(5, 1, 123, It.IsAny<string?>()))
                .ReturnsAsync(ackRecord);

            var controller = new CompanyPolicyController(mockService.Object, mockUserContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.Acknowledge(req) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<PolicyAcknowledgement>>(result.Value);
            Assert.Equal(123, apiRes.Data.EmployeeId);
            Assert.Equal(5, apiRes.Data.PolicyId);
        }

        [Fact]
        public async Task CompanyPolicyController_GetComplianceSummary_ReturnsSummary()
        {
            var mockService = new Mock<ICompanyPolicyService>();
            var mockUserContext = new Mock<IUserContextService>();

            var summary = new PolicyComplianceSummaryDto
            {
                TotalActivePolicies = 12,
                MandatoryPoliciesCount = 8,
                OverallComplianceRatePercent = 88.0m
            };

            mockService.Setup(s => s.GetComplianceSummaryAsync())
                .ReturnsAsync(summary);

            var controller = new CompanyPolicyController(mockService.Object, mockUserContext.Object);

            var result = await controller.GetComplianceSummary() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<PolicyComplianceSummaryDto>>(result.Value);
            Assert.Equal(12, apiRes.Data.TotalActivePolicies);
            Assert.Equal(88.0m, apiRes.Data.OverallComplianceRatePercent);
        }

        // =========================================================================
        // 3. REPORTS CONTROLLER TESTS
        // =========================================================================

        [Fact]
        public async Task ReportsController_GetAttendanceSummary_AuthorizedUser_ReturnsAttendanceList()
        {
            var mockService = new Mock<IReportsService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.IsAdmin()).Returns(true);

            var sampleData = new List<AttendanceReportItemDto>
            {
                new AttendanceReportItemDto { EmployeeId = 1, EmployeeName = "Alice", PresentDays = 21, TotalWorkHours = 168 }
            };

            mockService.Setup(s => s.GetAttendanceSummaryReportAsync(11, 2026, null))
                .ReturnsAsync(sampleData);

            var controller = new ReportsController(mockService.Object, mockUserContext.Object);

            var result = await controller.GetAttendanceSummary(11, 2026, null) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<List<AttendanceReportItemDto>>>(result.Value);
            Assert.Single(apiRes.Data);
            Assert.Equal("Alice", apiRes.Data[0].EmployeeName);
        }

        [Fact]
        public async Task ReportsController_GetAttendanceSummary_Unauthorized_ReturnsForbid()
        {
            var mockService = new Mock<IReportsService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.IsAdmin()).Returns(false);
            mockUserContext.Setup(u => u.IsHR()).Returns(false);
            mockUserContext.Setup(u => u.IsManager()).Returns(false);

            var controller = new ReportsController(mockService.Object, mockUserContext.Object);

            var result = await controller.GetAttendanceSummary(11, 2026, null);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task ReportsController_GetPayrollReconciliation_Authorized_ReturnsList()
        {
            var mockService = new Mock<IReportsService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.IsHR()).Returns(true);

            var reconData = new List<PayrollReconciliationReportItemDto>
            {
                new PayrollReconciliationReportItemDto { EmployeeId = 10, EmployeeName = "Bob", BasicSalary = 50000, NetPay = 45000 }
            };

            mockService.Setup(s => s.GetPayrollReconciliationReportAsync(null, 11, 2026, null))
                .ReturnsAsync(reconData);

            var controller = new ReportsController(mockService.Object, mockUserContext.Object);

            var result = await controller.GetPayrollReconciliation(null, 11, 2026, null) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<List<PayrollReconciliationReportItemDto>>>(result.Value);
            Assert.Single(apiRes.Data);
            Assert.Equal("Bob", apiRes.Data[0].EmployeeName);
        }

        [Fact]
        public async Task ReportsController_GetHeadcountDemographics_Authorized_ReturnsBreakdown()
        {
            var mockService = new Mock<IReportsService>();
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(u => u.IsAdmin()).Returns(true);

            var demo = new HeadcountDemographicsReportDto
            {
                TotalActiveEmployees = 150,
                HeadcountByDepartment = new Dictionary<string, int> { { "Engineering", 90 }, { "HR", 10 } }
            };

            mockService.Setup(s => s.GetHeadcountDemographicsReportAsync())
                .ReturnsAsync(demo);

            var controller = new ReportsController(mockService.Object, mockUserContext.Object);

            var result = await controller.GetHeadcountDemographics() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            var apiRes = Assert.IsType<ApiResponse<HeadcountDemographicsReportDto>>(result.Value);
            Assert.Equal(150, apiRes.Data.TotalActiveEmployees);
            Assert.Equal(90, apiRes.Data.HeadcountByDepartment["Engineering"]);
        }
    }
}
