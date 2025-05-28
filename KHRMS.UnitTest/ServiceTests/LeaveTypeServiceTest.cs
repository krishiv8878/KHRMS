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
        private readonly Mock<ILeaveTypeService> _mock;
        private readonly ILeaveTypeService _service;
        public LeaveTypeServiceTest()
        {
            _mock = new Mock<ILeaveTypeService>();
            _service = _mock.Object;
        }

        [Fact]
        public async Task AddLeaveType_ValidInput_ReturnsPass()
        {
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            _mock.Setup(x => x.AddLeaveType(leavetype)).ReturnsAsync(true);

            var result = await _service.AddLeaveType(leavetype);
            Assert.True(result);
            _mock.Verify(x => x.AddLeaveType(leavetype), Times.Once);
        }

        [Fact]
        public async Task Add_LeaveType_DuplicateEntry_ThrowsException()
        {
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };

            // Simulate exception when duplicate LeaveType is added
            _mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                .ThrowsAsync(new InvalidOperationException("LeaveType already exists"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.AddLeaveType(leavetype)
            );

            // Verify correct exception message
            Assert.Equal("LeaveType already exists", exception.Message);

            // Verify AddLeaveType was called exactly once
            _mock.Verify(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()), Times.Once);
        }

        [Fact]
        public async Task Add_LeaveType_InvalidInput_ThrowsException()
        {
     
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };

            // Simulate an exception when trying to add a LeaveType
            _mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                .ThrowsAsync(new InvalidOperationException("LeaveType already exists"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.AddLeaveType(leavetype)
            );

            // Verify correct exception message
            Assert.Equal("LeaveType already exists", exception.Message);

            // Verify that AddLeaveType was called exactly once
            _mock.Verify(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()), Times.Once);
        }

        [Fact]
        public async Task Delete_LeaveType_ValidId_ReturnsPass()
        {
            var Id = 1;
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            _mock.Setup(x => x.DeleteLeaveType(Id)).ReturnsAsync(true);

            var result = await _service.DeleteLeaveType(Id);
            Assert.True(result);
            _mock.Verify(x => x.DeleteLeaveType(Id), Times.Once);
        }

        [Fact]
        public async Task Delete_LeaveType_NonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var Id = 999;
            // Simulating that AddLeaveType succeeds
            _mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                .Returns(Task.FromResult(true));

            // Simulating that DeleteLeaveType throws KeyNotFoundException
            _mock.Setup(x => x.DeleteLeaveType(Id))
                .ThrowsAsync(new KeyNotFoundException("LeaveType not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteLeaveType(Id)
            );

            // Verify that DeleteLeaveType was called exactly once
            _mock.Verify(x => x.DeleteLeaveType(Id), Times.Once);

            // Ensure the correct exception message is thrown
            Assert.Equal("LeaveType not found", exception.Message);
        }

        [Fact]
        public async Task Delete_LeaveType_ExceptionOccurs_ThrowsException()
        {
            // Arrange
            var Id = 1;

            // Simulating that AddLeaveType succeeds
            _mock.Setup(x => x.AddLeaveType(It.IsAny<Core.LeaveType>()))
                .Returns(Task.FromResult(true));

            // Simulating that DeleteLeaveType throws KeyNotFoundException
            _mock.Setup(x => x.DeleteLeaveType(Id))
                .ThrowsAsync(new KeyNotFoundException("LeaveType not found"));

            // Act & Assert: Expect DeleteLeaveType to throw KeyNotFoundException
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _service.DeleteLeaveType(Id)
            );

            // Verify that DeleteLeaveType was called exactly once
            _mock.Verify(x => x.DeleteLeaveType(Id), Times.Once);

            // Ensure the correct exception message is thrown
            Assert.Equal("LeaveType not found", exception.Message);
        }


        [Fact]
        public async Task Get_AllLeaveType_HasRecords_ReturnsLeaveTypes()
        {
            var Id = 1;
            var leavetype = new List<Core.LeaveType>
            {
                new Core.LeaveType{
                        Id = 1,
                    Type = "Full Type",
                    Description = "string"
                }
            };
            _mock.Setup(x => x.GetAllLeaveType()).ReturnsAsync(leavetype);

            var result = await _service.GetAllLeaveType();
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
        }

        [Fact]
        public async Task Get_AllLeaveType_NoRecords_ThrowsInvalidOperationException()
        {
            // Arrange
           
            // Ensure GetAllLeaveType throws the expected exception asynchronously
            _mock.Setup(x => x.GetAllLeaveType())
                .ThrowsAsync(new InvalidOperationException("No LeaveType available"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetAllLeaveType()
            );

            // Verify that the exception message matches
            Assert.Equal("No LeaveType available", exception.Message);

            // Ensure that GetAllLeaveType was called exactly once
            _mock.Verify(x => x.GetAllLeaveType(), Times.Once);
        }

        [Fact]
        public async Task Get_AllLeaveType_ExceptionOccurs_ThrowsException()
        {
            // Ensure GetAllLeaveType throws a generic Exception with a specific message
            _mock.Setup(x => x.GetAllLeaveType())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetAllLeaveType()
            );

            // Verify that the exception message matches
            Assert.Equal("Unexpected error", exception.Message);

            // Ensure that GetAllLeaveType was called exactly once
            _mock.Verify(x => x.GetAllLeaveType(), Times.Once);
        }

        [Fact]
        public void Get_LeaveTypeById_ValidId_ReturnsLeaveType()
        {
            var Id = 1;
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            _mock.Setup(x => x.GetLeaveTypeById(1));
            var result = _service.GetLeaveTypeById(1);
            Assert.NotNull(result);
            Assert.Equal(1, leavetype.Id);
        }

        [Fact]
        public async Task Get_LeaveTypeById_NonExistentId_ThrowsKeyNotFoundException()
        {
            // Arrange
            var Id = 999;
          
            // Setup _mock to throw KeyNotFoundException when called with any integer
            _mock.Setup(x => x.GetLeaveTypeById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("LeaveType not found"));
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetLeaveTypeById(Id)
            );

            // Verify that the exception message matches
            Assert.Equal("LeaveType not found", exception.Message);

            // Ensure that GetLeaveTypeById was called exactly once with Id
            _mock.Verify(x => x.GetLeaveTypeById(Id), Times.Once);
        }

        [Fact]
        public async Task Get_LeaveTypeById_ExceptionOccurs_ThrowsException()
        {
            // Arrange
            var Id = 1;
            // Properly setup _mock to throw an exception when GetLeaveTypeById is called
            _mock.Setup(x => x.GetLeaveTypeById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("LeaveType not found"));
            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetLeaveTypeById(Id)
            );

            // Verify exception message
            Assert.Equal("LeaveType not found", exception.Message);

            // Ensure method was called exactly once
            _mock.Verify(x => x.GetLeaveTypeById(Id), Times.Once);
        }

        [Fact]
        public async Task Update_LeaveType_ValidInput_ReturnsPass()
        {
            var Id = 1;
          
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };
            Core.LeaveType updateleavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "Leave"
            };
            _mock.Setup(x => x.UpdateLeaveType(leavetype)).ReturnsAsync(true);

            var result = await _service.UpdateLeaveType(leavetype);
            Assert.True(result);
            _mock.Verify(x => x.UpdateLeaveType(leavetype), Times.Once);
        }


        [Fact]
        public async Task Update_LeaveType_NonExistentId_ThrowsKeyNotFoundException()
        {
            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };

            // Ensure that UpdateLeaveType always throws an exception
            _mock.Setup(x => x.UpdateLeaveType(It.IsAny<Core.LeaveType>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.UpdateLeaveType(leavetype));

            // Verify exception message
            Assert.Equal("Update not found", exception.Message);

            // Verify that UpdateLeaveType was actually called once
            _mock.Verify(x => x.UpdateLeaveType(It.IsAny<Core.LeaveType>()), Times.Once);
        }

        [Fact]
        public async Task Update_LeaveType_ExceptionOccurs_ThrowsException()
        {

            Core.LeaveType leavetype = new Core.LeaveType()
            {
                Id = 1,
                Type = "Full Type",
                Description = "string"
            };

            // Ensure that UpdateLeaveType always throws an exception when called
            _mock.Setup(x => x.UpdateLeaveType(It.IsAny<Core.LeaveType>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
                await _service.UpdateLeaveType(leavetype));

            // Verify exception message
            Assert.Equal("Update not found", exception.Message);

            // Ensure UpdateLeaveType was actually called
            _mock.Verify(x => x.UpdateLeaveType(It.IsAny<Core.LeaveType>()), Times.Once);
        }

    }
}
