using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Request;
using Moq;
using NPOI.SS.Formula.Functions;

namespace KHRMS.UnitTest.ServiceTests
{
    public class LeaveRequestTypeServiceTest
    {
        private readonly Mock<ILeaveRequestTypeService> _mock;
        public LeaveRequestTypeServiceTest()
        {
            _mock = new Mock<ILeaveRequestTypeService>();
        }

        [Fact]
        public async Task Add_Async_ShouldSucceed_WhenDataIsValid()
        {
            var startdate = new DateTime(2025, 05, 10);
            var enddate = new DateTime(2025, 05, 15);
            var approveddate = new DateTime(2025, 05, 07);

            var leaveRequest = new LeaveRequest()
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = false,
                ApprovedBy = 0,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };

            _mock.Setup(x => x.AddLeaveRequestType(It.IsAny<LeaveRequest>())).ReturnsAsync(true);
            var result = await _mock.Object.AddLeaveRequestType(leaveRequest);
            Assert.True(result);
            _mock.Verify(x => x.AddLeaveRequestType(It.IsAny<LeaveRequest>()), Times.Once);
        }

        [Fact]
        public async Task Add_LeaveRequest_ShouldThrowException_WhenDataIsInvalid()
        {
            var startdate = new DateTime(2025, 05, 10);
            var enddate = new DateTime(2025, 05, 15);
            var approveddate = new DateTime(2025, 05, 07);

            var leaveRequest = new LeaveRequest()
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = false,
                ApprovedBy = 0,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };

            _mock.Setup(x => x.AddLeaveRequestType(It.IsAny<LeaveRequest>())).Throws(new ArgumentException("Invalid Data of LeaveRequest"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.AddLeaveRequestType(leaveRequest));
            Assert.Equal("Invalid Data of LeaveRequest", exception.Message);
        }

        [Fact]
        public async Task Add_LeaveRequest_ShouldThrowException_WhenDataIsNull()
        {
            LeaveRequest leaveRequest = null;

            _mock.Setup(x => x.AddLeaveRequestType(It.IsAny<LeaveRequest>())).Throws(new Exception("LeaveRequest Is Null"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.AddLeaveRequestType(leaveRequest));

            Assert.Equal("LeaveRequest Is Null", exception.Message);
        }

        [Fact]
        public async Task Get_AllLeaveRequest_ShouldReturnList_WhenDataExists()
        {
            var startdate = new DateTime(2025, 05, 10);
            var enddate = new DateTime(2025, 05, 15);
            var approveddate = new DateTime(2025, 05, 07);

            var leaveRequest = new List<LeaveReqestModel>
            {
                new LeaveReqestModel(){Id = 1,LeaveMode="",LeaveTypeName="Casual",StartDate = startdate,EndDate = enddate,IsApproved = false,LeaveReson = "Nothing"},
                new LeaveReqestModel(){Id = 2,LeaveMode="",LeaveTypeName="Sick",StartDate = startdate,EndDate = enddate,IsApproved = false,LeaveReson = "xyz"},
            };
            _mock.Setup(x => x.GetAllLeaveRequestType()).ReturnsAsync(leaveRequest);
            var result = await _mock.Object.GetAllLeaveRequestType();

            _mock.Verify(x => x.GetAllLeaveRequestType(), Times.Once);

            Assert.Equal(leaveRequest.Count(), result.Count());
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);
        }

        [Fact]
        public async Task Get_AllLeaveRequest_ShouldThrowException_WhenNoDataFound()
        {
            var leaveReqests = new List<LeaveReqestModel>();
            _mock.Setup(x => x.GetAllLeaveRequestType()).ThrowsAsync(new Exception("leaveReqests Not Found"));
            var exception = await Assert.ThrowsAsync<Exception>(() => _mock.Object.GetAllLeaveRequestType());
            Assert.Equal("leaveReqests Not Found", exception.Message);
        }

        [Fact]
        public async Task Get_LeaveRequestById_ShouldReturnLeaveRequest_WhenIdIsValid()
        {
            var leaveReqestsid = 1;
            var leaveReqests = new LeaveRequest { Id = leaveReqestsid };
            _mock.Setup(x => x.GetLeaveRequestTypeById(leaveReqestsid)).ReturnsAsync(leaveReqests);
            await _mock.Object.GetLeaveRequestTypeById(leaveReqestsid);
            _mock.Verify(x => x.GetLeaveRequestTypeById(leaveReqestsid), Times.Once());
        }

