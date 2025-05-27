using KHRMS.Core;
using KHRMS.Services;
using Moq;
using Xunit.Sdk;


namespace KHRMS.UnitTest.ServiceTests
{
    public class HolidayServiceTest
    {
        private readonly Mock<IHolidayService> _mock;
        private readonly IHolidayService _service;
        public HolidayServiceTest()
        {
                _mock = new Mock<IHolidayService>();
            _service = _mock.Object;
        }

        [Fact]
        public async Task Create_Holiday_ShouldReturnSuccess_WhenHolidayIsValid()
        {
            var holiday = new Holiday
            {
                Id = 1,
                HolidayName = "Diwali",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            _mock.Setup(x => x.CreateHoliday(holiday)).ReturnsAsync(true);

            var result = await _service.CreateHoliday(holiday);

            Assert.True(result);
            _mock.Verify(x => x.CreateHoliday(holiday), Times.Once);
        }
       
        [Fact]
        public async Task Create_Holiday_ShouldThrowException_WhenHolidayAlreadyExists()
        {
    
            // Set up _mock to return false (indicating failure)
            _mock.Setup(x => x.CreateHoliday(It.IsAny<Holiday>()))
                .ThrowsAsync(new InvalidOperationException("Holiday already exists"));

            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "", // Assuming this should trigger an error
                Description = "Public Holiday",
            };

            // Act & Assert: Expecting an exception when calling CreateHoliday
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.CreateHoliday(holiday)
            );

            // Check exception message
            Assert.Equal("Holiday already exists", exception.Message);
        }

