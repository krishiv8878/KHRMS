using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KHRMS.UnitTest
{
    public class Phase2SecurityTests
    {
        [Fact]
        public async Task EmployeeAccessService_HierarchyScope_EnforcesDirectAndIndirectReporting()
        {
            // Arrange
            // Manager (10) -> Lead (20) -> Engineer (30)
            var employees = new List<Employee>
            {
                new Employee { Id = 10, FirstName = "Manager", LastName = "One", ManagerId = null, IsActive = true, IsDeleted = false },
                new Employee { Id = 20, FirstName = "Lead", LastName = "Two", ManagerId = 10, IsActive = true, IsDeleted = false },
                new Employee { Id = 30, FirstName = "Engineer", LastName = "Three", ManagerId = 20, IsActive = true, IsDeleted = false },
                new Employee { Id = 40, FirstName = "Peer", LastName = "Four", ManagerId = null, IsActive = true, IsDeleted = false }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockEmpRepo = new Mock<IEmployeeRepository>();
            mockEmpRepo.Setup(r => r.GetAll()).ReturnsAsync(employees);
            mockUnitOfWork.Setup(u => u.Employees).Returns(mockEmpRepo.Object);

            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(c => c.GetCurrentEmployeeId()).Returns(10);
            mockUserContext.Setup(c => c.HasPermissionAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockUserContext.Setup(c => c.IsAdmin()).Returns(false);
            var mockPermission = new Mock<IPermissionService>();

            var accessService = new EmployeeAccessService(mockUserContext.Object, mockUnitOfWork.Object, mockPermission.Object);

            // Act & Assert
            // 1. Manager (10) accessing self (10) => True
            Assert.True(await accessService.CanAccessEmployeeAsync(10));

            // 2. Manager (10) accessing direct report (20) => True
            Assert.True(await accessService.CanAccessEmployeeAsync(20));

            // 3. Manager (10) accessing indirect report (30) => True
            Assert.True(await accessService.CanAccessEmployeeAsync(30));

            // 4. Manager (10) accessing unrelated peer (40) => False
            Assert.False(await accessService.CanAccessEmployeeAsync(40));

            // 5. Manager (10) subordinate IDs contains both 20 and 30
            var subs = await accessService.GetSubordinateEmployeeIdsAsync(10);
            Assert.Contains(20L, subs);
            Assert.Contains(30L, subs);
            Assert.DoesNotContain(40L, subs);
        }

        [Fact]
        public void PayrollService_CsvExport_NeutralizesFormulaInjection()
        {
            // SEC-19: Reflection check on EscapeCsv method
            var method = typeof(PayrollService).GetMethod("EscapeCsv", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            var dangerousPayloads = new[]
            {
                "=cmd|' /C calc'!A0",
                "+1+1",
                "-2+3",
                "@SUM(1,2)",
                "\tDDE"
            };

            foreach (var payload in dangerousPayloads)
            {
                var result = (string)method.Invoke(null, new object[] { payload })!;
                Assert.StartsWith("'", result.TrimStart('"'));
            }
        }

        [Fact]
        public async Task UserLoginService_AccountLockout_After5ConsecutiveFailures()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockUserLogins = new Mock<IUserLoginRepository>();
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<UserLogin>();
            var validHash = hasher.HashPassword(new UserLogin(), "CorrectPassword");
            var users = new List<UserLogin>
            {
                new UserLogin { Id = 1, Email = "test@company.com", Password = validHash, IsActive = true, IsDeleted = false }
            };
            mockUserLogins.Setup(r => r.GetAll()).ReturnsAsync(users);
            mockUnitOfWork.Setup(u => u.UserLogins).Returns(mockUserLogins.Object);

            var mockEmail = new Mock<ISendEmailService>();
            var mockToken = new Mock<ITokenService>();
            var config = new ConfigurationBuilder().Build();

            var loginService = new UserLoginService(mockUnitOfWork.Object, mockEmail.Object, mockToken.Object, config, memoryCache);

            // Act: 4 failed attempts with bad password
            for (int i = 0; i < 4; i++)
            {
                var res = await loginService.GetUserLoginById("test@company.com", "WrongPassword");
                Assert.Null(res);
            }

            // 5th attempt: should trigger lockout exception
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                loginService.GetUserLoginById("test@company.com", "WrongPassword"));

            Assert.Contains("temporarily locked", ex.Message, StringComparison.OrdinalIgnoreCase);

            // 6th attempt (even with any password): should remain locked
            var ex2 = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                loginService.GetUserLoginById("test@company.com", "AnyPassword"));

            Assert.Contains("temporarily locked", ex2.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UserLoginService_ResetPasswordToken_CannotBeReused()
        {
            // Arrange
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockUserLogins = new Mock<IUserLoginRepository>();
            var testUser = new UserLogin { Id = 1, Email = "reset@company.com", Password = "OldHash", IsActive = true, IsDeleted = false };
            mockUserLogins.Setup(r => r.GetAll()).ReturnsAsync(new List<UserLogin> { testUser });
            mockUserLogins.Setup(r => r.Update(It.IsAny<UserLogin>()));
            mockUnitOfWork.Setup(u => u.UserLogins).Returns(mockUserLogins.Object);
            mockUnitOfWork.Setup(u => u.Save()).Returns(1);

            var mockEmail = new Mock<ISendEmailService>();
            var mockToken = new Mock<ITokenService>();
            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "reset@company.com")
                }));
            mockToken.Setup(t => t.ValidatePasswordResetToken("ValidSingleUseToken")).Returns(principal);

            var config = new ConfigurationBuilder().Build();
            var loginService = new UserLoginService(mockUnitOfWork.Object, mockEmail.Object, mockToken.Object, config, memoryCache);

            // Act 1: First reset with token succeeds
            var firstReset = await loginService.ResetPassword(new ResetPasswordModel
            {
                Token = "ValidSingleUseToken",
                Password = "NewSecurePassword123!"
            });
            Assert.True(firstReset);

            // Act 2: Second reset with identical token fails with InvalidOperationException
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                loginService.ResetPassword(new ResetPasswordModel
                {
                    Token = "ValidSingleUseToken",
                    Password = "AnotherNewPassword123!"
                }));

            Assert.Contains("already been used", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
