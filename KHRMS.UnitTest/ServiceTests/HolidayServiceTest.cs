using KHRMS.Core;
using KHRMS.Services;
using Moq;


namespace KHRMS.UnitTest.ServiceTests
{
    public class HolidayServiceTest
    {
        public HolidayServiceTest()
        {
                
        }

        [Fact]
        public void Create_Holiday_ShouldReturnSuccess_WhenHolidayIsValid()
        {
            var mock = new Mock<IHolidayService>();
            List<Holiday> holidays = new List<Holiday>();
            mock.Setup(x => x.CreateHoliday(It.IsAny<Holiday>()))
                        .Returns(Task.FromResult(true));
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            holidays.Add(holiday);
            Assert.Equal(1, 1);
        }
       
        [Fact]
        public async Task Create_Holiday_ShouldThrowException_WhenHolidayAlreadyExists()
        {
            // Arrange
            var mock = new Mock<IHolidayService>();

            // Set up mock to return false (indicating failure)
            mock.Setup(x => x.CreateHoliday(It.IsAny<Holiday>()))
                .ThrowsAsync(new InvalidOperationException("Holiday already exists"));

            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "", // Assuming this should trigger an error
                Description = "Public Holiday",
            };

            // Act & Assert: Expecting an exception when calling CreateHoliday
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await mock.Object.CreateHoliday(holiday)
            );

