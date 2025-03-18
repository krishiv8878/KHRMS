using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class EmployeeRoleMappingControllerTest
    {
        private readonly Mock<IEmployeeRoleMappingService> _mock;
        private readonly EmployeeRoleMappingController _controller;
        public EmployeeRoleMappingControllerTest()
        {
            _mock = new Mock<IEmployeeRoleMappingService>();
            _controller = new EmployeeRoleMappingController(_mock.Object);
        }
        [Fact]
        public async Task EmployeeRoleMapping_AddSuccessfully()
        {
            var employeeRoleMapping = new EmployeeRoleMapping()
            {
                Id = 1,
                EmployeeId = 1,
                RoleId = 1,
            };
            _mock.Setup(x => x.CreateEmployeeRoleMapping(It.IsAny<EmployeeRoleMapping>())).ReturnsAsync(true);

            var result = await _controller.AssignEmployeeRole(employeeRoleMapping);
            Assert.NotNull(result);
            _mock.Verify(x => x.CreateEmployeeRoleMapping(It.IsAny<EmployeeRoleMapping>()), Times.Once);
        }

        [Fact]
        public async Task EmployeeRoleMapping_GetSuccessfully()
        {
            var employeeRoleMapping = new List<EmployeeRoleMapping>
            {
                new EmployeeRoleMapping() {Id = 1,EmployeeId = 1,RoleId = 1},
                new EmployeeRoleMapping() {Id = 2,EmployeeId = 2, RoleId = 2}
            };
            _mock.Setup(x => x.GetAllEmployeeRoleMapping()).ReturnsAsync(employeeRoleMapping);
            var result = await _controller.GetEmployeeRoles();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnValue = Assert.IsType<KHRMS.Infrastructure.ApiResponse<List<EmployeeRoleMapping>>>(okResult.Value);

            Assert.NotNull(returnValue);
            Assert.Equal(employeeRoleMapping.Count(), returnValue.Data.Count());
            Assert.Contains(returnValue.Data, r => r.Id == 1);
            Assert.Contains(returnValue.Data, r => r.Id == 2);

            _mock.Verify(x => x.GetAllEmployeeRoleMapping(), Times.Once);

        }

        [Fact]
        public async Task employeeRoleMapping_DeleteSuccessfully()
        {
            var employeeRoleMappingid = 1;
            _mock.Setup(x => x.DeleteEmployeeRoleMapping(employeeRoleMappingid)).ReturnsAsync(true);
            var result = await _controller.DeleteEmployeeRole(employeeRoleMappingid);
            Assert.NotNull(result);
            _mock.Verify(x => x.DeleteEmployeeRoleMapping(employeeRoleMappingid), Times.Once);
        }

        [Fact]
        public async Task employeeRoleMapping_UpdateSuccessfully()
        {
            var employeeRoleMapping = new EmployeeRoleMapping()
            {
                Id = 1,
                EmployeeId = 1,
                RoleId = 1
            };
            var updateEmployeeRoleMapping = new EmployeeRoleMapping()
            {
                Id = 1,
                EmployeeId = 1,
                RoleId = 2,//update roleid
            };

            _mock.Setup(x => x.UpdateEmployeeRoleMapping(It.IsAny<EmployeeRoleMapping>())).ReturnsAsync(true);

            var result = await _controller.UpdateEmployeeRole(updateEmployeeRoleMapping);
            Assert.NotNull(result);

            _mock.Verify(x => x.UpdateEmployeeRoleMapping(It.Is<EmployeeRoleMapping>(r =>
                r.Id == updateEmployeeRoleMapping.Id &&
                r.RoleId == updateEmployeeRoleMapping.RoleId)), Times.Once());
        }
    }
}