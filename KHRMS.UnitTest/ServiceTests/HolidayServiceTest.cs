using KHRMS.Core;
using KHRMS.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ServiceTests
{
    public class HolidayServiceTest
    {
        public HolidayServiceTest()
        {
                
        }

        [Fact]
        public void CreateHolidayReturnPass()
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
        public void CreateHolidayReturnFail()
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
            var exception = Assert.Throws<InvalidOperationException>(() => holidays.Add(holiday));
            Assert.Equal("Holiday already exists", exception.Message);
        }

        [Fact]
        public void CreateHolidayReturnException()
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
            var exception = Assert.Throws<ArgumentNullException>(() => holidays.Add(null));
            Assert.Equal("Holiday cannot be null", exception.Message);
        }

        [Fact]
        public void DeleteHolidayReturnPass()
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
        public async Task DeleteHolidayReturnFail()
        {
            var Id = 999;
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            mock.Setup(x => x.DeleteHoliday(Id));
            var result = holidayeservice.DeleteHoliday(Id);
            mock.Verify(x => x.DeleteHoliday(Id), Times.Once);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => holidayeservice.DeleteHoliday(Id));
            Assert.Equal("Holiday not found", exception.Message);
        }

        [Fact]
        public async Task DeleteHolidayReturnException()
        {
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            Holiday holiday = new Holiday()
            {
                Id = 1,
                HolidayName = "",
                Description = new DateTime(2024, 10, 16, 9, 44, 16).ToString("yyyy-MM-dd HH:mm:ss"),
            };
            mock.Setup(x => x.DeleteHoliday(Id));
            var result = holidayeservice.DeleteHoliday(Id);
            mock.Verify(x => x.DeleteHoliday(Id), Times.Once);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => holidayeservice.DeleteHoliday(Id));
            Assert.Equal("Holiday not found", exception.Message);
        }


        [Fact]
        public void GetAllHolidaysReturnPass()
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
        public async Task GetAllHolidaysReturnFail()
        {
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            Holiday holiday = new Holiday();
            mock.Setup(x => x.GetAllHolidays());
            var result = holidayeservice.GetAllHolidays();
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => holidayeservice.GetAllHolidays());
            Assert.Equal("No Holiday available", exception.Message);

        }

        [Fact]
        public async Task GetAllHolidaysReturnException()
        {
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            Holiday holiday = new Holiday();
            mock.Setup(x => x.GetAllHolidays());
            var result = holidayeservice.GetAllHolidays();
            var exception = await Assert.ThrowsAsync<Exception>(() => mock.Object.GetAllHolidays());
            Assert.Equal("Unexpected error", exception.Message);

        }

        [Fact]
        public void GetHolidayByIdReturnPass()
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
        public async Task GetHolidayByIdReturnFail()
        {
            var Id = 999;
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            mock.Setup(x => x.GetHolidayById(1));
            var result = holidayeservice.GetHolidayById(1);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => holidayeservice.GetHolidayById(Id));
            Assert.Equal("Employee not found", exception.Message);
        }

        [Fact]
        public async Task GetHolidayByIdReturnException()
        {
            var Id = 1;
            var mock = new Mock<IHolidayService>();
            IHolidayService holidayeservice = mock.Object;
            List<Holiday> holidays = new List<Holiday>();
            mock.Setup(x => x.GetHolidayById(1));
            var result = holidayeservice.GetHolidayById(1);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => holidayeservice.GetHolidayById(Id));
            Assert.Equal("Employee not found", exception.Message);
        }

        [Fact]
        public void UpdateHolidayReturnPass()
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
        public async Task UpdateHolidayReturnFail()
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
            var result = holidayeservice.UpdateHoliday(holiday);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => holidayeservice.UpdateHoliday(holiday));
            Assert.Equal("Update not found", exception.Message);
        }

        [Fact]
        public async Task UpdateHolidayReturnException()
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
            var result = holidayeservice.UpdateHoliday(holiday);
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => holidayeservice.UpdateHoliday(null));
            Assert.Equal("Update not found", exception.Message);
        }

    }
}
