using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class EmployeeAttendanceControllerTest
    {
        private readonly Mock<IEmployeeAttendanceService> _mock;
        private readonly EmployeeAttendanceController _controller;
        public EmployeeAttendanceControllerTest()
        {
            _mock = new Mock<IEmployeeAttendanceService>();
            _controller = new EmployeeAttendanceController(_mock.Object);
        }

        [Fact]
        public async Task Add_EmployeeAttendance_WhenValidAttendanceProvided_ShouldAddSuccessfully()
        {
            var now = DateTime.Now;
            var baseDate = new DateTime(1, 1, 1);

            var employeeAttendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = now,
                ClockOut = now.AddHours(9),
                EmployeeId = 1,
                TotalHours = 9,
                EffectiveHours = 8,
            };

            _mock.Setup(x => x.AddAsync(It.IsAny<EmployeeAttendance>())).Returns(Task.CompletedTask);

            var result = await _controller.AddEmployeeAttendanceRequest(employeeAttendance);

            Assert.NotNull(result);
            _mock.Verify(x => x.AddAsync(It.IsAny<EmployeeAttendance>()), Times.Once);
        }

        [Fact]
        public async Task Get_AllEmployeeAttendances_WhenCalled_ShouldReturnListOfAttendances()
        {
            var baseDate = new DateTime(1, 1, 1);

            var employeeAttendanceList = new List<EmployeeAttendance>
            {
                new EmployeeAttendance() { Id = 1, ClockIn = DateTime.Now, ClockOut = DateTime.Now.AddHours(8), EmployeeId = 1, TotalHours = 8, EffectiveHours = 7 },
                new EmployeeAttendance() { Id = 2, ClockIn = DateTime.Now, ClockOut = DateTime.Now.AddHours(9), EmployeeId = 2, TotalHours = 9, EffectiveHours = 8 }
            };

            _mock.Setup(x => x.GetAllAsync()).ReturnsAsync(employeeAttendanceList);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ApiResponse<IEnumerable<EmployeeAttendance>>>(okResult.Value);

            Assert.NotNull(result);
            Assert.Equal(employeeAttendanceList.Count, returnValue.Data.Count());
            Assert.Contains(returnValue.Data, r => r.Id == 1);
            Assert.Contains(returnValue.Data, r => r.Id == 2);
            _mock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task Delete_EmployeeAttendance_WhenAttendanceExists_ShouldDeleteSuccessfully()
        {
            var attendanceid = 1;
            var employeeAttendance = new EmployeeAttendance { Id = attendanceid };
            _mock.Setup(x => x.GetByIdAsync(attendanceid)).ReturnsAsync(employeeAttendance);
            _mock.Setup(x => x.DeleteAsync(attendanceid)).Returns(Task.CompletedTask);
            var result = await _controller.DeleteEmployeeAttendanceRequest(attendanceid);
            Assert.NotNull(result);
            _mock.Verify(x => x.DeleteAsync(attendanceid), Times.Once);
        }

        [Fact]
        public async Task Update_EmployeeAttendance_WhenValidUpdateProvided_ShouldUpdateSuccessfully()
        {
            var now = DateTime.Now;
            var baseDate = new DateTime(1, 1, 1);

            var updateEmployeeAttendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = now,
                ClockOut = now.AddHours(10), // updated ClockOut
                EmployeeId = 1,
                TotalHours = 10,
                EffectiveHours = 9,
            };

            _mock.Setup(x => x.UpdateAsync(It.IsAny<EmployeeAttendance>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateEmployeeAttendanceRequest(updateEmployeeAttendance);

            Assert.NotNull(result);
            _mock.Verify(x => x.UpdateAsync(It.Is<EmployeeAttendance>(r =>
                r.Id == updateEmployeeAttendance.Id &&
                r.ClockOut == updateEmployeeAttendance.ClockOut)), Times.Once);
        }
    }
}
