using System.Threading.Tasks;
using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class AssetsMasterControllerTest
    {
        private readonly Mock<IAssetsMasterService> _mock;
        private readonly AssetsMasterController _Controller;
        public AssetsMasterControllerTest()
        {
           _mock = new Mock<IAssetsMasterService>();
            _Controller = new AssetsMasterController(_mock.Object);
        }

        [Fact]
        public async Task Add_AssetsMaster_Returns_Success()
        {
            var assets = new AssetsMaster()
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            _mock.Setup(x => x.AddAssetsMaster(It.IsAny<AssetsMaster>())).ReturnsAsync(true);
            var result = await _Controller.AddAssetsMaster(assets);
            Assert.NotNull(result);
            _mock.Verify(x=>x.AddAssetsMaster(It.IsAny<AssetsMaster>()), Times.Once());
        }

        [Fact]
        public void Get_All_AssetsMaster_Returns_Success()
        {
            var assets = new List<AssetsMaster>
            {
                new AssetsMaster{
                    Id = 1,
                    AssetsMasterName = "Java",
                    Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                    SerialNumber = "string",
                    DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0) 
                }
            };
             _mock.Setup(x => x.GetAllAssetsMaster()).ReturnsAsync(assets);
            var result = _Controller.GetAssetsMaster();
            Assert.NotNull(result);
            _mock.Verify(x => x.GetAllAssetsMaster(), Times.Once());
        }

        [Fact]
        public void Update_AssetsMaster_Returns_Success()
        {
            var assets = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            var updateassetdata = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "C#",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            _mock.Setup(x => x.UpdateAssetsMaster(It.IsAny<AssetsMaster>())).ReturnsAsync(true);
            var result = _Controller.UpdateAssetsMaster(updateassetdata);
            Assert.NotNull(result);

            _mock.Verify(x => x.UpdateAssetsMaster(It.Is<AssetsMaster>(r =>
                r.Id == updateassetdata.Id &&
                r.AssetsMasterName == updateassetdata.AssetsMasterName)), Times.Once());
        }

        [Fact]
        public void Delete_AssetsMaster_Returns_Success()
        {
            var assets = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            _mock.Setup(x => x.DeleteAssetsMaster(assets.Id)).ReturnsAsync(true);

            var result = _Controller.DeleteAssetsMaster(assets.Id);
            Assert.NotNull(result);

            _mock.Verify(x => x.DeleteAssetsMaster(assets.Id), Times.Once);
        }

    }
}