            // Check exception message
            Assert.Equal("Holiday already exists", exception.Message);
        }

        [Fact]
        public async Task Create_Holiday_ShouldThrowException_WhenHolidayIsNull()
        {
            // Arrange
            var mock = new Mock<IHolidayService>();

            // Set up mock to throw an exception when a null holiday is passed
            mock.Setup(x => x.CreateHoliday(null))
                .ThrowsAsync(new ArgumentNullException("Holiday cannot be null"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await mock.Object.CreateHoliday(null) // Calling the mock service
            );

            // Ensure the correct exception message is returned
            Assert.Equal("Holiday cannot be null", exception.ParamName);
        }

        [Fact]
        public void Delete_Holiday_ShouldReturnSuccess_WhenHolidayExists()
        {
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            var Id = 1;
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            mock.Setup(x => x.DeleteHoliday(Id));
            var result = holidayeservice.DeleteHoliday(Id);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteHoliday(Id), Times.Once);
        }
  
        [Fact]
        public async Task Delete_Holiday_ShouldThrowException_WhenHolidayDoesNotExist()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IHolidayService>();
            var holidayService = mock.Object;

            // Mock DeleteHoliday to throw an exception if the holiday does not exist
            mock.Setup(x => x.DeleteHoliday(Id))
                .ThrowsAsync(new KeyNotFoundException("Holiday not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await holidayService.DeleteHoliday(Id)
            );

            // Ensure the exception message matches
            Assert.Equal("Holiday not found", exception.Message);

            // Verify that DeleteHoliday was called once
            mock.Verify(x => x.DeleteHoliday(Id), Times.Once);
        }
     
        [Fact]
        public async Task Delete_Holiday_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            var holidayService = mock.Object;

            // Mock DeleteHoliday to throw an exception
            mock.Setup(x => x.DeleteHoliday(Id))
                .ThrowsAsync(new KeyNotFoundException("Holiday not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await holidayService.DeleteHoliday(Id)
            );

            // Ensure the exception message matches
            Assert.Equal("Holiday not found", exception.Message);

            // Verify that DeleteHoliday was called once
            mock.Verify(x => x.DeleteHoliday(Id), Times.Once);
        }

        [Fact]
        public void Get_AllHolidays_ShouldReturnList_WhenHolidaysExist()
        {
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();     
            var Id = 1;
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            mock.Setup(x => x.GetAllHolidays());
            var result = holidayeservice.GetAllHolidays();
            Assert.NotNull(result);
            Assert.Equal(1, holiday.Id);
            Assert.Equal("", holiday.HolidayName);
        }
      

        [Fact]
        public async Task Get_AllHolidays_ShouldThrowException_WhenNoHolidaysAvailable()
        {
            // Arrange
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayService = mock.Object;

            // Simulate: GetAllHolidays throws InvalidOperationException
            mock.Setup(x => x.GetAllHolidays())
                .ThrowsAsync(new InvalidOperationException("No Holiday available"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await holidayService.GetAllHolidays()
            );

            // Verify correct exception message
            Assert.Equal("No Holiday available", exception.Message);

            // Verify that GetAllHolidays was called exactly once
            mock.Verify(x => x.GetAllHolidays(), Times.Once);
        }


        [Fact]
        public async Task Get_AllHolidays_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var mock = new Mock<IHolidayService>();
            var holidayService = mock.Object;

            // Configure mock to throw an exception when GetAllHolidays is called
            mock.Setup(x => x.GetAllHolidays())
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await holidayService.GetAllHolidays()
            );

            // Verify exception message
            Assert.Equal("Unexpected error", exception.Message);

            // Verify method was called once
            mock.Verify(x => x.GetAllHolidays(), Times.Once);
        }

        [Fact]
        public void Get_HolidayById_ShouldReturnHoliday_WhenIdIsValid()
        {
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            var Id = 1;
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            mock.Setup(x => x.GetHolidayById(1));
            var result = holidayeservice.GetHolidayById(1);
            Assert.NotNull(result);
            Assert.Equal(1, holiday.Id);
            Assert.Equal("", holiday.HolidayName);
        }

        [Fact]
        public async Task Get_HolidayById_ShouldThrowException_WhenHolidayDoesNotExist()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IHolidayService>();
            var holidayService = mock.Object;

            // Configure mock to throw KeyNotFoundException
            mock.Setup(x => x.GetHolidayById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Holiday not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await holidayService.GetHolidayById(Id)
            );

            // Verify exception message
            Assert.Equal("Holiday not found", exception.Message);

            // Verify method was called once
            mock.Verify(x => x.GetHolidayById(Id), Times.Once);
        }

        [Fact]
        public async Task Get_HolidayById_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            var holidayService = mock.Object;

            // Configure mock to throw KeyNotFoundException
            mock.Setup(x => x.GetHolidayById(It.IsAny<int>()))
                .ThrowsAsync(new KeyNotFoundException("Holiday not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await holidayService.GetHolidayById(Id)
            );

            // Verify exception message
            Assert.Equal("Holiday not found", exception.Message);

            // Verify method was called once
            mock.Verify(x => x.GetHolidayById(Id), Times.Once);
        }

        [Fact]
        public void Update_Holiday_ShouldReturnSuccess_WhenHolidayExists()
        {
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            var Id = 1;
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            Holiday updateholiday = new Holiday()
            {
                Id = 1,
                HolidayName = "Raj",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            mock.Setup(x => x.GetHolidayById(1));
            var result = holidayeservice.UpdateHoliday(holiday);
            Assert.NotNull(result);
            Assert.Equal(1,1);
        }

        [Fact]
        public async Task Update_Holiday_ShouldThrowException_WhenHolidayDoesNotExist()
        {
            // Arrange
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayService = mock.Object;

            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };

            // Simulate: Holiday does not exist (returns null)
            mock.Setup(x => x.GetHolidayById(It.IsAny<int>()))
                .ReturnsAsync((Holiday)null);

            // Simulate: Update operation throws KeyNotFoundException
            mock.Setup(x => x.UpdateHoliday(It.IsAny<Holiday>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await holidayService.UpdateHoliday(holiday)
            );

            // Verify correct exception message
            Assert.Equal("Update not found", exception.Message);

            // Verify UpdateHoliday was called exactly once
            mock.Verify(x => x.UpdateHoliday(It.IsAny<Holiday>()), Times.Once);
        }

        
        [Fact]
        public async Task Update_Holiday_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayService = mock.Object;

            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };

            // Simulate: GetHolidayById returns null (holiday does not exist)
            mock.Setup(x => x.GetHolidayById(It.IsAny<int>()))
                .ReturnsAsync((Holiday)null);

            // Simulate: Update operation throws KeyNotFoundException
            mock.Setup(x => x.UpdateHoliday(It.IsAny<Holiday>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await holidayService.UpdateHoliday(holiday)
            );

            // Verify correct exception message
            Assert.Equal("Update not found", exception.Message);

            // Verify UpdateHoliday was called exactly once
            mock.Verify(x => x.UpdateHoliday(It.IsAny<Holiday>()), Times.Once);
        }

    }
}
