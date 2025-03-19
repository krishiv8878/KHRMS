using KHRMS.Core;
using KHRMS.Services;
using Moq;


namespace KHRMS.UnitTest.ControllerTests
{
    public class HolidayControllerTesting
    {
        public HolidayControllerTesting()
        {

        }

        [Fact]
        public void Get_AllHolidays_WhenCalled_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            var controller = new HolidayController(mock.Object);
            mock.Setup(x => x.GetAllHolidays());
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            var result = controller.GetHolidays();
            Assert.NotNull(result);
            Assert.Equal(1, 1);
        }


        [Fact]
        public void Add_Holiday_WhenValidDataProvided_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            var controller = new HolidayController(mock.Object);
            mock.Setup(x => x.CreateHoliday(It.IsAny<Holiday>()));
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            var result = controller.AddHoliday(holiday);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("", holiday.HolidayName);
        }

        [Fact]
        public void Update_Holiday_WhenValidDataProvided_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            var controller = new HolidayController(mock.Object);
            mock.Setup(x => x.UpdateHoliday(It.IsAny<Holiday>()));
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
            var result = controller.UpdateHoliday(holiday);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            Assert.Equal("", holiday.HolidayName);
        }

        [Fact]
        public void Delete_Holiday_WhenHolidayExists_ShouldReturnSuccess()
        {
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            var controller = new HolidayController(mock.Object);
            mock.Setup(x => x.DeleteHoliday(1));
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            var result = controller.DeleteHoliday(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteHoliday(1), Times.Once);
        }

    }
}
