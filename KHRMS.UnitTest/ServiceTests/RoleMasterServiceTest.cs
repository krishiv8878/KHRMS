using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{
    public class RoleMasterServiceTest
    {
        private readonly Mock<IRoleMasterService> _mockService;
        private readonly IRoleMasterService _service;

        public RoleMasterServiceTest()
        {
            _mockService = new Mock<IRoleMasterService>();
            _service = _mockService.Object;
        }

        [Fact]
        public async Task Add_RoleMaster_ShouldReturnSuccess()
        {
            var roleMaster = new RoleMaster { Id = 1, RoleName = "Admin" };
            _mockService.Setup(x => x.AddRoleMaster(roleMaster)).ReturnsAsync(true);

            var result = await _service.AddRoleMaster(roleMaster);
            Assert.True(result);
            _mockService.Verify(x => x.AddRoleMaster(roleMaster), Times.Once);
        }

        [Fact]
        public async Task Get_AllRoleMasters_ShouldReturnAllRoles()
        {
            var roleMasters = new List<RoleMaster>
            {
                new RoleMaster { Id = 1, RoleName = "Admin" },
                new RoleMaster { Id = 2, RoleName = "User" }
            };
            _mockService.Setup(x => x.GetAllRoleMaster()).ReturnsAsync(roleMasters);

            var result = await _service.GetAllRoleMaster();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task Get_RoleMasterById_ShouldReturnRole()
        {
            var roleMaster = new RoleMaster { Id = 1, RoleName = "Admin" };
            _mockService.Setup(x => x.GetRoleMasterById(1)).ReturnsAsync(roleMaster);

            var result = await _service.GetRoleMasterById(1);
            Assert.NotNull(result);
            Assert.Equal("Admin", result.RoleName);
        }

        [Fact]
        public async Task Get_RoleMasterById_ShouldReturnNull_WhenNotFound()
        {
            _mockService.Setup(x => x.GetRoleMasterById(999)).ReturnsAsync((RoleMaster)null);

            var result = await _service.GetRoleMasterById(999);
            Assert.Null(result);
        }

        [Fact]
        public async Task Update_RoleMaster_ShouldReturnSuccess()
        {
            var roleMaster = new RoleMaster { Id = 1, RoleName = "Admin" };
            _mockService.Setup(x => x.UpdateRoleMaster(roleMaster)).ReturnsAsync(true);

            var result = await _service.UpdateRoleMaster(roleMaster);
            Assert.True(result);
            _mockService.Verify(x => x.UpdateRoleMaster(roleMaster), Times.Once);
        }

        [Fact]
        public async Task DeleteRoleMaster_ShouldReturnSuccess()
        {
            var Id = 1;
            _mockService.Setup(x => x.DeleteRoleMaster(Id)).ReturnsAsync(true);

            var result = await _service.DeleteRoleMaster(Id);
            Assert.True(result);
            _mockService.Verify(x => x.DeleteRoleMaster(Id), Times.Once);
        }

        [Fact]
        public async Task Delete_RoleMaster_ShouldReturnFalse_WhenNotFound()
        {
            var Id = 999;
            _mockService.Setup(x => x.DeleteRoleMaster(Id)).ReturnsAsync(false);

            var result = await _service.DeleteRoleMaster(Id);
            Assert.False(result);
        }
    }

}

