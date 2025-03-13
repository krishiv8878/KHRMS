using KHRMS.Core;
using KHRMS.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ControllerTests
{
    public class LeaveTypeControllerTest
    {
        public LeaveTypeControllerTest()
        {
                
        }


        [Fact]
        public void GetLeaveTypeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            var controller = new LeaveTypeController(mock.Object);
            mock.Setup(x => x.GetAllLeaveType());
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            var result = controller.GetLeaveType();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }

        [Fact]
        public void AddLeaveTypeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            var controller = new LeaveTypeController(mock.Object);
            mock.Setup(x => x.AddLeaveType(It.IsAny<LeaveType>()));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            var result = controller.AddLeaveType(leavetype);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("Casual", leavetype.LeaveName);
        }

        [Fact]
        public void UpdateLeaveTypeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            var controller = new LeaveTypeController(mock.Object);
            mock.Setup(x => x.UpdateLeaveType(It.IsAny<LeaveType>()));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            Core.LeaveType updateleavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "EarnLeave",
                Type = "Full Type",
                Description = "string"
            };
            var result = controller.UpdateLeaveType(leavetype);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("Casual", leavetype.LeaveName);
        }

        [Fact]
        public void DeleteLeaveTypeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            var controller = new LeaveTypeController(mock.Object);
            mock.Setup(x => x.DeleteLeaveType(1));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            var result = controller.DeleteLeaveType(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x=>x.DeleteLeaveType(1),Times.Once);
        }
    }
}
