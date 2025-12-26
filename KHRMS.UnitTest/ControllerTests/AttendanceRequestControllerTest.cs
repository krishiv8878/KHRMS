using System.Security.Claims;
using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Request;
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
            var employee = new Employee { Id = attendanceRequest.Id };
            _mock.Setup(x=>x.GetByIdAsync(attendanceRequest.EmployeeId)).ReturnsAsync(attendanceRequest);
            _mock.Setup(x => x.AddAsync(It.IsAny<AttendanceRequestDTO>(), It.IsAny<ClaimsPrincipal>())).Returns(Task.CompletedTask);

            var result = await _controller.AddAttendanceRequest(attendanceRequestDTO);
            Assert.NotNull(result);
            _mock.Verify(x => x.AddAsync(It.IsAny<AttendanceRequestDTO>(), It.IsAny<ClaimsPrincipal>()), Times.Once);
        }
     
        [Fact]
        public async Task Get_AllAttendanceRequests_WhenCalled_ReturnsSuccess()
        {
            var getattendanceRequest = new List<AttendanceRequestUpdateDTO>
            {
                new AttendanceRequestUpdateDTO{RequestType="String",RequestedDate = new DateTime(2025, 3, 13, 10, 0, 0),Reason="Forgot",Status="Not Approved",clockIn = new DateTime(2025, 3, 13, 10, 30, 0),clockOut = new DateTime(2025, 3, 13, 7, 0, 0)},
                new AttendanceRequestUpdateDTO{RequestType="String",RequestedDate = new DateTime(2025, 3, 14, 10, 0, 0),Reason="Forgot",Status="Not Approved",clockIn = new DateTime(2025, 3, 14, 10, 30, 0),clockOut = new DateTime(2025, 3, 14, 7, 0, 0)},
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

            var result = await _controller.UpdateAttendanceRequest(UpdateattendanceRequest);
            Assert.NotNull(result);
            
            _mock.Verify(x=>x.UpdateAsync(It.Is<AttendanceRequestUpdateDTO>(r => 
                r.Id == UpdateattendanceRequest.Id && 
                r.Status == UpdateattendanceRequest.Status)), Times.Once());
        }
    }
}

