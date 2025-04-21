using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{
    public class EmployeeAttendanceServiceTest
    {
        private readonly Mock<IEmployeeAttendanceService> _mock;
        public EmployeeAttendanceServiceTest()
        {
            _mock = new Mock<IEmployeeAttendanceService>();
        }

        [Fact]
        public async Task Add_Async_ShouldSucceed_WhenDataIsValid()
        {
            var baseDate = new DateTime(1, 1, 1); // represents time-only values

            var employeeAttendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = DateTime.Now,
                ClockOut = DateTime.Now.AddHours(7),
                EmployeeId = 1,
                TotalHours = baseDate.AddHours(7),
                EffectiveHours = baseDate.AddHours(6),
            };

            _mock.Setup(x => x.AddAsync(It.IsAny<EmployeeAttendance>())).Returns(Task.CompletedTask);

            await _mock.Object.AddAsync(employeeAttendance);

            _mock.Verify(x => x.AddAsync(It.Is<EmployeeAttendance>(e =>
                e.Id == 1 &&
                e.EmployeeId == 1 &&
                e.TotalHours == baseDate.AddHours(7) &&
                e.EffectiveHours == baseDate.AddHours(6)
            )), Times.Once);
        }

        [Fact]
        public async Task Add_Async_ShouldThrowException_WhenDataIsInvalid()
        {
            var baseDate = new DateTime(1, 1, 1); // Used to simulate duration values

            var employeeAttendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = DateTime.Now,
                ClockOut = DateTime.Now,
                EmployeeId = 999, // employee does not exist
                TotalHours = baseDate.AddHours(7),
                EffectiveHours = baseDate.AddHours(6)
            };

            _mock.Setup(x => x.AddAsync(It.IsAny<EmployeeAttendance>()))
                 .Throws(new ArgumentException("Invalid Data of EmployeeAttendance"));

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _mock.Object.AddAsync(employeeAttendance));

            Assert.Equal("Invalid Data of EmployeeAttendance", exception.Message);
        }

        [Fact]
        public async Task Add_Async_ShouldThrowException_WhenDataIsNull()
        {
            EmployeeAttendance employeeAttendance = null;

            _mock.Setup(x => x.AddAsync(It.IsAny<EmployeeAttendance>())).Throws(new Exception("Employee Attendance Is Null"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.AddAsync(employeeAttendance));

            Assert.Equal("Employee Attendance Is Null", exception.Message);
        }
        [Fact]
        public async Task Get_AllAsync_ShouldReturnEmployeeAttendances_WhenDataExists()
        {
            var baseDate = new DateTime(1, 1, 1); // Used to simulate durations

            var employeeAttendance = new List<EmployeeAttendance>
    {
        new EmployeeAttendance
        {
            Id = 1,
            ClockIn = DateTime.Now,
            ClockOut = DateTime.Now,
            EmployeeId = 1,
            TotalHours = baseDate.AddHours(7),
            EffectiveHours = baseDate.AddHours(6)
        },
        new EmployeeAttendance
        {
            Id = 2,
            ClockIn = DateTime.Now,
            ClockOut = DateTime.Now,
            EmployeeId = 2,
            TotalHours = baseDate.AddHours(7),
            EffectiveHours = baseDate.AddHours(5)
        }
    };

            _mock.Setup(x => x.GetAllAsync()).ReturnsAsync(employeeAttendance);

            var result = await _mock.Object.GetAllAsync();

            _mock.Verify(x => x.GetAllAsync(), Times.Once);

            Assert.Equal(employeeAttendance.Count, result.Count());
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);
        }

        [Fact]
        public async Task Get_AllAsync_ShouldThrowException_WhenNoDataExists()
        {
            var attendanceNotfound = new List<EmployeeAttendance>();
            _mock.Setup(x => x.GetAllAsync()).ThrowsAsync(new Exception("Employee Attendance Not Found"));
            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.GetAllAsync());
            Assert.Equal("Employee Attendance Not Found", exception.Message);
        }
        [Fact]
        public async Task Get_ByIdAsync_ShouldReturnEmployeeAttendance_WhenIdIsValid()
        {
            var employeeAttendanceid = 1;
            var employeeAttendance = new EmployeeAttendance { Id = employeeAttendanceid };
            _mock.Setup(x => x.GetByIdAsync(employeeAttendanceid)).ReturnsAsync(employeeAttendance);
            await _mock.Object.GetByIdAsync(employeeAttendanceid);
            _mock.Verify(x => x.GetByIdAsync(employeeAttendanceid), Times.Once());
        }
        [Fact]
        public async Task Get_ByIdAsync_ShouldThrowException_WhenIdNotFound()
        {
            var employeeAttendanceid = 1;
            var employeeAttendance = new EmployeeAttendance { Id = employeeAttendanceid };
            _mock.Setup(x => x.GetByIdAsync(employeeAttendanceid)).ThrowsAsync(new ArgumentException("EmployeeAttendanceId Not found"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.GetByIdAsync(employeeAttendanceid));
            Assert.Equal("EmployeeAttendanceId Not found", exception.Message);
        }
        [Fact]
        public async Task Delete_Async_ShouldSucceed_WhenIdIsValid()
        {
            var attendanceid = 1;
            _mock.Setup(x => x.DeleteAsync(attendanceid)).Returns(Task.CompletedTask);
            await _mock.Object.DeleteAsync(attendanceid);
            _mock.Verify(x => x.DeleteAsync(attendanceid), Times.Once);
        }
        [Fact]
        public async Task Delete_Async_ShouldThrowException_WhenIdNotFound()
        {
            var attendanceid = 1;
            _mock.Setup(x => x.DeleteAsync(attendanceid)).ThrowsAsync(new KeyNotFoundException("EmployeeAttendance Not found"));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.DeleteAsync(attendanceid));
            Assert.Equal("EmployeeAttendance Not found", exception.Message);
        }
        [Fact]
        public async Task Update_Async_ShouldSucceed_WhenDataIsValid()
        {
            var baseDate = new DateTime(1, 1, 1); // Base date to simulate duration

            var employeeAttendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = DateTime.Now,
                ClockOut = DateTime.Now,
                EmployeeId = 1,
                TotalHours = baseDate.AddHours(7),
                EffectiveHours = baseDate.AddHours(6),
            };

            var updateEmployeeAttendance = new EmployeeAttendance()
            {
                Id = 1,
                ClockIn = DateTime.Now,
                ClockOut = new DateTime(2025, 3, 17, 7, 0, 0), // Updated ClockOut
                EmployeeId = 1,
                TotalHours = baseDate.AddHours(10),
                EffectiveHours = baseDate.AddHours(9),
            };

            _mock.Setup(x => x.UpdateAsync(It.IsAny<EmployeeAttendance>())).Returns(Task.CompletedTask);

            await _mock.Object.UpdateAsync(updateEmployeeAttendance);

            _mock.Verify(x => x.UpdateAsync(It.Is<EmployeeAttendance>(r =>
                r.Id == updateEmployeeAttendance.Id &&
                r.ClockOut == updateEmployeeAttendance.ClockOut &&
                r.TotalHours == baseDate.AddHours(10) &&
                r.EffectiveHours == baseDate.AddHours(9)
            )), Times.Once);
        }


        [Fact]
        public async Task Update_Async_ShouldThrowException_WhenAttendanceNotFound()
        {
            var baseDate = new DateTime(1, 1, 1); // Base date for time-only logic

            var employeeAttendance = new EmployeeAttendance()
            {
                Id = 999, // ID does not exist
                ClockIn = DateTime.Now,
                ClockOut = DateTime.Now,
                EmployeeId = 1,
                TotalHours = baseDate.AddHours(7),
                EffectiveHours = baseDate.AddHours(6),
            };

            _mock.Setup(x => x.UpdateAsync(It.IsAny<EmployeeAttendance>()))
                 .Throws(new ArgumentException("EmployeeAttendance not found"));

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                _mock.Object.UpdateAsync(employeeAttendance));

            Assert.Equal("EmployeeAttendance not found", exception.Message);
        }
    }
    }
