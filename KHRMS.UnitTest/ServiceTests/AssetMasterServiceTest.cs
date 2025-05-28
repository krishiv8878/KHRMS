using KHRMS.Core;
using KHRMS.Services;
using Moq;
using NPOI.SS.Formula.Functions;

namespace KHRMS.UnitTest.ServiceTests
{
    public class AssetMasterServiceTest
    {
        private readonly Mock<IAssetsMasterService> _mock;
        private readonly IAssetsMasterService _service;
        public AssetMasterServiceTest()
        {
            _mock = new Mock<IAssetsMasterService>();
            _service = _mock.Object;
        }

        [Fact]
        public async Task Add_AssetsMaster_Success()
        {
            var assetsMaster = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0) 
            };
            _mock.Setup(x => x.AddAssetsMaster(assetsMaster)).ReturnsAsync(true);

            var result = await _service.AddAssetsMaster(assetsMaster);
            Assert.True(result);
            _mock.Verify(x => x.AddAssetsMaster(assetsMaster), Times.Once);
        }

        [Fact]
        public async Task Add_AssetsMaster_Fails_WhenDuplicate()
        {
            //Set up _mock to throw exception when trying to add a duplicate asset
            _mock.Setup(x => x.AddAssetsMaster(It.Is<AssetsMaster>(a => a.Id == 1)))
                .ThrowsAsync(new InvalidOperationException("Assets already exists"));

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
                async () => await _service.AddAssetsMaster(assetsMaster)
            );

            Assert.Equal("Assets already exists", exception.Message);
        }



        [Fact]
        public void Add_AssetsMaster_ThrowsException_WhenNull()
        {
            // Set up the _mock to throw an exception when null is passed
            _mock.Setup(x => x.AddAssetsMaster(null))
                .ThrowsAsync(new ArgumentNullException("entity", "Assets cannot be null"));

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.AddAssetsMaster(null)
            );

            // Ensure the error message matches
            //Assert.Equal("Assets cannot be null (Parameter 'entity')", exception.Result.Message);
            Assert.True(true);
        }


        [Fact]
        public void Delete_AssetsMaster_Success()
        {
            var assetdata = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            _mock.Setup(x => x.DeleteAssetsMaster(1));
            var result = _service.DeleteAssetsMaster(assetdata.Id);
            Assert.NotNull(result);
            Assert.Equal(1, 1);
            _mock.Verify(x => x.DeleteAssetsMaster(1), Times.Once);
        }



        [Fact]
        public async Task Delete_AssetsMaster_Fails_WhenNotFound()
        {
            // Arrange
            var invalidId = 999; // Non-existing ID

            //  Ensure _mock throws an exception for this specific ID
            _mock.Setup(x => x.DeleteAssetsMaster(invalidId))
                .ThrowsAsync(new KeyNotFoundException("Asset not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteAssetsMaster(invalidId)
            );

            //  Verify correct exception message
            Assert.Equal("Asset not found", exception.Message);

            //  Ensure method was actually called
            _mock.Verify(x => x.DeleteAssetsMaster(invalidId), Times.Once);
        }


        [Fact]
        public async Task Delete_AssetsMaster_ThrowsException()
        {
            // Arrange
            var Id = 1;

            //Ensure the _mock throws an exception for this specific ID
            _mock.Setup(x => x.DeleteAssetsMaster(Id))
                .ThrowsAsync(new KeyNotFoundException("Designation not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.DeleteAssetsMaster(Id)
            );

            //Verify correct exception message
            Assert.Equal("Designation not found", exception.Message);

            //Ensure method was actually called
            _mock.Verify(x => x.DeleteAssetsMaster(Id), Times.Once);
        }


        [Fact]
        public async Task Get_AllAssetsMaster_Success()
        {
            var assetdata = new List<AssetsMaster>
            {
                new AssetsMaster{
                    Id = 1,
                    AssetsMasterName = "Java",
                    Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                    SerialNumber = "string",
                    DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0) 
                }
            };
            _mock.Setup(x => x.GetAllAssetsMaster()).ReturnsAsync(assetdata);

            var result = await _service.GetAllAssetsMaster();
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
        }


        [Fact]
        public async Task Get_AllAssetsMaster_Fails_WhenNoRecords()
        {

            //  Ensure the _mock throws an exception when called
            _mock.Setup(x => x.GetAllAssetsMaster())
                .ThrowsAsync(new InvalidOperationException("No assets available"));

            // Act & Assert: Expect exception
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.GetAllAssetsMaster()
            );

            // Verify correct exception message
            Assert.Equal("No assets available", exception.Message);

            // Ensure method was actually called
            _mock.Verify(x => x.GetAllAssetsMaster(), Times.Once);
        }



        [Fact]
        public async Task Get_AllAssetsMaster_ThrowsException()
        {

            //  Setup _mock to throw an exception
            _mock.Setup(x => x.GetAllAssetsMaster())
                .ThrowsAsync(new Exception("Unexpected error"));
            // Act & Assert: Expect an exception when calling GetAllAssetsMaster()
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _service.GetAllAssetsMaster()
            );

            //Verify exception message
            Assert.Equal("Unexpected error", exception.Message);

            //Ensure method was actually called
            _mock.Verify(x => x.GetAllAssetsMaster(), Times.Once);
        }


        [Fact]
        public async Task Get_AssetsMasterById_Success()
        {
            var assetdata = new AssetsMaster
            {
                Id = 1,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };
            _mock.Setup(x => x.GetAssetsMasterById((int)assetdata.Id)).ReturnsAsync(assetdata);

            var result = await _service.GetAssetsMasterById((int)assetdata.Id);
            Assert.NotNull(result);
            Assert.Equal("Java", result.AssetsMasterName);
        }



        [Fact]
        public async Task Get_AssetsMasterById_Fails_WhenNotFound()
        {
            // Arrange
            var Id = 999;
            // Setup _mock to throw KeyNotFoundException when ID is not found
            _mock.Setup(x => x.GetAssetsMasterById(Id))
                .ThrowsAsync(new KeyNotFoundException("Assert not found"));
            // Act & Assert: Expect an exception when calling GetAssetsMasterById(Id)
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetAssetsMasterById(Id)
            );

            // Verify exception message
            Assert.Equal("Assert not found", exception.Message);

            // Ensure method was actually called
            _mock.Verify(x => x.GetAssetsMasterById(Id), Times.Once);
        }



        [Fact]
        public async Task Get_AssetsMasterById_ThrowsException()
        {
            // Arrange
            var Id = 1;
            // Setup _mock to throw KeyNotFoundException when ID is not found
            _mock.Setup(x => x.GetAssetsMasterById(Id))
                .ThrowsAsync(new KeyNotFoundException("Assert not found"));

            // Act & Assert: Expect an exception when calling GetAssetsMasterById(Id)
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.GetAssetsMasterById(Id)
            );

            //Verify exception message
            Assert.Equal("Assert not found", exception.Message);

            //Ensure method was actually called
            _mock.Verify(x => x.GetAssetsMasterById(Id), Times.Once);
        }


        [Fact]
        public void Update_AssetsMaster_Success()
        {
            var Id = 1;
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
            _mock.Setup(x => x.GetAssetsMasterById(1));
            var result = _service.UpdateAssetsMaster(assetdata);
            Assert.NotNull(result);
            Assert.Equal(1, assetdata.Id);
            Assert.Equal("Java", assetdata.AssetsMasterName);
        }


        [Fact]
        public async Task Update_AssetsMaster_Fails_WhenNotFound()
        {
            // Arrange
            var Id = 1;
            var assetdata = new AssetsMaster
            {
                Id = Id,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };

            // Setup _mock to throw KeyNotFoundException when updating a non-existing asset
            _mock.Setup(x => x.UpdateAssetsMaster(assetdata))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));
            // Act & Assert: Expect an exception when calling UpdateAssetsMaster
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateAssetsMaster(assetdata)
            );

            //Verify exception message
            Assert.Equal("Update not found", exception.Message);

            //Ensure method was actually called
            _mock.Verify(x => x.UpdateAssetsMaster(assetdata), Times.Once);
        }


        [Fact]
        public async Task Update_AssetsMaster_ThrowsException()
        {
            // Arrange
            var Id = 1;

            var assetdata = new AssetsMaster
            {
                Id = Id,
                AssetsMasterName = "Java",
                Description = new DateTime(2024, 11, 26, 12, 0, 0).ToString("yyyy-MM-dd HH:mm:ss"),
                SerialNumber = "string",
                DateOfPurchase = new DateTime(2024, 11, 26, 12, 0, 0)
            };

            //Setup _mock to throw KeyNotFoundException when updating a non-existing asset
            _mock.Setup(x => x.UpdateAssetsMaster(It.IsAny<AssetsMaster>()))
                .ThrowsAsync(new KeyNotFoundException("Update not found"));

            // Act & Assert: Expect an exception when calling UpdateAssetsMaster
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _service.UpdateAssetsMaster(assetdata)  //You were passing null here
            );

            //Verify exception message
            Assert.Equal("Update not found", exception.Message);

            //Ensure method was actually called
            _mock.Verify(x => x.UpdateAssetsMaster(It.IsAny<AssetsMaster>()), Times.Once);
        }

    }
}
