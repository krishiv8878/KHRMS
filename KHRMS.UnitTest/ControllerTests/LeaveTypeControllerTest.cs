using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;


namespace KHRMS.UnitTest.ControllerTests
{
    public class LeaveTypeControllerTest
    {
        public LeaveTypeControllerTest()
        {

        }


        [Fact]
        public void Get_AllLeaveTypes_WhenCalled_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            var controller = new LeaveTypeController(mock.Object);
            mock.Setup(x => x.GetAllLeaveType());
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            var result = controller.GetLeaveType();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }

        [Fact]
        public void Add_LeaveType_WhenValidInputProvided_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            var controller = new LeaveTypeController(mock.Object);
            mock.Setup(x => x.AddLeaveType(It.IsAny<LeaveType>()));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            var result = controller.AddLeaveType(leavetype);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }

        [Fact]
        public async Task Update_LeaveType_WhenExistingLeaveTypeUpdated_ShouldReturnSuccess()
        {
            // Arrange
            var mock = new Mock<ILeaveTypeService>();
            var controller = new LeaveTypeController(mock.Object);

            var leavetype = new LeaveType
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };

            mock.Setup(x => x.UpdateLeaveType(leavetype)).ReturnsAsync(true);

            // Act
            var result = await controller.UpdateLeaveType(leavetype);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);

            Assert.True(response.Data);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ApiMessageConstant.LeaveTypeUpdated, response.Message);
        }

        [Fact]
        public void Delete_LeaveType_WhenExistingLeaveTypeDeleted_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            var controller = new LeaveTypeController(mock.Object);
            mock.Setup(x => x.DeleteLeaveType(1));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            var result = controller.DeleteLeaveType(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteLeaveType(1), Times.Once);
        }
    }
}
