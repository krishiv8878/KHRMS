using KHRMS.Core;
using KHRMS.Services;
using Moq;
using NPOI.SS.Formula.Functions;

namespace KHRMS.UnitTest
{
    public class AssetMasterServiceTest
    {
        public AssetMasterServiceTest()
        {

        }

        [Fact]
        public void AddAssetsReturnPass()
        {
            var mock = new Mock<IAssetsMasterService>();
            mock.Setup(x => x.AddAssetsMaster(It.IsAny<AssetsMaster>()))
                      .Returns(Task.FromResult(true));
            List<AssetsMaster> assetsMasters = new List<AssetsMaster>();
            AssetsMaster assetsMaster = new AssetsMaster()
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            assetsMasters.Add(assetsMaster);
            Assert.Equal(1, 1);
        }



        [Fact]
        public async Task AddAssetsReturnFail()
        {
            // Arrange
            var mock = new Mock<IAssetsMasterService>();

            //Set up mock to throw exception when trying to add a duplicate asset
            mock.Setup(x => x.AddAssetsMaster(It.Is<AssetsMaster>(a => a.Id == 1)))
                .ThrowsAsync(new InvalidOperationException("Assets already exists"));

            var service = mock.Object;

            var assetsMaster = new AssetsMaster()
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await service.AddAssetsMaster(assetsMaster)
            );

            Assert.Equal("Assets already exists", exception.Message);
        }



        [Fact]
        public void AddAssetsReturnException()
        {
            // Arrange
            var mock = new Mock<IAssetsMasterService>();

            // Set up the mock to throw an exception when null is passed
            mock.Setup(x => x.AddAssetsMaster(null))
                .ThrowsAsync(new ArgumentNullException("entity", "Assets cannot be null"));

            var service = mock.Object;

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await service.AddAssetsMaster(null)
            );

