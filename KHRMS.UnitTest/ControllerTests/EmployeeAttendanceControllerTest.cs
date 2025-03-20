using KHRMS.Core;
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
            EmployeeAttendance employeeAttendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = DateTime.Now,
                ClockOut = DateTime.Now,
                EmployeeId = 1,
                TotalHours = new TimeSpan(7),
                EffectiveHours = new TimeSpan(6),
            };
            _mock.Setup(x => x.AddAsync(It.IsAny<EmployeeAttendance>())).Returns(Task.CompletedTask);

            var result = _controller.AddEmployeeAttendanceRequest(employeeAttendance);
            Assert.NotNull(result);
            _mock.Verify(x => x.AddAsync(It.IsAny<EmployeeAttendance>()), Times.Once);
        }

        [Fact]
        public async Task Get_AllEmployeeAttendances_WhenCalled_ShouldReturnListOfAttendances()
        {
            var employeeAttendance = new List<EmployeeAttendance>
            {
                new EmployeeAttendance() {Id = 1,ClockIn=DateTime.Now,ClockOut=DateTime.Now,EmployeeId=1,TotalHours = new TimeSpan(7), EffectiveHours = new TimeSpan(6)},
                new EmployeeAttendance() {Id = 2,ClockIn=DateTime.Now,ClockOut=DateTime.Now,EmployeeId=2,TotalHours = new TimeSpan(7), EffectiveHours = new TimeSpan(6),}
            };
            _mock.Setup(x => x.GetAllAsync()).ReturnsAsync(employeeAttendance);
            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnValue = Assert.IsType<KHRMS.Infrastructure.ApiResponse<IEnumerable<EmployeeAttendance>>>(okResult.Value);

            Assert.NotNull(result);
            Assert.Equal(employeeAttendance.Count(), returnValue.Data.Count());
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
            var employeeAttendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = DateTime.Now,
                ClockOut = DateTime.Now,
                EmployeeId = 1,
                TotalHours = new TimeSpan(7),
                EffectiveHours = new TimeSpan(6),
            };
            var updateEmployeeattendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = DateTime.Now,
                ClockOut = new DateTime(2025, 3, 17, 7, 0, 0),//update clockOut time
                EmployeeId = 1,
                TotalHours = new TimeSpan(7),
                EffectiveHours = new TimeSpan(6),
            };

            _mock.Setup(x => x.UpdateAsync(It.IsAny<EmployeeAttendance>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateEmployeeAttendanceRequest(updateEmployeeattendance);
            Assert.NotNull(result);

            _mock.Verify(x => x.UpdateAsync(It.Is<EmployeeAttendance>(r =>
                r.Id == updateEmployeeattendance.Id &&
                r.ClockOut == updateEmployeeattendance.ClockOut)), Times.Once());
        }
    }
}
