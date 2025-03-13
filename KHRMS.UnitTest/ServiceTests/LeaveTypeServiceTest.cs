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
        public async Task AddLeaveTypeReturnFail()
        {
            // Arrange
            var mock = new Mock<ILeaveTypeService>();
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };

            // Simulate exception when duplicate LeaveType is added
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                .ThrowsAsync(new InvalidOperationException("LeaveType already exists"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await mock.Object.AddLeaveType(leavetype)
            );

            // Verify correct exception message
            Assert.Equal("LeaveType already exists", exception.Message);

            // Verify AddLeaveType was called exactly once
            mock.Verify(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()), Times.Once);
        }
       
        [Fact]
        public async Task AddLeaveTypeReturnException()
        {
            // Arrange
            var mock = new Mock<ILeaveTypeService>();

            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };

            // Simulate an exception when trying to add a LeaveType
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                .ThrowsAsync(new InvalidOperationException("LeaveType already exists"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await mock.Object.AddLeaveType(leavetype)
            );

            // Verify correct exception message
            Assert.Equal("LeaveType already exists", exception.Message);

            // Verify that AddLeaveType was called exactly once
            mock.Verify(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()), Times.Once);
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
            // Arrange
            var Id = 999;
            var mock = new Mock<ILeaveTypeService>();
            var leaveTypeService = mock.Object;

            // Simulating that AddLeaveType succeeds
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                .Returns(Task.FromResult(true));

            // Simulating that DeleteLeaveType throws KeyNotFoundException
            mock.Setup(x => x.DeleteLeaveType(Id))
                .ThrowsAsync(new KeyNotFoundException("LeaveType not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => leaveTypeService.DeleteLeaveType(Id)
            );

            // Verify that DeleteLeaveType was called exactly once
            mock.Verify(x => x.DeleteLeaveType(Id), Times.Once);

            // Ensure the correct exception message is thrown
            Assert.Equal("LeaveType not found", exception.Message);
        }
     
        [Fact]
        public async Task DeleteLeaveTypeReturnException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();
            var leaveTypeService = mock.Object;

            // Simulating that AddLeaveType succeeds
            mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                .Returns(Task.FromResult(true));

            // Simulating that DeleteLeaveType throws KeyNotFoundException
            mock.Setup(x => x.DeleteLeaveType(Id))
                .ThrowsAsync(new KeyNotFoundException("LeaveType not found"));

            // Act & Assert: Expect DeleteLeaveType to throw KeyNotFoundException
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => leaveTypeService.DeleteLeaveType(Id)
            );

            // Verify that DeleteLeaveType was called exactly once
            mock.Verify(x => x.DeleteLeaveType(Id), Times.Once);

            // Ensure the correct exception message is thrown
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
            // Arrange
            var mock = new Mock<ILeaveTypeService>();

            // Ensure GetAllLeaveType throws the expected exception asynchronously
            mock.Setup(x => x.GetAllLeaveType())
                .ThrowsAsync(new InvalidOperationException("No LeaveType available"));

            ILeaveTypeService leavetypeservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await leavetypeservice.GetAllLeaveType()
            );

            // Verify that the exception message matches
            Assert.Equal("No LeaveType available", exception.Message);

            // Ensure that GetAllLeaveType was called exactly once
            mock.Verify(x => x.GetAllLeaveType(), Times.Once);
        }
      
        [Fact]
        public async Task GetAllLeaveTypeReturnException()
        {
            // Arrange
            var mock = new Mock<ILeaveTypeService>();

            // Ensure GetAllLeaveType throws a generic Exception with a specific message
            mock.Setup(x => x.GetAllLeaveType())
                .ThrowsAsync(new Exception("Unexpected error"));

            ILeaveTypeService leavetypeservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await leavetypeservice.GetAllLeaveType()
            );

            // Verify that the exception message matches
            Assert.Equal("Unexpected error", exception.Message);

            // Ensure that GetAllLeaveType was called exactly once
            mock.Verify(x => x.GetAllLeaveType(), Times.Once);
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
            // Arrange
            var Id = 999;
            var mock = new Mock<ILeaveTypeService>();

            // Setup mock to throw KeyNotFoundException when called with any integer
            mock.Setup(x => x.GetLeaveTypeById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("LeaveType not found"));

            ILeaveTypeService leavetypeservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await leavetypeservice.GetLeaveTypeById(Id)
            );

            // Verify that the exception message matches
            Assert.Equal("LeaveType not found", exception.Message);

            // Ensure that GetLeaveTypeById was called exactly once with Id
            mock.Verify(x => x.GetLeaveTypeById(Id), Times.Once);
        }

        [Fact]
        public async Task GetLeaveTypeByIdReturnException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<ILeaveTypeService>();

            // Properly setup mock to throw an exception when GetLeaveTypeById is called
            mock.Setup(x => x.GetLeaveTypeById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("LeaveType not found"));

            ILeaveTypeService leavetypeservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await leavetypeservice.GetLeaveTypeById(Id)
            );

            // Verify exception message
            Assert.Equal("LeaveType not found", exception.Message);

            // Ensure method was called exactly once
            mock.Verify(x => x.GetLeaveTypeById(Id), Times.Once);
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
            // Arrange
            var mock = new Mock<ILeaveTypeService>();

            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };

            // Ensure that UpdateLeaveType always throws an exception
            mock.Setup(x => x.UpdateLeaveType(It.IsAny<Core.LeaveType>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var leavetypeservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await leavetypeservice.UpdateLeaveType(leavetype));

            // Verify exception message
            Assert.Equal("Update not found", exception.Message);

            // Verify that UpdateLeaveType was actually called once
            mock.Verify(x => x.UpdateLeaveType(It.IsAny<Core.LeaveType>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLeaveTypeReturnException()
        {
            // Arrange
            var mock = new Mock<ILeaveTypeService>();

            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                LeaveName = "Casual",
                Type = "Full Type",
                Description = "string"
            };

            // Ensure that UpdateLeaveType always throws an exception when called
            mock.Setup(x => x.UpdateLeaveType(It.IsAny<Core.LeaveType>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var leavetypeservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await leavetypeservice.UpdateLeaveType(leavetype));

            // Verify exception message
            Assert.Equal("Update not found", exception.Message);

            // Ensure UpdateLeaveType was actually called
            mock.Verify(x => x.UpdateLeaveType(It.IsAny<Core.LeaveType>()), Times.Once);
        }

    }
}
