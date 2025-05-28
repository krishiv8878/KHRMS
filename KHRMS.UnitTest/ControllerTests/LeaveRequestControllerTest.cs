using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Infrastructure.Migrations;
using KHRMS.Services;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NPOI.SS.Formula.Functions;

namespace KHRMS.UnitTest.ControllerTests
{
    public class LeaveRequestControllerTest
    {
        private readonly Mock<ILeaveRequestTypeService> _mock;
        private readonly LeaveRequestController _controller;
        public LeaveRequestControllerTest()
        {
            _mock = new Mock<ILeaveRequestTypeService>();
            _controller = new LeaveRequestController(_mock.Object);
        }

        [Fact]
        public async Task Add_Leaverequest_WhenValidRequestProvided_ShouldAddSuccessfully()
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

            var result = await _controller.Create(leaveRequest);

            Assert.NotNull(result);
            _mock.Verify(x => x.AddLeaveRequestType(It.IsAny<LeaveRequest>()), Times.Once);
        }

        [Fact]
        public async Task Get_AllLeaveRequest_WhenCalled_ShouldReturnListOfLeaveRequest()
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

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<ApiResponse<List<LeaveReqestModel>>>(okResult.Value);

            Assert.NotNull(result);
            Assert.Equal(leaveRequest.Count, returnValue.Data.Count());
            Assert.Contains(returnValue.Data, r => r.Id == 1);
            Assert.Contains(returnValue.Data, r => r.Id == 2);
            _mock.Verify(x => x.GetAllLeaveRequestType(), Times.Once);
        }

        [Fact]
        public async Task Delete_LeaveRequest_WhenLeaveRequestExists_ShouldDeleteSuccessfully()
        {
            var leaveRequestid = 1;
            var leaveRequest = new LeaveRequest { Id = leaveRequestid };
            _mock.Setup(x => x.GetLeaveRequestTypeById(leaveRequestid)).ReturnsAsync(leaveRequest);
            _mock.Setup(x => x.DeleteLeaveRequestType(leaveRequestid)).ReturnsAsync(true);
            var result = await _controller.Delete(leaveRequestid);
            Assert.NotNull(result);
            _mock.Verify(x => x.DeleteLeaveRequestType(leaveRequestid), Times.Once);
        }

        [Fact]
        public async Task Update_LeaveRequest_WhenValidUpdateProvided_ShouldUpdateSuccessfully()
        {
            var startdate = new DateTime(2025, 05, 10);
            var enddate = new DateTime(2025, 05, 15);
            var approveddate = new DateTime(2025, 05, 07);

            var updateleaveRequest = new LeaveRequest()
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = true,
                ApprovedBy = 0,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };
            _mock.Setup(x => x.UpdateLeaveRequestType(It.IsAny<LeaveRequest>())).ReturnsAsync(true);

            var result = await _controller.Update(updateleaveRequest.Id, updateleaveRequest);

            Assert.NotNull(result);
            _mock.Verify(x => x.UpdateLeaveRequestType(It.Is<LeaveRequest>(r =>
                r.Id == updateleaveRequest.Id &&
                r.IsApproved == updateleaveRequest.IsApproved)), Times.Once);
        }

        [Fact]
        public async Task Approve_LeaveRequest_WhenValidUpdateProvided_ShouldApproveSuccessfully()
        {
            var startdate = new DateTime(2025, 05, 10);
            var enddate = new DateTime(2025, 05, 15);
            var approveddate = new DateTime(2025, 05, 07);

            var ApproveleaveRequest = new LeaveRequest()
            {
                Id = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                LeaveMode = "",
                StartDate = startdate,
                EndDate = enddate,
                IsApproved = true,
                ApprovedBy = 0,
                ApprovedDate = approveddate,
                LeaveReason = "Nothing",
            };
            _mock.Setup(x => x.ApproveLeaveRequestAsync(It.IsAny<LeaveRequest>())).ReturnsAsync(true);

            var result = await _controller.ApproveLeaveRequest(ApproveleaveRequest);

            Assert.NotNull(result);
            _mock.Verify(x => x.ApproveLeaveRequestAsync(It.Is<LeaveRequest>(r =>
                r.Id == ApproveleaveRequest.Id &&
                r.IsApproved == ApproveleaveRequest.IsApproved)), Times.Once);
        }

    }
}
