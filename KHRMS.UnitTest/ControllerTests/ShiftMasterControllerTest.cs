using KHRMS.Core;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace KHRMS.UnitTest.ControllerTests
{
    public class ShiftMasterControllerTest
    {
        private readonly Mock<IShiftService> _mock;
        private readonly ShiftController _controller;
        public ShiftMasterControllerTest()
        {
            _mock = new Mock<IShiftService>();
            _controller = new ShiftController(_mock.Object);
        }
        [Fact]
        public async Task Add_Shift_ShouldReturnSuccess_WhenShiftIsAdded()
        {
            ShiftMaster shiftMaster = new ShiftMaster()
            {
                Id = 1,
                ShiftName = "Night",
                StartTime = "10:20:00",
                EndTime = "07:10:00"
            };
            _mock.Setup(x => x.AddShiftAsync(It.IsAny<ShiftMaster>())).Returns(Task.CompletedTask);

            var result = _controller.AddShift(shiftMaster);
            Assert.NotNull(result);
            _mock.Verify(x => x.AddShiftAsync(It.IsAny<ShiftMaster>()), Times.Once);
        }

        [Fact]
        public async Task Get_AllShifts_ShouldReturnSuccess_WhenShiftsExist()
        {
            var shiftMaster = new List<ShiftMaster>()
            {
                new ShiftMaster{Id=1,ShiftName="Night",StartTime = "10:20:00",EndTime = "07:10:10"},
                new ShiftMaster{Id=2,ShiftName="Day",StartTime = "07:00:00",EndTime = "10:10:00"},
            };
            _mock.Setup(x => x.GetAllShiftsAsync()).ReturnsAsync(shiftMaster);
            var result = await _controller.GetAllShifts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnValue = Assert.IsType<KHRMS.Infrastructure.ApiResponse<IEnumerable<ShiftMaster>>>(okResult.Value);
            Assert.NotNull(result);
            Assert.Equal(shiftMaster.Count(), returnValue.Data.Count());
            Assert.Contains(returnValue.Data, r => r.Id == 1);
            Assert.Contains(returnValue.Data, r => r.Id == 2);
            _mock.Verify(x => x.GetAllShiftsAsync(), Times.Once);

        }

        [Fact]
        public async Task Delete_Shift_ShouldReturnSuccess_WhenShiftIsDeleted()
        {
            var shiftMasterId = 1;
            var shiftMaster = new ShiftMaster { Id = shiftMasterId };
            _mock.Setup(x => x.GetShiftByIdAsync(shiftMasterId)).ReturnsAsync(shiftMaster);
            _mock.Setup(x => x.DeleteShiftAsync(shiftMasterId)).Returns(Task.CompletedTask);
            var result = await _controller.DeleteShift(shiftMasterId);
            Assert.NotNull(result);
            _mock.Verify(x => x.DeleteShiftAsync(shiftMasterId), Times.Once);
        }

        [Fact]
        public async Task Update_Shift_ShouldReturnSuccess_WhenShiftIsUpdated()
        {
            // Arrange
            var shiftMaster = new ShiftMaster
            {
                Id = 1,
                ShiftName = "Night",
                StartTime = "10:20:00",
                EndTime = "07:10:00"
            };

            var updatedShiftMaster = new ShiftMaster
            {
                Id = 1,
                ShiftName = "Day", // updated shift name
                StartTime = "10:20:00",
                EndTime = "07:10:00"
            };

            _mock.Setup(x => x.UpdateShiftAsync(It.IsAny<ShiftMaster>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(updatedShiftMaster.Id, updatedShiftMaster);
            var okResult = result as OkObjectResult;

            // Assert
            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            _mock.Verify(x => x.UpdateShiftAsync(It.Is<ShiftMaster>(r =>
                r.Id == updatedShiftMaster.Id &&
                r.ShiftName == updatedShiftMaster.ShiftName)), Times.Once());
        }

    }
}

