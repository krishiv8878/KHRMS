using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KHRMS.AI;
using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KHRMS.UnitTest
{
    public class Sprint3RemediationTests
    {
        [Fact]
        public async Task UserLoginController_ForgotPassword_ReturnsGenericOk_ForBothExistingAndNonExistentEmails()
        {
            // SEC-04: Both existing and non-existing emails must receive an identical 200 OK response
            var mockLoginService = new Mock<IUserLoginService>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();

            // Case A: Existing user (service returns true)
            mockLoginService.Setup(s => s.ForgotPasswordMail(It.Is<ForgotPasswordRequestModel>(m => m.Email == "existing@company.com")))
                .ReturnsAsync(true);

            // Case B: Non-existent user (service returns false)
            mockLoginService.Setup(s => s.ForgotPasswordMail(It.Is<ForgotPasswordRequestModel>(m => m.Email == "nonexistent@company.com")))
                .ReturnsAsync(false);

            var controller = new UserLoginController(mockLoginService.Object, mockHttpAccessor.Object);

            // Act A
            var resA = await controller.ForgotPassword(new ForgotPasswordRequestModel
            {
                Email = "existing@company.com",
                ClientUrl = "http://localhost:4200/reset-password"
            }) as OkObjectResult;

            // Act B
            var resB = await controller.ForgotPassword(new ForgotPasswordRequestModel
            {
                Email = "nonexistent@company.com",
                ClientUrl = "http://localhost:4200/reset-password"
            }) as OkObjectResult;

            // Assert
            Assert.NotNull(resA);
            Assert.Equal((int)HttpStatusCode.OK, resA.StatusCode);
            var apiResA = Assert.IsType<ApiResponse<string>>(resA.Value);

            Assert.NotNull(resB);
            Assert.Equal((int)HttpStatusCode.OK, resB.StatusCode);
            var apiResB = Assert.IsType<ApiResponse<string>>(resB.Value);

            // Both messages must be completely uniform to prevent account enumeration
            Assert.Equal(ApiMessageConstant.GenericPasswordResetNotice, apiResA.Message);
            Assert.Equal(ApiMessageConstant.GenericPasswordResetNotice, apiResB.Message);
            Assert.Equal(apiResA.Message, apiResB.Message);
        }

        [Fact]
        public async Task UserLoginController_ForgotPassword_ValidatesRequiredFields()
        {
            var mockLoginService = new Mock<IUserLoginService>();
            var mockHttpAccessor = new Mock<IHttpContextAccessor>();
            var controller = new UserLoginController(mockLoginService.Object, mockHttpAccessor.Object);

            var resNull = await controller.ForgotPassword(null!) as BadRequestObjectResult;
            var resEmptyEmail = await controller.ForgotPassword(new ForgotPasswordRequestModel { Email = "", ClientUrl = "http://localhost" }) as BadRequestObjectResult;

            Assert.NotNull(resNull);
            Assert.NotNull(resEmptyEmail);
        }

        [Fact]
        public async Task SqlChatPolicyService_SearchesActiveCompanyPolicies_AndStripsHtml()
        {
            // INT-04: Test SQL policy query, term scoring, and HTML stripping
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPolicyRepo = new Mock<ICompanyPolicyRepository>();

            var policies = new List<CompanyPolicy>
            {
                new CompanyPolicy
                {
                    Id = 1,
                    Title = "Annual Leave Entitlement Policy",
                    Category = "Leave",
                    ContentHtml = "<p>Employees receive <strong>24 days</strong> of paid annual leave per calendar year.</p>",
                    IsActive = true,
                    IsDeleted = false
                },
                new CompanyPolicy
                {
                    Id = 2,
                    Title = "Information Security Clean Desk Policy",
                    Category = "IT & Security",
                    ContentHtml = "<div>Workstations must be locked when unattended. Sensitive papers must be shredded.</div>",
                    IsActive = true,
                    IsDeleted = false
                },
                new CompanyPolicy
                {
                    Id = 3,
                    Title = "Archived Old Policy",
                    Category = "Leave",
                    ContentHtml = "Deprecated policy text",
                    IsActive = false,
                    IsDeleted = true
                }
            };

            mockPolicyRepo.Setup(r => r.GetAll()).ReturnsAsync(policies);
            mockUnitOfWork.Setup(u => u.CompanyPolicies).Returns(mockPolicyRepo.Object);

            var policyService = new SqlChatPolicyService(mockUnitOfWork.Object);

            // Act 1: Search for leave policy
            var results = (await policyService.SearchAsync("annual leave entitlement")).ToList();

            // Assert 1
            Assert.NotEmpty(results);
            var top = results.First();
            Assert.Equal("Annual Leave Entitlement Policy", top.Title);
            Assert.Equal("Leave", top.Category);
            // Verify HTML tags were stripped
            Assert.DoesNotContain("<p>", top.Content);
            Assert.DoesNotContain("<strong>", top.Content);
            Assert.Contains("24 days", top.Content);

            // Act 2: Search for clean desk policy
            var secResults = (await policyService.SearchAsync("clean desk lock workstation")).ToList();
            Assert.NotEmpty(secResults);
            Assert.Equal("Information Security Clean Desk Policy", secResults.First().Title);
        }

        [Fact]
        public async Task SqlChatPolicyService_FallsBackToDefaultPolicies_WhenDatabaseIsEmpty()
        {
            // INT-04: When DB has no custom policies, fallback gracefully to default baseline
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPolicyRepo = new Mock<ICompanyPolicyRepository>();
            mockPolicyRepo.Setup(r => r.GetAll()).ReturnsAsync(new List<CompanyPolicy>());
            mockUnitOfWork.Setup(u => u.CompanyPolicies).Returns(mockPolicyRepo.Object);

            var policyService = new SqlChatPolicyService(mockUnitOfWork.Object);

            var results = (await policyService.SearchAsync("maternity leave 26 weeks")).ToList();

            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.Title.Contains("Maternity Leave", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task EmployeePaymentInfoController_SanitizesExceptions_OnAddAndUpdate()
        {
            // SEC-05: Ensure internal exception messages (SQL errors, stack traces) are NEVER returned to caller
            var mockPaymentService = new Mock<IEmployeePaymentInfoService>();
            var mockAccessService = new Mock<IEmployeeAccessService>();

            mockAccessService.Setup(a => a.GetCurrentEmployeeId()).Returns(100);
            mockAccessService.Setup(a => a.CanAccessEmployeeAsync(It.IsAny<long>(), "PAYROLL_MANAGE")).ReturnsAsync(true);

            // Configure services to throw internal database / SQL exceptions
            mockPaymentService.Setup(s => s.AddAsync(It.IsAny<EmployeePaymentInfo>()))
                .ThrowsAsync(new Exception("System.Data.SqlClient.SqlException: Table 'KHRMS.dbo.EmployeePaymentInfo' was not found or server connection failed"));

            mockPaymentService.Setup(s => s.UpdateAsync(It.IsAny<EmployeePaymentInfo>()))
                .ThrowsAsync(new Exception("Deadlock encountered on process ID 44 with transaction T1"));

            var controller = new EmployeePaymentInfoController(mockPaymentService.Object, mockAccessService.Object);

            // Act 1: Add Payment Info
            var addResult = await controller.Create(new EmployeePaymentInfo { EmployeeId = 100, BankName = "HDFC", AccountNumber = "1234567890" }) as ObjectResult;

            // Assert 1
            Assert.NotNull(addResult);
            Assert.Equal((int)HttpStatusCode.InternalServerError, addResult.StatusCode);
            var addApiResponse = Assert.IsType<ApiResponse<bool>>(addResult.Value);
            Assert.DoesNotContain("SqlException", addApiResponse.Message);
            Assert.DoesNotContain("dbo.EmployeePaymentInfo", addApiResponse.Message);
            Assert.Contains("unexpected error", addApiResponse.Message, StringComparison.OrdinalIgnoreCase);

            // Act 2: Update Payment Info
            var updateResult = await controller.Update(new EmployeePaymentInfo { Id = 1, EmployeeId = 100, BankName = "HDFC", AccountNumber = "1234567890" }) as ObjectResult;

            // Assert 2
            Assert.NotNull(updateResult);
            Assert.Equal((int)HttpStatusCode.InternalServerError, updateResult.StatusCode);
            var updateApiResponse = Assert.IsType<ApiResponse<bool>>(updateResult.Value);
            Assert.DoesNotContain("Deadlock", updateApiResponse.Message);
            Assert.Contains("unexpected error", updateApiResponse.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
