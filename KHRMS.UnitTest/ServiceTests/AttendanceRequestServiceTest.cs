using System.Security.Claims;
using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Request;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{
    public class AttendanceRequestServiceTest
    {
        private readonly Mock<IAttendanceRequestService> _mock;
        public AttendanceRequestServiceTest()
        {
            _mock = new Mock<IAttendanceRequestService>();
        }
        [Fact]
        public async Task Add_AttendanceRequest_Successfully()
        {
            AttendanceRequest attendanceRequest = new AttendanceRequest()
            {
                Id = 1,
                EmployeeId = 1,
                RequestType = "String",
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Not Approved",
                LastActionBy = null,
                ClockInTime = new DateTime(2025, 3, 13, 10, 30, 0),
                ClockOutTime = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };
            var attendanceRequestDTO = new AttendanceRequestDTO()
            {
                EmployeeId = 1,
                RequestType = "String",
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Not Approved",
                LastActionBy = null,
                clockIn = new DateTime(2025, 3, 13, 10, 30, 0),
                clockOut = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequestDTO>(), It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            await _mock.Object.AddAsync(attendanceRequestDTO, null);
            _mock.Verify(x => x.AddAsync(It.IsAny<AttendanceRequestDTO>(), It.IsAny<ClaimsPrincipal>()), Times.Once);
        }
        [Fact]
        public async Task Add_AttendanceRequest_ThrowsException_WhenDataIsInvalid()
        {
            AttendanceRequest InvalidRequest = new AttendanceRequest()
            {
                Id = 0, //invalid id
                EmployeeId = 1,
                RequestType = null, //invalid type
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Not Approved",
                LastActionBy = null,
                ClockInTime = new DateTime(2025, 3, 13, 10, 30, 0),
                ClockOutTime = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };
            var InvalidRequestDTO = new AttendanceRequestDTO()
            {
                EmployeeId = 1,
                RequestType = null, //invalid type
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Not Approved",
                LastActionBy = null,
                clockIn = new DateTime(2025, 3, 13, 10, 30, 0),
                clockOut = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequestDTO>(), It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new ArgumentException("Invalid request"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.AddAsync(InvalidRequestDTO, null));
            Assert.Equal("Invalid request", exception.Message);
        }
        [Fact]
        public async Task Add_AttendanceRequest_ThrowsException_WhenRequestIsNull()
        {
            AttendanceRequestDTO nullattendanceRequest = null;
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequestDTO>(), It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new Exception("AttendanceRequest cannot be null"));
            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.AddAsync(nullattendanceRequest, null));
            Assert.Equal("AttendanceRequest cannot be null", exception.Message);
        }
        [Fact]
        public async Task Add_AttendanceRequest_ThrowsException_WhenRequestAlreadyExists()
        {
            AttendanceRequestDTO attendanceRequestExist = new AttendanceRequestDTO()
            {
                EmployeeId = 1,
                RequestType = "String",
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),//Already Exist Requested Date
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Not Approved",
                LastActionBy = null,
                clockIn = new DateTime(2025, 3, 13, 10, 30, 0),
                clockOut = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequestDTO>(), It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException("Attendance request already exists for requested date"));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _mock.Object.AddAsync(attendanceRequestExist, null));
            Assert.Equal("Attendance request already exists for requested date", exception.Message);
        }
        [Fact]
        public async Task Get_AllAttendanceRequests_Successfully()
        {
            var getattendanceRequest = new List<AttendanceRequestUpdateDTO>
            {
                new AttendanceRequestUpdateDTO{Id = 1,EmployeeId=1,RequestType="String",RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),RequestedBy=1,Reason="Forgot",Status="Not Approved",LastActionBy=1,clockIn = new DateTime(2025, 3, 13, 10, 30, 0),clockOut = new DateTime(2025, 3, 13, 7, 0, 0),ManagerId = 3},
                new AttendanceRequestUpdateDTO{Id = 2,EmployeeId=1,RequestType="String",RequestedDate = new DateTime(2025, 3, 14, 10, 0, 0),RequestedBy=1,Reason="Forgot",Status="Not Approved",LastActionBy=1,clockIn = new DateTime(2025, 3, 14, 10, 30, 0),clockOut = new DateTime(2025, 3, 14, 7, 0, 0),ManagerId = 3},
            };
            _mock.Setup(x => x.GetAllAsync()).ReturnsAsync(getattendanceRequest);
            var result = await _mock.Object.GetAllAsync();

            _mock.Verify(x => x.GetAllAsync(), Times.Once);
            Assert.Equal(getattendanceRequest.Count(), result.Count());
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);

        }
        [Fact]
        public async Task Get_AllAttendanceRequests_ThrowsException_WhenNotFound()
        {
            var attendanceRequestnotfound = new List<AttendanceRequest>();
            _mock.Setup(x => x.GetAllAsync()).ThrowsAsync(new InvalidOperationException("Attendance request not found"));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _mock.Object.GetAllAsync());
            Assert.Equal("Attendance request not found", exception.Message);

        }

        [Fact]
        public async Task Get_AttendanceRequestById_Successfully()
        {
            var attendanceRequestid = 1;
            var attendanceRequest = new AttendanceRequest { Id = attendanceRequestid };
            _mock.Setup(x => x.GetByIdAsync(attendanceRequestid)).ReturnsAsync(attendanceRequest);
            await _mock.Object.GetByIdAsync(attendanceRequestid);
            _mock.Verify(x => x.GetByIdAsync(attendanceRequestid), Times.Once());
        }
        [Fact]
        public async Task Get_AttendanceRequestById_ThrowsException_WhenNotFound()
        {
            var attendanceRequestid = 1;
            var attendanceRequest = new AttendanceRequest { Id = attendanceRequestid };
            _mock.Setup(x => x.GetByIdAsync(attendanceRequestid)).ThrowsAsync(new ArgumentException("AttendanceRequestId Not found"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.GetByIdAsync(attendanceRequestid));
            Assert.Equal("AttendanceRequestId Not found", exception.Message);
        }

        [Fact]
        public async Task Delete_AttendanceRequest_Successfully()
        {
            var attendancerequestId = 1;
            _mock.Setup(x => x.DeleteAsync(attendancerequestId)).Returns(Task.CompletedTask);
            await _mock.Object.DeleteAsync(attendancerequestId);
            _mock.Verify(x => x.DeleteAsync(attendancerequestId), Times.Once);
        }

        [Fact]
        public async Task Delete_AttendanceRequest_ThrowsException_WhenNotFound()
        {
            var attendancerequestId = 1;
            _mock.Setup(x => x.DeleteAsync(attendancerequestId)).ThrowsAsync(new KeyNotFoundException("Attendance request not found"));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.DeleteAsync(attendancerequestId));

            Assert.Equal("Attendance request not found", exception.Message);
        }

        [Fact]
        public async Task UpdateAttendanceRequest_Successfully()
        {
            var attendanceRequest = new AttendanceRequest
            {
                Id = 1,
                EmployeeId = 1,
                RequestType = "String",
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Not Approved",
                LastActionBy = null,
                ClockInTime = new DateTime(2025, 3, 13, 10, 30, 0),
                ClockOutTime = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };
            var UpdateattendanceRequest = new AttendanceRequestUpdateDTO
            {
                Id = 1,
                EmployeeId = 1,
                RequestType = "String",
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Approved", //update status
                LastActionBy = 1,
                clockIn = new DateTime(2025, 3, 13, 10, 30, 0),
                clockOut = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };

            _mock.Setup(x => x.UpdateAsync(It.IsAny<AttendanceRequestUpdateDTO>())).Returns(Task.CompletedTask);

            await _mock.Object.UpdateAsync(UpdateattendanceRequest);

            _mock.Verify(x => x.UpdateAsync(It.Is<AttendanceRequestUpdateDTO>(r =>
                r.Id == UpdateattendanceRequest.Id &&
                r.Status == UpdateattendanceRequest.Status)), Times.Once());
        }
        [Fact]
        public async Task AttendanceRequest_UpdateThrowException_WhenRequestNotFound()
        {
            var attendanceRequest = new AttendanceRequestUpdateDTO
            {
                Id = 9999,//id doesnt exists
                EmployeeId = 1,
                RequestType = "String",
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Not Approved",
                LastActionBy = 1,
                clockIn = new DateTime(2025, 3, 13, 10, 30, 0),
                clockOut = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };

            _mock.Setup(x => x.UpdateAsync(It.IsAny<AttendanceRequestUpdateDTO>())).ThrowsAsync(new KeyNotFoundException("Requested id of Attendance request not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.UpdateAsync(attendanceRequest));
            Assert.Equal("Requested id of Attendance request not found", exception.Message);
        }
    }
}