        [Fact]
        public async Task Get_LeaveRequestById_ShouldThrowException_WhenIdNotFound()
        {
            var leaveReqestsid = 1;
            var leaveReqests = new LeaveRequest { Id = leaveReqestsid };
            _mock.Setup(x => x.GetLeaveRequestTypeById(leaveReqestsid)).ThrowsAsync(new ArgumentException("LeaveRequest Not found"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.GetLeaveRequestTypeById(leaveReqestsid));
            Assert.Equal("LeaveRequest Not found", exception.Message);
        }

        [Fact]
        public async Task Delete_LeaveRequest_ShouldReturnTrue_WhenIdIsValid()
        {
            var leaveReqestsid = 1;
            _mock.Setup(x => x.DeleteLeaveRequestType(leaveReqestsid)).ReturnsAsync(true);
            var result = await _mock.Object.DeleteLeaveRequestType(leaveReqestsid);
            Assert.True(result);
            _mock.Verify(x => x.DeleteLeaveRequestType(leaveReqestsid), Times.Once);
        }
        [Fact]
        public async Task Delete_LeaveRequest_ShouldThrowException_WhenIdNotFound()
        {
            var leaveReqestsid = 1;
            _mock.Setup(x => x.DeleteLeaveRequestType(leaveReqestsid)).ThrowsAsync(new KeyNotFoundException("LeaveRequest Not found"));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.DeleteLeaveRequestType(leaveReqestsid));
            Assert.Equal("LeaveRequest Not found", exception.Message);
        }

        [Fact]
        public async Task Update_LeaveRequest_ShouldReturnTrue_WhenDataIsValid()
        {
            var startdate = new DateTime(2025, 05, 10);
            var enddate = new DateTime(2025, 05, 15);
            var approveddate = new DateTime(2025, 05, 07);

            var leaveRequest = new LeaveRequest()
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = false,
                ApprovedBy = 0,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };
            var UpdateleaveRequest = new LeaveRequest()
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 2, //update leavetype id
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = false,
                ApprovedBy = 0,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };
            _mock.Setup(x => x.UpdateLeaveRequestType(It.IsAny<LeaveRequest>())).ReturnsAsync(true);

            var result = await _mock.Object.UpdateLeaveRequestType(UpdateleaveRequest);
            Assert.True(result);
            _mock.Verify(x => x.UpdateLeaveRequestType(It.Is<LeaveRequest>(r =>
                        r.Id == UpdateleaveRequest.Id &&
                        r.EmployeeId == UpdateleaveRequest.EmployeeId)), Times.Once);
        }

        [Fact]
        public async Task Update_LeaveRequest_ShouldThrowException_WhenIdNotFound()
        {
            var startdate = new DateTime(2025, 05, 10);
            var enddate = new DateTime(2025, 05, 15);
            var approveddate = new DateTime(2025, 05, 07);
            var UpdateleaveRequest = new LeaveRequest()
            {
                Id = 999,
                EmployeeId = 1,
                LeaveTypeId = 2, //update leavetype id
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = false,
                ApprovedBy = 0,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };
            _mock.Setup(x => x.UpdateLeaveRequestType(It.IsAny<LeaveRequest>())).Throws(new ArgumentException("LeaveRequest not found"));

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.UpdateLeaveRequestType(UpdateleaveRequest));
            Assert.Equal("LeaveRequest not found", exception.Message);
        }

        [Fact]
        public async Task Approve_LeaveRequest_ShouldReturnTrue_WhenDataIsValid()
        {
            var startdate = new DateTime(2025, 05, 10);
            var enddate = new DateTime(2025, 05, 15);
            var approveddate = new DateTime(2025, 05, 07);

            var leaveRequest = new LeaveRequest()
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = false,
                ApprovedBy = 0,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };
            var UpdateleaveRequest = new LeaveRequest()
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1, 
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = true,
                ApprovedBy = 3,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };
            _mock.Setup(x => x.ApproveLeaveRequestAsync(It.IsAny<LeaveRequest>())).ReturnsAsync(true);

            var result = await _mock.Object.ApproveLeaveRequestAsync(UpdateleaveRequest);
            Assert.True(result);
            _mock.Verify(x => x.ApproveLeaveRequestAsync(It.Is<LeaveRequest>(r =>
                        r.Id == UpdateleaveRequest.Id &&
                        r.EmployeeId == UpdateleaveRequest.EmployeeId)), Times.Once);
        }

    }
}
