using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{
    public class EmployeeRoleMappingServiceTest
    {

        private readonly Mock<IEmployeeRoleMappingService> _mock;
        public EmployeeRoleMappingServiceTest()
        {
            _mock = new Mock<IEmployeeRoleMappingService>();
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
            var result = await _mock.Object.CreateEmployeeRoleMapping(employeeRoleMapping);
            Assert.True(result);
            _mock.Verify(x => x.CreateEmployeeRoleMapping(It.IsAny<EmployeeRoleMapping>()), Times.Once);
        }
        [Fact]
        public async Task EmployeeRoleMapping_ThrowException_whenDataInvalid()
        {
            var employeeRoleMapping = new EmployeeRoleMapping()
            {
                Id = 1,
                EmployeeId = 1,
                RoleId = 999,//invalid roleId 
            };

            _mock.Setup(x => x.CreateEmployeeRoleMapping(It.IsAny<EmployeeRoleMapping>())).Throws(new ArgumentException("Invalid Data of EmployeeRoleMapping"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.CreateEmployeeRoleMapping(employeeRoleMapping));
            Assert.Equal("Invalid Data of EmployeeRoleMapping", exception.Message);
        }
        [Fact]
        public async Task EmployeeRoleMapping_ThrowException_whenDataIsNull()
        {
            EmployeeRoleMapping employeeRoleMapping = null;

            _mock.Setup(x => x.CreateEmployeeRoleMapping(It.IsAny<EmployeeRoleMapping>())).Throws(new Exception("EmployeeRoleMapping Is Null"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.CreateEmployeeRoleMapping(employeeRoleMapping));

            Assert.Equal("EmployeeRoleMapping Is Null", exception.Message);
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
            var result = await _mock.Object.GetAllEmployeeRoleMapping();

            _mock.Verify(x => x.GetAllEmployeeRoleMapping(), Times.Once);

            Assert.Equal(employeeRoleMapping.Count(), result.Count());
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);
        }
        [Fact]
        public async Task employeeRoleMapping_NotFound()
        {
            var employeeRoleMapping = new List<EmployeeRoleMapping>();
            _mock.Setup(x => x.GetAllEmployeeRoleMapping()).ThrowsAsync(new Exception("EmployeeRoleMapping Not Found"));
            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.GetAllEmployeeRoleMapping());
            Assert.Equal("EmployeeRoleMapping Not Found", exception.Message);
        }
        [Fact]
        public async Task employeeRoleMapping_GetSuccessFullyById()
        {
            var employeeRoleMappingid = 1;
            var employeeRoleMapping = new EmployeeRoleMapping { Id = employeeRoleMappingid };
            _mock.Setup(x => x.GetRoleMappingById(employeeRoleMappingid)).ReturnsAsync(employeeRoleMapping);
            await _mock.Object.GetRoleMappingById(employeeRoleMappingid);
            _mock.Verify(x => x.GetRoleMappingById(employeeRoleMappingid), Times.Once());
        }
        [Fact]
        public async Task employeeRoleMapping_IdNotFound()
        {
            var employeeRoleMappingid = 1;
            var employeeRoleMapping = new EmployeeRoleMapping { Id = employeeRoleMappingid };
            _mock.Setup(x => x.GetRoleMappingById(employeeRoleMappingid)).ThrowsAsync(new ArgumentException("EmployeeRoleMappingId Not found"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.GetRoleMappingById(employeeRoleMappingid));
            Assert.Equal("EmployeeRoleMappingId Not found", exception.Message);
        }
        [Fact]
        public async Task employeeRoleMapping_DeleteSuccessfully()
        {
            var employeeRoleMappingid = 1;
            _mock.Setup(x => x.DeleteEmployeeRoleMapping(employeeRoleMappingid)).ReturnsAsync(true);
            var result = await _mock.Object.DeleteEmployeeRoleMapping(employeeRoleMappingid);
            Assert.True(result);
            _mock.Verify(x => x.DeleteEmployeeRoleMapping(employeeRoleMappingid), Times.Once);
        }
        [Fact]
        public async Task employeeRoleMapping_DeleteThrowException_WhenNotFound()
        {
            var employeeRoleMappingid = 1;
            _mock.Setup(x => x.DeleteEmployeeRoleMapping(employeeRoleMappingid)).ThrowsAsync(new KeyNotFoundException("EmployeeRoleMapping Not found"));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.DeleteEmployeeRoleMapping(employeeRoleMappingid));
            Assert.Equal("EmployeeRoleMapping Not found", exception.Message);
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

            var result = await _mock.Object.UpdateEmployeeRoleMapping(updateEmployeeRoleMapping);
            Assert.True(result);
            _mock.Verify(x => x.UpdateEmployeeRoleMapping(It.Is<EmployeeRoleMapping>(r =>
                        r.Id == updateEmployeeRoleMapping.Id &&
                        r.RoleId == updateEmployeeRoleMapping.RoleId)), Times.Once);
        }

        [Fact]
        public async Task employeeRoleMapping_ThrowException_WhenAttendanceNotFound()
        {
            var updateEmployeeRoleMapping = new EmployeeRoleMapping()
            {
                Id = 999,//id does not exist
                EmployeeId = 1,
                RoleId = 1,
            };
            _mock.Setup(x => x.UpdateEmployeeRoleMapping(It.IsAny<EmployeeRoleMapping>())).Throws(new ArgumentException("EmployeeRoleMapping not found"));

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.UpdateEmployeeRoleMapping(updateEmployeeRoleMapping));
            Assert.Equal("EmployeeRoleMapping not found", exception.Message);
        }
    }
}
