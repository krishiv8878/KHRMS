using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class AssetsMasterControllerTest
    {
        public AssetsMasterControllerTest()
        {
           
        }

        [Fact]
        public void Add_AssetsMaster_Returns_Success()
        {
            var mock = new Mock<IAssetsMasterService>();
            mock.Setup(x => x.AddAssetsMaster(It.IsAny<AssetsMaster>()));
            var controller = new AssetsMasterController(mock.Object);
            var Id = 1;
            var assets = new AssetsMaster 
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            var result = controller.AddAssetsMaster(assets);
            Assert.NotNull(result);
            Assert.Equal(Id, result.Id);
            Assert.Equal(assets.Id, 1);
            Assert.Equal("Java", assets.AssetsMasterName);
        }

        [Fact]
        public void Get_All_AssetsMaster_Returns_Success()
        {
            var mock = new Mock<IAssetsMasterService>();
            var controller = new AssetsMasterController(mock.Object);
            var Id = 1;
            var assets = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
;            var result = controller.GetDesignations();
            Assert.NotNull(result);
            Assert.Equal(1, assets.Id);
            Assert.Equal("Java", assets.AssetsMasterName);
        }

        [Fact]
        public void Update_AssetsMaster_Returns_Success()
        {
            var mock = new Mock<IAssetsMasterService>();
            var controller = new AssetsMasterController(mock.Object);
            var Id = 1;
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
            mock.Setup(x => x.GetAssetsMasterById(1));
            var result = controller.UpdateAssetsMaster(assets);
            Assert.NotNull(result);
            Assert.Equal(1, assets.Id);
            Assert.Equal("Java", assets.AssetsMasterName);
        }

        [Fact]
        public void Delete_AssetsMaster_Returns_Success()
        {
            var mock = new Mock<IAssetsMasterService>();
            mock.Setup(x => x.DeleteAssetsMaster(1));
            var controller = new AssetsMasterController(mock.Object);
            var Id = 1;
            var assets = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            var result = controller.DeleteAssetsMaster(1);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteAssetsMaster(1), Times.Once);
        }

    }
}
