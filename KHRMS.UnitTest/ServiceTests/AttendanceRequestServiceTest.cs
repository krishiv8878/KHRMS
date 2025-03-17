using System.Security.Claims;
using KHRMS.Core;
using KHRMS.Services;
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
        public async Task AttendanceRequest_AddSuccessfully()
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
                ClockInTime = new DateTime(2025,3,13,10,30,0),
                ClockOutTime = new DateTime(2025,3,13,7,0,0),
                ManagerId = 3
            };
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequest>(), It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            await _mock.Object.AddAsync(attendanceRequest, null);
            _mock.Verify(x => x.AddAsync(It.IsAny<AttendanceRequest>(), It.IsAny<ClaimsPrincipal>()), Times.Once);
        }
        [Fact]
        public async Task AttendanceRequest_ThrowException_WhenDataInValid()
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
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequest>(), It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new ArgumentException("Invalid request"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.AddAsync(InvalidRequest, null));
            Assert.Equal("Invalid request", exception.Message);
        }
        [Fact]
        public async Task AttendanceRequest_ThrowException_WhenDataIsNull()
        {
            AttendanceRequest nullattendanceRequest = null;
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequest>(), It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new Exception("AttendanceRequest cannot be null"));
            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.AddAsync(nullattendanceRequest, null));
            Assert.Equal("AttendanceRequest cannot be null", exception.Message);
        }
        [Fact]
        public async Task AttendanceRequest_ThrowException_WhenDataIsExist()
        {
            AttendanceRequest attendanceRequestExist = new AttendanceRequest()
            {
                Id = 1, 
                EmployeeId = 1,
                RequestType = "String", 
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),//Already Exist Requested Date
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Not Approved",
                LastActionBy = null,
                ClockInTime = new DateTime(2025, 3, 13, 10, 30, 0),
                ClockOutTime = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequest>(), It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new InvalidOperationException("Attendance request already exists for requested date"));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _mock.Object.AddAsync(attendanceRequestExist, null));
            Assert.Equal("Attendance request already exists for requested date", exception.Message);
        }
        [Fact]
        public async Task AttendanceRequest_GetSuccessfully()
        {
            var getattendanceRequest = new List<AttendanceRequest>
            {
                new AttendanceRequest{Id = 1,EmployeeId=1,RequestType="String",RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),RequestedBy=1,Reason="Forgot",Status="Not Approved",LastActionBy=null,ClockInTime = new DateTime(2025, 3, 13, 10, 30, 0),ClockOutTime = new DateTime(2025, 3, 13, 7, 0, 0),ManagerId = 3},
                new AttendanceRequest{Id = 2,EmployeeId=1,RequestType="String",RequestedDate = new DateTime(2025, 3, 14, 10, 0, 0),RequestedBy=1,Reason="Forgot",Status="Not Approved",LastActionBy=null,ClockInTime = new DateTime(2025, 3, 14, 10, 30, 0),ClockOutTime = new DateTime(2025, 3, 14, 7, 0, 0),ManagerId = 3},
            };
            _mock.Setup(x => x.GetAllAsync()).ReturnsAsync(getattendanceRequest);
            var result = await _mock.Object.GetAllAsync();

            _mock.Verify(x => x.GetAllAsync(), Times.Once);
            Assert.Equal(getattendanceRequest.Count(), result.Count());
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);

        }
        [Fact]
        public async Task AttendanceRequest_NotFound()
        {
            var attendanceRequestnotfound = new List<AttendanceRequest>();
            _mock.Setup(x => x.GetAllAsync()).ThrowsAsync(new InvalidOperationException("Attendance request not found"));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _mock.Object.GetAllAsync());
            Assert.Equal("Attendance request not found", exception.Message);

        }

        [Fact]
        public async Task AttendanceRequest_GetSuccessFullyById()
        {
            var attendanceRequestid = 1;
            var attendanceRequest = new AttendanceRequest { Id = attendanceRequestid };
            _mock.Setup(x => x.GetByIdAsync(attendanceRequestid)).ReturnsAsync(attendanceRequest);
            await _mock.Object.GetByIdAsync(attendanceRequestid);
            _mock.Verify(x => x.GetByIdAsync(attendanceRequestid), Times.Once());
        }
        [Fact]
        public async Task AttendanceRequest_IdNotFound()
        {
            var attendanceRequestid = 1;
            var attendanceRequest = new AttendanceRequest { Id = attendanceRequestid };
            _mock.Setup(x => x.GetByIdAsync(attendanceRequestid)).ThrowsAsync(new ArgumentException("AttendanceRequestId Not found"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.GetByIdAsync(attendanceRequestid));
            Assert.Equal("AttendanceRequestId Not found", exception.Message);
        }

        [Fact]
        public async Task AttendanceRequest_DeleteSuccessfully()
        {
            var attendancerequestId = 1;
            _mock.Setup(x=>x.DeleteAsync(attendancerequestId)).Returns(Task.CompletedTask);
            await _mock.Object.DeleteAsync(attendancerequestId);
            _mock.Verify(x=>x.DeleteAsync(attendancerequestId), Times.Once);
        }

        [Fact]
        public async Task AttendanceRequest_DeleteThrowException_WhenRequestNotFound()
        {
            var attendancerequestId = 1;
            _mock.Setup(x => x.DeleteAsync(attendancerequestId)).ThrowsAsync(new KeyNotFoundException("Attendance request not found"));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.DeleteAsync(attendancerequestId));

            Assert.Equal("Attendance request not found", exception.Message);
        }

        [Fact]
        public async Task AttendanceRequest_UpdateSuccessfully()
        {
            var attendanceRequest = new AttendanceRequest
            {
                Id = 1,EmployeeId=1,RequestType="String",RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),RequestedBy=1,Reason="Forgot",Status="Not Approved",LastActionBy=null,ClockInTime = new DateTime(2025, 3, 13, 10, 30, 0),ClockOutTime = new DateTime(2025, 3, 13, 7, 0, 0),ManagerId = 3
            };
            var UpdateattendanceRequest = new AttendanceRequest
            {
                Id = 1,
                EmployeeId = 1,
                RequestType = "String",
                RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),
                RequestedBy = 1,
                Reason = "Forgot",
                Status = "Approved", //update status
                LastActionBy = null,
                ClockInTime = new DateTime(2025, 3, 13, 10, 30, 0),
                ClockOutTime = new DateTime(2025, 3, 13, 7, 0, 0),
                ManagerId = 3
            };

            _mock.Setup(x => x.UpdateAsync(It.IsAny<AttendanceRequest>())).Returns(Task.CompletedTask);

            await _mock.Object.UpdateAsync(UpdateattendanceRequest);
            
            _mock.Verify(x=>x.UpdateAsync(It.Is<AttendanceRequest>(r => 
                r.Id == UpdateattendanceRequest.Id && 
                r.Status == UpdateattendanceRequest.Status)), Times.Once());
        }
        [Fact]
        public async Task AttendanceRequest_UpdateThrowException_WhenRequestNotFound()
        {
            var attendanceRequest = new AttendanceRequest
            {
                Id = 9999,//id doesnt exists
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

            _mock.Setup(x => x.UpdateAsync(It.IsAny<AttendanceRequest>())).ThrowsAsync(new KeyNotFoundException("Requested id of Attendance request not found")); 

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(()=> _mock.Object.UpdateAsync(attendanceRequest));
            Assert.Equal("Requested id of Attendance request not found", exception.Message);
        }
    }
}