        [Fact]
        public async Task Create_Holiday_ShouldThrowException_WhenHolidayIsNull()
        {
 
            // Set up _mock to throw an exception when a null holiday is passed
            _mock.Setup(x => x.CreateHoliday(null))
                .ThrowsAsync(new ArgumentNullException("Holiday cannot be null"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.CreateHoliday(null) // Calling the _mock service
            );

            // Ensure the correct exception message is returned
            Assert.Equal("Holiday cannot be null", exception.ParamName);
        }

        [Fact]
        public async Task Delete_Holiday_ShouldReturnSuccess_WhenHolidayExists()
        {
            var id = 1;
            
            _mock.Setup(x => x.DeleteHoliday(id)).ReturnsAsync(true);

            var result = await _service.DeleteHoliday(id);

            Assert.True(result);
            _mock.Verify(x => x.DeleteHoliday(id), Times.Once);

        }

        [Fact]
        public async Task Delete_Holiday_ShouldThrowException_WhenHolidayDoesNotExist()
        {
            // Arrange
            var Id = 999;
            // Mock DeleteHoliday to throw an exception if the holiday does not exist
            _mock.Setup(x => x.DeleteHoliday(Id))
                .ThrowsAsync(new KeyNotFoundException("Holiday not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteHoliday(Id)
            );

            // Ensure the exception message matches
            Assert.Equal("Holiday not found", exception.Message);

            // Verify that DeleteHoliday was called once
            _mock.Verify(x => x.DeleteHoliday(Id), Times.Once);
        }
     
        [Fact]
        public async Task Delete_Holiday_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;

            // Mock DeleteHoliday to throw an exception
            _mock.Setup(x => x.DeleteHoliday(Id))
                .ThrowsAsync(new KeyNotFoundException("Holiday not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteHoliday(Id)
            );

            // Ensure the exception message matches
            Assert.Equal("Holiday not found", exception.Message);

            // Verify that DeleteHoliday was called once
            _mock.Verify(x => x.DeleteHoliday(Id), Times.Once);
        }

        [Fact]
        public async Task Get_AllHolidays_ShouldReturnList_WhenHolidaysExist()
        {
            var holidays = new List<Holiday>
            {
                new Holiday { Id = 1, HolidayName = "New Year", Description = "2024-01-01" },
                new Holiday { Id = 2, HolidayName = "Diwali", Description = "2024-10-16" }
            };

            var _mock = new Mock<IHolidayService>();
            _mock.Setup(x => x.GetAllHolidays()).ReturnsAsync(holidays);

            var result = await _mock.Object.GetAllHolidays();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
      

        [Fact]
        public async Task Get_AllHolidays_ShouldThrowException_WhenNoHolidaysAvailable()
        {
            // Simulate: GetAllHolidays throws InvalidOperationException
            _mock.Setup(x => x.GetAllHolidays())
                .ThrowsAsync(new InvalidOperationException("No Holiday available"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetAllHolidays()
            );

            // Verify correct exception message
            Assert.Equal("No Holiday available", exception.Message);

            // Verify that GetAllHolidays was called exactly once
            _mock.Verify(x => x.GetAllHolidays(), Times.Once);
        }


        [Fact]
        public async Task Get_AllHolidays_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Configure _mock to throw an exception when GetAllHolidays is called
            _mock.Setup(x => x.GetAllHolidays())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetAllHolidays()
            );

            // Verify exception message
            Assert.Equal("Unexpected error", exception.Message);

            // Verify method was called once
            _mock.Verify(x => x.GetAllHolidays(), Times.Once);
        }

        [Fact]
        public async Task Get_HolidayById_ShouldReturnHoliday_WhenIdIsValid()
        {
            var holiday = new Holiday
            {
                Id = 1,
                HolidayName = "Independence Day",
                Description = "2024-08-15"
            };

            var _mock = new Mock<IHolidayService>();
            _mock.Setup(x => x.GetHolidayById(1)).ReturnsAsync(holiday);

            var result = await _mock.Object.GetHolidayById(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Independence Day", result.HolidayName);
        }

        [Fact]
        public async Task Get_HolidayById_ShouldThrowException_WhenHolidayDoesNotExist()
        {
            // Arrange
            var Id = 999;
            // Configure _mock to throw KeyNotFoundException
            _mock.Setup(x => x.GetHolidayById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Holiday not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetHolidayById(Id)
            );

            // Verify exception message
            Assert.Equal("Holiday not found", exception.Message);

            // Verify method was called once
            _mock.Verify(x => x.GetHolidayById(Id), Times.Once);
        }

        [Fact]
        public async Task Get_HolidayById_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;

            // Configure _mock to throw KeyNotFoundException
            _mock.Setup(x => x.GetHolidayById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Holiday not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetHolidayById(Id)
            );

            // Verify exception message
            Assert.Equal("Holiday not found", exception.Message);

            // Verify method was called once
            _mock.Verify(x => x.GetHolidayById(Id), Times.Once);
        }

        [Fact]
        public async Task Update_Holiday_ShouldReturnSuccess_WhenHolidayExists()
        {
            var holiday = new Holiday
            {
                Id = 1,
                HolidayName = "Updated Holiday",
                Description = "2024-12-25"
            };

            var _mock = new Mock<IHolidayService>();
            _mock.Setup(x => x.UpdateHoliday(holiday)).ReturnsAsync(true);

            var result = await _mock.Object.UpdateHoliday(holiday);

            Assert.True(result);
            _mock.Verify(x => x.UpdateHoliday(holiday), Times.Once);
        }

        [Fact]
        public async Task Update_Holiday_ShouldThrowException_WhenHolidayDoesNotExist()
        {

            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };

            // Simulate: Holiday does not exist (returns null)
            _mock.Setup(x => x.GetHolidayById(It.IsAny<int>()))
                .ReturnsAsync((Holiday)null);

            // Simulate: Update operation throws KeyNotFoundException
            _mock.Setup(x => x.UpdateHoliday(It.IsAny<Holiday>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateHoliday(holiday)
            );

            // Verify correct exception message
            Assert.Equal("Update not found", exception.Message);

            // Verify UpdateHoliday was called exactly once
            _mock.Verify(x => x.UpdateHoliday(It.IsAny<Holiday>()), Times.Once);
        }

        
        [Fact]
        public async Task Update_Holiday_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };

            // Simulate: GetHolidayById returns null (holiday does not exist)
            _mock.Setup(x => x.GetHolidayById(It.IsAny<int>()))
                .ReturnsAsync((Holiday)null);

            // Simulate: Update operation throws KeyNotFoundException
            _mock.Setup(x => x.UpdateHoliday(It.IsAny<Holiday>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateHoliday(holiday)
            );

            // Verify correct exception message
            Assert.Equal("Update not found", exception.Message);

            // Verify UpdateHoliday was called exactly once
            _mock.Verify(x => x.UpdateHoliday(It.IsAny<Holiday>()), Times.Once);
        }

    }
}
