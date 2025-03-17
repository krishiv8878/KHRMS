using KHRMS.Core;
using KHRMS.Services;
using Moq;


namespace KHRMS.UnitTest.ServiceTests
{
    public class ShiftServiceTest
    {
        private readonly Mock<IShiftService> _mock;
        public ShiftServiceTest()
        {
            _mock = new Mock<IShiftService>();
        }
        [Fact]
        public async Task Shift_AddSuccessfully()
        {
            ShiftMaster shiftMaster = new ShiftMaster() 
            { 
                Id = 1,
                ShiftName = "Night",
                StartTime = new TimeOnly(10,0),
                EndTime = new TimeOnly(6,0)
            };
            _mock.Setup(x=>x.AddShiftAsync(It.IsAny<ShiftMaster>())).Returns(Task.CompletedTask);

            await _mock.Object.AddShiftAsync(shiftMaster);

            _mock.Verify(x=>x.AddShiftAsync(It.IsAny<ShiftMaster>()),Times.Once);
        }
        [Fact]
        public async Task ShiftMaster_ThrowException_WhenDataInvalid()
        {
            ShiftMaster shiftMaster = new ShiftMaster()
            {
                Id = 0, //invalid id
                ShiftName = "Test",
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(6, 0)
            };
            _mock.Setup(x => x.AddShiftAsync(It.IsAny<ShiftMaster>())).ThrowsAsync(new ArgumentException("Invalid Data of shiftmaster"));

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.AddShiftAsync(shiftMaster));

            Assert.Equal("Invalid Data of shiftmaster",exception.Message);
        }

        [Fact]
        public async Task ShiftMaster_ThrowException_WhenDataisNull()
        {
            ShiftMaster shiftMaster = null;

            _mock.Setup(x => x.AddShiftAsync(It.IsAny<ShiftMaster>())).ThrowsAsync(new Exception("ShiftMaster Data Is Null"));

            var exception = await Assert.ThrowsAsync<Exception>(()=> _mock.Object.AddShiftAsync(shiftMaster));

            Assert.Equal("ShiftMaster Data Is Null", exception.Message);
        }

        [Fact]
        public async Task Shiftmaster_GetSuccessfully()
        {
            var shiftMaster = new List<ShiftMaster>()
            {
                new ShiftMaster{Id=1,ShiftName="Night",StartTime = new TimeOnly(1, 0),EndTime = new TimeOnly(6, 0)},
                new ShiftMaster{Id=2,ShiftName="Day",StartTime = new TimeOnly(10, 0),EndTime = new TimeOnly(6, 0)},
            };
            _mock.Setup(x => x.GetAllShiftsAsync()).ReturnsAsync(shiftMaster);
            var result = await _mock.Object.GetAllShiftsAsync();

            _mock.Verify(x=>x.GetAllShiftsAsync(), Times.Once());
            Assert.Equal(shiftMaster.Count(), result.Count());
            Assert.Contains(result, r => r.Id == 1);
            Assert.Contains(result, r => r.Id == 2);
        }
        [Fact]
        public async Task ShiftMaster_NotFound()
        {
            var shiftMasternotfound = new List<ShiftMaster>();
            _mock.Setup(x => x.GetAllShiftsAsync()).ThrowsAsync(new InvalidOperationException("ShiftMaster not found"));
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _mock.Object.GetAllShiftsAsync());
            Assert.Equal("ShiftMaster not found", exception.Message);
        }
        [Fact]
        public async Task ShiftMaster_GetSuccessFullyById()
        {
            var shiftMasterid = 1;
            var shiftmaster = new ShiftMaster { Id = shiftMasterid };
            _mock.Setup(x=>x.GetShiftByIdAsync(shiftMasterid)).ReturnsAsync(shiftmaster);
            await _mock.Object.GetShiftByIdAsync(shiftMasterid);
            _mock.Verify(x=>x.GetShiftByIdAsync(shiftMasterid), Times.Once());
        }
        [Fact]
        public async Task ShiftMaster_IdNotFound()
        {
            var shiftMasterid = 1;
            var shiftmaster = new ShiftMaster { Id = shiftMasterid };
            _mock.Setup(x => x.GetShiftByIdAsync(shiftMasterid)).ThrowsAsync(new ArgumentException("ShiftMasterId Not found"));
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _mock.Object.GetShiftByIdAsync(shiftMasterid));
            Assert.Equal("ShiftMasterId Not found", exception.Message);
        }

        [Fact]
        public async Task ShiftMaster_DeleteSuccessfully()
        {
            var shiftMasterId = 1;
            _mock.Setup(x => x.DeleteShiftAsync(shiftMasterId)).Returns(Task.CompletedTask);
            await _mock.Object.DeleteShiftAsync(shiftMasterId);
            _mock.Verify(x => x.DeleteShiftAsync(shiftMasterId), Times.Once);
        }

        [Fact]
        public async Task ShiftMaster_DeleteThrowException_WhenNotFound()
        {
            var shiftMasterId = 1;
            _mock.Setup(x => x.DeleteShiftAsync(shiftMasterId)).ThrowsAsync(new KeyNotFoundException("ShiftMaster not found"));
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.DeleteShiftAsync(shiftMasterId));

            Assert.Equal("ShiftMaster not found", exception.Message);
        }

        [Fact]
        public async Task ShiftMaster_UpdateSuccessfully()
        {
            var shiftMaster = new ShiftMaster
            {
                Id = 1,
                ShiftName = "Night",
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(6, 0)
            };
            var UpdateashiftMaster = new ShiftMaster
            {
                Id = 1,
                ShiftName = "Day",//update shiftname
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(6, 0)
            };

            _mock.Setup(x => x.UpdateShiftAsync(It.IsAny<ShiftMaster>())).Returns(Task.CompletedTask);

            await _mock.Object.UpdateShiftAsync(UpdateashiftMaster);

            _mock.Verify(x => x.UpdateShiftAsync(It.Is<ShiftMaster>(r =>
                r.Id == UpdateashiftMaster.Id &&
                r.ShiftName == UpdateashiftMaster.ShiftName)), Times.Once());
        }
        [Fact]
        public async Task ShiftMaster_UpdateThrowException_WhenRequestNotFound()
        {
            var shiftMaster = new ShiftMaster
            {
                Id = 9999,//id doesnt exists
                ShiftName = "Night",
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(6, 0)
            };

            _mock.Setup(x => x.UpdateShiftAsync(It.IsAny<ShiftMaster>())).ThrowsAsync(new KeyNotFoundException("Requested id of ShiftMaster not found"));

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _mock.Object.UpdateShiftAsync(shiftMaster));
            Assert.Equal("Requested id of ShiftMaster not found", exception.Message);
        }

    }
}