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
        private readonly Mock<ILeaveTypeService> _mock;
        private readonly LeaveTypeController _controller;
        public LeaveTypeControllerTest()
        {
            _mock= new Mock<ILeaveTypeService>();
            _controller = new LeaveTypeController(_mock.Object );
        }


        [Fact]
        public void Get_AllLeaveTypes_WhenCalled_ShouldReturnSuccess()
        {
            var leavetype = new List<Core.LeaveType>
            {
                new LeaveType{
                    Id = 1,
                    Type = "Full Type",
                    Description = "string" 
                }
            };
            _mock.Setup(x => x.GetAllLeaveType()).ReturnsAsync(leavetype);
            var result = _controller.GetLeaveType();
            Assert.NotNull(result);
            _mock.Verify(x=>x.GetAllLeaveType(), Times.Once()); 
        }

        [Fact]
        public void Add_LeaveType_WhenValidInputProvided_ShouldReturnSuccess()
        {
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            _mock.Setup(x => x.AddLeaveType(It.IsAny<LeaveType>())).ReturnsAsync(true);
            var result = _controller.AddLeaveType(leavetype);
            Assert.NotNull(result);
            _mock.Verify(x => x.AddLeaveType(It.IsAny<LeaveType>()), Times.Once());
        }

        [Fact]
        public async Task Update_LeaveType_WhenExistingLeaveTypeUpdated_ShouldReturnSuccess()
        {
            var leavetype = new LeaveType
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };

            _mock.Setup(x => x.UpdateLeaveType(leavetype)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateLeaveType(leavetype);

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
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            _mock.Setup(x => x.DeleteLeaveType(leavetype.Id)).ReturnsAsync(true);

            var result = _controller.DeleteLeaveType(leavetype.Id);
            Assert.NotNull(result);

            _mock.Verify(x => x.DeleteLeaveType(leavetype.Id), Times.Once);
        }
    }
}
