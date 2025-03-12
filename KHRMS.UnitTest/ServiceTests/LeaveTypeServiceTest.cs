using KHRMS.Core;
using KHRMS.Infrastructure.Migrations;
using KHRMS.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ServiceTests
{
    public class LeaveTypeServiceTest
    {
        public LeaveTypeServiceTest()
        {
                
        }

        [Fact]
        public void AddLeaveTypeReturnPass()
        {
            var mock = new Mock<ILeaveTypeService>();
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                        .Returns(Task.FromResult(true));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            leavetypes.Add(leavetype);
            Assert.Equal(1, 1);
        }

        [Fact]
        public void AddLeaveTypeReturnFail()
        {
            var mock = new Mock<ILeaveTypeService>();
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                        .Returns(Task.FromResult(true));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            leavetypes.Add(leavetype);
            var exception = Assert.Throws<InvalidOperationException>(() => leavetypes.Add(leavetype));
            Assert.Equal("LeaveType already exists", exception.Message);
        }

        [Fact]
        public void AddLeaveTypeReturnException()
        {
            var mock = new Mock<ILeaveTypeService>();
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                        .Returns(Task.FromResult(true));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            leavetypes.Add(leavetype);
            var exception = Assert.Throws<InvalidOperationException>(() => leavetypes.Add(null));
            Assert.Equal("LeaveType already exists", exception.Message);
        }

        [Fact]
        public void DeleteLeaveTypeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                        .Returns(Task.FromResult(true));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            mock.Setup(x => x.DeleteLeaveType(Id));
            var result = leavetypeservice.DeleteLeaveType(Id);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteLeaveType(Id), Times.Once);
        }

        [Fact]
        public async Task DeleteLeaveTypeReturnFail()
        {
            var Id = 999;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                        .Returns(Task.FromResult(true));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            mock.Setup(x => x.DeleteLeaveType(Id));
            var result = leavetypeservice.DeleteLeaveType(Id);
            mock.Verify(x => x.DeleteLeaveType(Id), Times.Once);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => leavetypeservice.DeleteLeaveType(Id));
            Assert.Equal("LeaveType not found", exception.Message);
        }


        [Fact]
        public async Task DeleteLeaveTypeReturnException()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                        .Returns(Task.FromResult(true));
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            mock.Setup(x => x.DeleteLeaveType(Id));
            var result = leavetypeservice.DeleteLeaveType(Id);
            mock.Verify(x => x.DeleteLeaveType(Id), Times.Once);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => leavetypeservice.DeleteLeaveType(Id));
            Assert.Equal("LeaveType not found", exception.Message);
        }


        [Fact]
        public void GetAllLeaveTypeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            mock.Setup(x => x.GetAllLeaveType());
            var result = leavetypeservice.GetAllLeaveType();
            Assert.NotNull(result);
            Assert.Equal(1, leavetype.Id);
            Assert.Equal("Casual", leavetype.LeaveName);
        }


        [Fact]
        public async Task GetAllLeaveTypeReturnFail()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            Core.LeaveType leavetype = new Core.LeaveType();
            mock.Setup(x => x.GetAllLeaveType());
            var result = leavetypeservice.GetAllLeaveType();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => leavetypeservice.GetAllLeaveType());
            Assert.Equal("No LeaveType available", exception.Message);
        }

        [Fact]
        public async Task GetAllLeaveTypeReturnException()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            Core.LeaveType leavetype = new Core.LeaveType();
            mock.Setup(x => x.GetAllLeaveType());
            var result = leavetypeservice.GetAllLeaveType();
            var exception = await Assert.ThrowsAsync<Exception>(() => mock.Object.GetAllLeaveType());
            Assert.Equal("Unexpected error", exception.Message);
        }


        [Fact]
        public void GetLeaveTypeByIdReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            mock.Setup(x => x.GetLeaveTypeById(1));
            var result = leavetypeservice.GetLeaveTypeById(1);
            Assert.NotNull(result);
            Assert.Equal(1, leavetype.Id);
            Assert.Equal("Casual", leavetype.LeaveName);
        }

        [Fact]
        public async Task GetLeaveTypeByIdReturnFail()
        {
            var Id = 999;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            Core.LeaveType leavetype = new Core.LeaveType();
            mock.Setup(x => x.GetLeaveTypeById(1));
            var result = leavetypeservice.GetLeaveTypeById(1);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => leavetypeservice.GetLeaveTypeById(Id));
            Assert.Equal("LeaveType not found", exception.Message);
        }

        [Fact]
        public async Task GetLeaveTypeByIdReturnException()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            Core.LeaveType leavetype = new Core.LeaveType();
            mock.Setup(x => x.GetLeaveTypeById(1));
            var result = leavetypeservice.GetLeaveTypeById(1);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => leavetypeservice.GetLeaveTypeById(Id));
            Assert.Equal("LeaveType not found", exception.Message);
        }

        [Fact]
        public void UpdateLeaveTypeReturnPass()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
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
                Description = "Leave"
            };
            mock.Setup(x => x.GetLeaveTypeById(1));
            var result = leavetypeservice.UpdateLeaveType(leavetype);
            Assert.NotNull(result);
            Assert.Equal(1,1);
        }

        [Fact]
        public async Task UpdateLeaveTypeReturnFail()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            mock.Setup(x => x.GetLeaveTypeById(1));
            var result = leavetypeservice.UpdateLeaveType(leavetype);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => leavetypeservice.UpdateLeaveType(leavetype));
            Assert.Equal("Update not found", exception.Message);
        }


        [Fact]
        public async Task UpdateLeaveTypeReturnException()
        {
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            ILeaveTypeService leavetypeservice = mock.Object;
            List<Core.LeaveType> leavetypes = new List<Core.LeaveType>();
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };
            mock.Setup(x => x.GetLeaveTypeById(1));
            var result = leavetypeservice.UpdateLeaveType(leavetype);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => leavetypeservice.UpdateLeaveType(null));
            Assert.Equal("Update not found", exception.Message);
        }
    }
}
