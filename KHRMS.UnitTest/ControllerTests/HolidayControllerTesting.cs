using KHRMS.Core;
using KHRMS.Services;
using Moq;


namespace KHRMS.UnitTest.ControllerTests
{
    public class HolidayControllerTesting
    {
        private readonly Mock<IHolidayService> _mock;
        private readonly HolidayController _controller;
        public HolidayControllerTesting()
        {
            _mock = new Mock<IHolidayService>();
            _controller = new HolidayController(_mock.Object);
        }

        [Fact]
        public void Get_AllHolidays_WhenCalled_ShouldReturnSuccess()
        {
            var holiday = new List<Holiday>
            {
                new Holiday {
                    Id = 1,
                    HolidayName = "",
                    Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss") 
                }
            };
            _mock.Setup(x => x.GetAllHolidays()).ReturnsAsync(holiday);
            var result = _controller.GetHolidays();
            Assert.NotNull(result);
            _mock.Verify(x => x.GetAllHolidays(), Times.Once());
        }


        [Fact]
        public void Add_Holiday_WhenValidDataProvided_ShouldReturnSuccess()
        {
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            _mock.Setup(x => x.CreateHoliday(It.IsAny<Holiday>())).ReturnsAsync(true);
            var result = _controller.AddHoliday(holiday);
            Assert.NotNull(result);
            _mock.Verify(x => x.CreateHoliday(It.IsAny<Holiday>()), Times.Once());
        }

        [Fact]
        public void Update_Holiday_WhenValidDataProvided_ShouldReturnSuccess()
        {
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            Holiday updateholiday = new Holiday()
            {
                Id = 1,
                HolidayName = "NewHoliday",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            _mock.Setup(x => x.UpdateHoliday(It.IsAny<Holiday>())).ReturnsAsync(true);
            var result = _controller.UpdateHoliday(updateholiday);
            Assert.NotNull(result);

            _mock.Verify(x => x.UpdateHoliday(It.Is<Holiday>(r =>
                r.Id == updateholiday.Id &&
                r.HolidayName == updateholiday.HolidayName)), Times.Once());
        }

        [Fact]
        public void Delete_Holiday_WhenHolidayExists_ShouldReturnSuccess()
        {
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            _mock.Setup(x => x.DeleteHoliday(holiday.Id)).ReturnsAsync(true);

            var result = _controller.DeleteHoliday(holiday.Id);
            Assert.NotNull(result);

            _mock.Verify(x => x.DeleteHoliday(holiday.Id), Times.Once);
        }

    }
}
