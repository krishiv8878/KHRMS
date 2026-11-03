using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KHRMS.UnitTest
{
    public class ArchitectureSecurityTests
    {
        [Fact]
        public void AllControllersAndActions_MustHaveExplicitAuthorizationOrAllowAnonymous()
        {
            // Scan all controllers in the API assembly
            var apiAssembly = typeof(KHRMS.UserLoginController).Assembly;
            var controllerTypes = apiAssembly.GetTypes()
                .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            var unprotectedEndpoints = new List<string>();

            foreach (var controller in controllerTypes)
            {
                var controllerHasAuth = controller.GetCustomAttribute<AuthorizeAttribute>(inherit: true) != null ||
                                       controller.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true) != null ||
                                       controller.GetCustomAttribute<RequirePermissionAttribute>(inherit: true) != null;

                var actionMethods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName && typeof(IActionResult).IsAssignableFrom(m.ReturnType) ||
                                (m.ReturnType.IsGenericType && typeof(Task).IsAssignableFrom(m.ReturnType)))
                    .ToList();

                foreach (var action in actionMethods)
                {
                    var actionHasAuth = action.GetCustomAttribute<AuthorizeAttribute>(inherit: true) != null ||
                                       action.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true) != null ||
                                       action.GetCustomAttribute<RequirePermissionAttribute>(inherit: true) != null;

                    if (!controllerHasAuth && !actionHasAuth)
                    {
                        unprotectedEndpoints.Add($"{controller.Name}.{action.Name}");
                    }
                }
            }

            // Assert: If any endpoint has no authorization attribute and no AllowAnonymous, fail with complete list
            Assert.True(
                unprotectedEndpoints.Count == 0,
                $"The following {unprotectedEndpoints.Count} endpoints lack an explicit [Authorize], [RequirePermission], or [AllowAnonymous] attribute:\n" +
                string.Join("\n", unprotectedEndpoints)
            );
        }

        [Fact]
        public async Task EmployeeAccessService_SelfAccess_Allowed()
        {
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(c => c.GetCurrentEmployeeId()).Returns(42);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPermission = new Mock<IPermissionService>();

            var accessService = new EmployeeAccessService(mockUserContext.Object, mockUnitOfWork.Object,mockPermission.Object);

            var canAccess = await accessService.CanAccessEmployeeAsync(42);
            Assert.True(canAccess, "An employee must always be authorized to access their own record.");
        }

        [Fact]
        public async Task EmployeeAccessService_OtherEmployee_DeniedWithoutPrivileges()
        {
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(c => c.GetCurrentEmployeeId()).Returns(42);
            mockUserContext.Setup(c => c.HasPermissionAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockUserContext.Setup(c => c.IsAdmin()).Returns(false);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockPermission = new Mock<IPermissionService>();
            mockUnitOfWork.Setup(u => u.Employees.GetAll()).ReturnsAsync(new List<KHRMS.Core.Employee>
            {
                new KHRMS.Core.Employee { Id = 42, ManagerId = 99 },
                new KHRMS.Core.Employee { Id = 88, ManagerId = 99 } // Peer
            });

            var accessService = new EmployeeAccessService(mockUserContext.Object, mockUnitOfWork.Object,mockPermission.Object);

            var canAccess = await accessService.CanAccessEmployeeAsync(88);
            Assert.False(canAccess, "An employee must be denied access to a colleague's record if they are not their manager and lack HR permissions.");
        }

        [Fact]
        public async Task EmployeeAccessService_DirectReport_Allowed()
        {
            var mockUserContext = new Mock<IUserContextService>();
            mockUserContext.Setup(c => c.GetCurrentEmployeeId()).Returns(10); // Manager ID: 10
            mockUserContext.Setup(c => c.HasPermissionAsync(It.IsAny<string>())).ReturnsAsync(false);
            mockUserContext.Setup(c => c.IsAdmin()).Returns(false);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(u => u.Employees.GetAll()).ReturnsAsync(new List<KHRMS.Core.Employee>
            {
                new KHRMS.Core.Employee { Id = 10, ManagerId = null },
                new KHRMS.Core.Employee { Id = 55, ManagerId = 10 } // Reports directly to 10
            });
            var mockPermission = new Mock<IPermissionService>();

            var accessService = new EmployeeAccessService(mockUserContext.Object, mockUnitOfWork.Object,mockPermission.Object);

            var canAccess = await accessService.CanAccessEmployeeAsync(55);
            Assert.True(canAccess, "A manager must be authorized to access their direct report's record.");
        }
    }
}