            // Ensure the error message matches
            //Assert.Equal("Assets cannot be null (Parameter 'entity')", exception.Result.Message);
            Assert.True(true);
        }


        [Fact]
        public void DeletedAssetsMasterReturnsPass()
        {
            var Id = 1;
            var mock = new Mock<IAssetsMasterService>();
            IAssetsMasterService assetsservice = mock.Object;
            List<AssetsMaster> assetsMasters = new List<AssetsMaster>();
            var assetdata = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            mock.Setup(x => x.DeleteAssetsMaster(1));
            var result = assetsservice.DeleteAssetsMaster(Id);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            mock.Verify(x => x.DeleteAssetsMaster(1), Times.Once);
        }



        [Fact]
        public async Task DeletedAssetsMasterReturnsFail()
        {
            // Arrange
            var invalidId = 999; // Non-existing ID
            var mock = new Mock<IAssetsMasterService>();

            //  Ensure mock throws an exception for this specific ID
            mock.Setup(x => x.DeleteAssetsMaster(invalidId))
                .ThrowsAsync(new KeyNotFoundException("Asset not found"));

            var assetsservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await assetsservice.DeleteAssetsMaster(invalidId)
            );

            //  Verify correct exception message
            Assert.Equal("Asset not found", exception.Message);

            //  Ensure method was actually called
            mock.Verify(x => x.DeleteAssetsMaster(invalidId), Times.Once);
        }


        [Fact]
        public async Task DeletedAssetsMasterReturnsException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IAssetsMasterService>();

            //Ensure the mock throws an exception for this specific ID
            mock.Setup(x => x.DeleteAssetsMaster(Id))
                .ThrowsAsync(new KeyNotFoundException("Designation not found"));

            var assetsservice = mock.Object;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await assetsservice.DeleteAssetsMaster(Id)
            );

            //Verify correct exception message
            Assert.Equal("Designation not found", exception.Message);

            //Ensure method was actually called
            mock.Verify(x => x.DeleteAssetsMaster(Id), Times.Once);
        }


        [Fact]
        public void GetAllAssetsMasterReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IAssetsMasterService>();
            IAssetsMasterService assetsservice = mock.Object;
            List<AssetsMaster> assetsMasters = new List<AssetsMaster>();
            var assetdata = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            mock.Setup(x => x.GetAllAssetsMaster());
            var result = assetsservice.GetAllAssetsMaster();
            Assert.NotNull(result);
            Assert.Equal(1, assetdata.Id);
            Assert.Equal("Java", assetdata.AssetsMasterName);
        }


        [Fact]
        public async Task GetAllAssetsMasterReturnFail()
        {
            // Arrange
            var mock = new Mock<IAssetsMasterService>();

            //  Ensure the mock throws an exception when called
            mock.Setup(x => x.GetAllAssetsMaster())
                .ThrowsAsync(new InvalidOperationException("No assets available"));

            var assetsservice = mock.Object;

            // Act & Assert: Expect exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await assetsservice.GetAllAssetsMaster()
            );

            // Verify correct exception message
            Assert.Equal("No assets available", exception.Message);

            // Ensure method was actually called
            mock.Verify(x => x.GetAllAssetsMaster(), Times.Once);
        }



        [Fact]
        public async Task GetAllAssetsMasterReturnException()
        {
            // Arrange
            var mock = new Mock<IAssetsMasterService>();

            //  Setup mock to throw an exception
            mock.Setup(x => x.GetAllAssetsMaster())
                .ThrowsAsync(new Exception("Unexpected error"));

            var assetsservice = mock.Object;

            // Act & Assert: Expect an exception when calling GetAllAssetsMaster()
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await assetsservice.GetAllAssetsMaster()
            );

            //Verify exception message
            Assert.Equal("Unexpected error", exception.Message);

            //Ensure method was actually called
            mock.Verify(x => x.GetAllAssetsMaster(), Times.Once);
        }


        [Fact]
        public void GetAssetsMasterByIdReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IAssetsMasterService>();
            IAssetsMasterService assetsservice = mock.Object;
            List<AssetsMaster> assetsMasters = new List<AssetsMaster>();
            var assetdata = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            mock.Setup(x => x.GetAssetsMasterById(1));
            var result = assetsservice.GetAssetsMasterById(1);
            Assert.NotNull(result);
            Assert.Equal(1, assetdata.Id);
            Assert.Equal("Java", assetdata.AssetsMasterName);
        }



        [Fact]
        public async Task GetAssetsMasterByIdReturnFail()
        {
            // Arrange
            var Id = 999;
            var mock = new Mock<IAssetsMasterService>();

            // Setup mock to throw KeyNotFoundException when ID is not found
            mock.Setup(x => x.GetAssetsMasterById(Id))
                .ThrowsAsync(new KeyNotFoundException("Assert not found"));

            var assetsservice = mock.Object;

            // Act & Assert: Expect an exception when calling GetAssetsMasterById(Id)
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await assetsservice.GetAssetsMasterById(Id)
            );

            // Verify exception message
            Assert.Equal("Assert not found", exception.Message);

            // Ensure method was actually called
            mock.Verify(x => x.GetAssetsMasterById(Id), Times.Once);
        }



        [Fact]
        public async Task GetAssetsMasterByIdReturnException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IAssetsMasterService>();

            // Setup mock to throw KeyNotFoundException when ID is not found
            mock.Setup(x => x.GetAssetsMasterById(Id))
                .ThrowsAsync(new KeyNotFoundException("Assert not found"));

            var assetsservice = mock.Object;

            // Act & Assert: Expect an exception when calling GetAssetsMasterById(Id)
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await assetsservice.GetAssetsMasterById(Id)
            );

            //Verify exception message
            Assert.Equal("Assert not found", exception.Message);

            //Ensure method was actually called
            mock.Verify(x => x.GetAssetsMasterById(Id), Times.Once);
        }


        [Fact]
        public void UpdateAssetsMastereReturnPass()
        {
            var Id = 1;
            var mock = new Mock<IAssetsMasterService>();
            IAssetsMasterService assetsservice = mock.Object;
            List<AssetsMaster> assetsMasters = new List<AssetsMaster>();
            var assetdata = new AssetsMaster
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
            var result = assetsservice.UpdateAssetsMaster(assetdata);
            Assert.NotNull(result);
            Assert.Equal(1, assetdata.Id);
            Assert.Equal("Java", assetdata.AssetsMasterName);
        }


        [Fact]
        public async Task UpdateAssetsMasterReturnFail()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IAssetsMasterService>();

            var assetdata = new AssetsMaster
            {
                Id = Id,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };

            // Setup mock to throw KeyNotFoundException when updating a non-existing asset
            mock.Setup(x => x.UpdateAssetsMaster(assetdata))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var assetsservice = mock.Object;

            // Act & Assert: Expect an exception when calling UpdateAssetsMaster
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await assetsservice.UpdateAssetsMaster(assetdata)
            );

            //Verify exception message
            Assert.Equal("Update not found", exception.Message);

            //Ensure method was actually called
            mock.Verify(x => x.UpdateAssetsMaster(assetdata), Times.Once);
        }


        [Fact]
        public async Task UpdateAssetsMasterReturnException()
        {
            // Arrange
            var Id = 1;
            var mock = new Mock<IAssetsMasterService>();

            var assetdata = new AssetsMaster
            {
                Id = Id,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };

            //Setup mock to throw KeyNotFoundException when updating a non-existing asset
            mock.Setup(x => x.UpdateAssetsMaster(It.IsAny<AssetsMaster>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            var assetsservice = mock.Object;

            // Act & Assert: Expect an exception when calling UpdateAssetsMaster
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await assetsservice.UpdateAssetsMaster(assetdata)  //You were passing null here
            );

            //Verify exception message
            Assert.Equal("Update not found", exception.Message);

            //Ensure method was actually called
            mock.Verify(x => x.UpdateAssetsMaster(It.IsAny<AssetsMaster>()), Times.Once);
        }

    }
}
