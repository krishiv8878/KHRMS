using System.Security.Claims;
using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class AttendanceRequestControllerTest
    {
        private readonly Mock<IAttendanceRequestService> _mock;
        private readonly AttendanceRequestController _controller;
        public AttendanceRequestControllerTest()
        {
            _mock = new Mock<IAttendanceRequestService>();
            _controller = new AttendanceRequestController(_mock.Object);
        }
        [Fact]
        public async Task Add_AttendanceRequest_WhenValidRequestProvided_ReturnsSuccess()
        { 
            var attendanceRequest = new AttendanceRequest()
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
            var employee = new Employee { Id = attendanceRequest.Id };
            _mock.Setup(x=>x.GetByIdAsync(attendanceRequest.EmployeeId)).ReturnsAsync(attendanceRequest);
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequest>(), It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            var result = await _controller.AddAttendanceRequest(attendanceRequest);
            Assert.NotNull(result);
            _mock.Verify(x => x.AddAsync(It.IsAny<AttendanceRequest>(), It.IsAny<ClaimsPrincipal>()), Times.Once);
        }
     
        [Fact]
        public async Task Get_AllAttendanceRequests_WhenCalled_ReturnsSuccess()
        {
            var getattendanceRequest = new List<AttendanceRequest>
            {
                new AttendanceRequest{Id = 1,EmployeeId=1,RequestType="String",RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),RequestedBy=1,Reason="Forgot",Status="Not Approved",LastActionBy=null,ClockInTime = new DateTime(2025, 3, 13, 10, 30, 0),ClockOutTime = new DateTime(2025, 3, 13, 7, 0, 0),ManagerId = 3},
                new AttendanceRequest{Id = 2,EmployeeId=1,RequestType="String",RequestedDate = new DateTime(2025, 3, 14, 10, 0, 0),RequestedBy=1,Reason="Forgot",Status="Not Approved",LastActionBy=null,ClockInTime = new DateTime(2025, 3, 14, 10, 30, 0),ClockOutTime = new DateTime(2025, 3, 14, 7, 0, 0),ManagerId = 3},
            };
            _mock.Setup(x => x.GetAllAsync()).ReturnsAsync(getattendanceRequest);
            var result = await _controller.GetAttendanceRequests();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnValue = Assert.IsType<KHRMS.Infrastructure.ApiResponse<List<AttendanceRequest>>>(okResult.Value);

            Assert.NotNull(returnValue);
            Assert.Equal(getattendanceRequest.Count(), returnValue.Data.Count()); 
            Assert.Contains(returnValue.Data, r => r.Id == 1);
            Assert.Contains(returnValue.Data, r => r.Id == 2);

            _mock.Verify(x => x.GetAllAsync(), Times.Once);

        }
        
        [Fact]
        public async Task Delete_AttendanceRequest_WhenRequestExists_ReturnsSuccess()
        {
            var attendancerequestId = 1;
            _mock.Setup(x=>x.DeleteAsync(attendancerequestId)).Returns(Task.CompletedTask);
            var result = await _controller.DeleteAttendanceRequest(attendancerequestId);
            Assert.NotNull(result);
            _mock.Verify(x=>x.DeleteAsync(attendancerequestId), Times.Once);
        }

        [Fact]
        public async Task Update_AttendanceRequest_WhenRequestExists_ReturnsSuccess()
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

            var result = await _controller.UpdateAttendanceRequest(UpdateattendanceRequest);
            Assert.NotNull(result);
            
            _mock.Verify(x=>x.UpdateAsync(It.Is<AttendanceRequest>(r => 
                r.Id == UpdateattendanceRequest.Id && 
                r.Status == UpdateattendanceRequest.Status)), Times.Once());
        }
    }
}

