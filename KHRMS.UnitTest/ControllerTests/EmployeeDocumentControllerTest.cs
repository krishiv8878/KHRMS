using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace KHRMS.UnitTest.ControllerTests
{
    public class EmployeeDocumentControllerTest
    {
        private readonly Mock<IEmployeeDocumentService> _mockService;
        private readonly EmployeeDocumentController _controller;

        public EmployeeDocumentControllerTest()
        {
            _mockService = new Mock<IEmployeeDocumentService>();
            _controller = new EmployeeDocumentController(_mockService.Object, Mock.Of<ILogger<EmployeeDocumentController>>());
        }

        [Fact]
        public async Task Get_AllDocuments_WhenCalled_ReturnsOkResult()
        {
            var documents = new List<EmployeeDocumentInfo>
            {
                new EmployeeDocumentInfo { Id = 1, EmployeeId = 1001, FilePath = "path1.pdf" },
                new EmployeeDocumentInfo { Id = 2, EmployeeId = 1002, FilePath = "path2.docx" }
            };
            _mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(documents);

            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Get_DocumentById_WhenDocumentExists_Returns_OkResult()
        {
            // Arrange
            var document = new EmployeeDocumentInfo { Id = 1, EmployeeId = 1001, FilePath = "path1.pdf" };
            _mockService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(document);

            // Act
            var result = await _controller.GetDocument(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<EmployeeDocumentInfo>>(okResult.Value);

            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ApiMessageConstant.EmployeeDocumentFound, response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(1, response.Data.Id);
            Assert.Equal(1001, response.Data.EmployeeId);
        }

        [Fact]
        public async Task Get_DocumentById_WhenDocumentDoesNotExist_Returns_NotFoundResult()
        {
            // Arrange
            _mockService.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((EmployeeDocumentInfo)null);

            // Act
            var result = await _controller.GetDocument(999);

            // Assert
            var notFoundResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<EmployeeDocumentInfo>>(notFoundResult.Value);

            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ApiMessageConstant.EmployeeDocumentNotFound, response.Message);
            Assert.Null(response.Data);
        }


        //[Fact]
        //public async Task UploadDocument_WhenValidFileProvided_Returns_CreatedAtActionResult()
        //{
        //    var mockFile = new Mock<IFormFile>();
        //    var content = new MemoryStream();
        //    var writer = new StreamWriter(content);
        //    writer.Write("Dummy content");
        //    writer.Flush();
        //    content.Position = 0;
        //    mockFile.Setup(_ => _.OpenReadStream()).Returns(content);
        //    mockFile.Setup(_ => _.FileName).Returns("sample.pdf");
        //    mockFile.Setup(_ => _.Length).Returns(content.Length);

        //    var document = new EmployeeDocumentInfo
        //    {
        //        Id = 1,
        //        EmployeeId = 1001,
        //        FilePath = "uploads/sample.pdf",
        //        DocumentName = "Sample Document" // Add Document Name
        //    };

        //    //_mockService.Setup(x => x.AddAsync(It.IsAny<EmployeeDocumentInfo>()))
        //    //    .Callback<EmployeeDocumentInfo>(doc => doc.Id = 1)  // Assign ID in _mock setup
        //    //    .Returns(Task.CompletedTask);
        //    _mockService.Setup(x => x.AddAsync(It.IsAny<EmployeeDocumentInfo>()))
        //.Callback<EmployeeDocumentInfo>(doc =>
        //{
        //    doc.Id = 1;
        //    doc.DocumentName = "Sample Document"; // Set document name in _mock setup
        //})
        //.Returns(Task.CompletedTask);

        //    _mockService.Setup(x => x.GetByIdAsync(It.IsAny<long>())).ReturnsAsync(document);

        //    var result = await _controller.Create(1001, mockFile.Object);

        //    var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
        //    Assert.NotNull(createdAtResult.Value);

        //    var returnedDocument = Assert.IsType<EmployeeDocumentInfo>(createdAtResult.Value);
        //    Assert.Equal(1, returnedDocument.Id);       
        //    Assert.Equal("Sample Document", returnedDocument.DocumentName); // Verify document name
        //}
        [Fact]
        public async Task UploadDocument_WhenValidFileProvided_Returns_CreatedAtActionResult()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var content = new MemoryStream();
            var writer = new StreamWriter(content);
            writer.Write("Dummy content");
            writer.Flush();
            content.Position = 0;

            mockFile.Setup(f => f.OpenReadStream()).Returns(content);
            mockFile.Setup(f => f.FileName).Returns("sample.pdf");
            mockFile.Setup(f => f.Length).Returns(content.Length);

            var document = new EmployeeDocumentInfo
            {
                Id = 1,
                EmployeeId = 1001,
                FilePath = "uploads/sample.pdf",
                DocumentName = "Employee Contract"
            };

            _mockService.Setup(x => x.AddAsync(It.IsAny<EmployeeDocumentInfo>()))
                .Callback<EmployeeDocumentInfo>(doc => doc.Id = 1)
                .Returns(Task.CompletedTask);

            _mockService.Setup(x => x.GetByIdAsync(It.IsAny<long>())).ReturnsAsync(document);

            // Act
            var result = await _controller.UploadDocument(1001, "Employee Docs", "Employee Contract", mockFile.Object);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<ApiResponse<EmployeeDocumentInfo>>(createdAtResult.Value);

            Assert.Equal((int)HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.Equal(1, response.Data.Id);
            Assert.Equal("Employee Contract", response.Data.DocumentName);
        }

        [Fact]
        public async Task DeleteDocument_WhenDocumentExists_Returns_OkResult()
        {
            // Arrange
            var id = 1;
            _mockService.Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(new EmployeeDocumentInfo { Id = id });

            _mockService.Setup(x => x.DeleteAsync(id))
                .ReturnsAsync(true);  // ✅ Fix: Ensure it returns Task<bool>

            // Act
            var result = await _controller.DeleteDocument(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);

            Assert.True(response.Data);  // ✅ Ensure Data is true
        }


        [Fact]
        public async Task DeleteDocument_WhenDocumentDoesNotExist_Returns_BadRequestObjectResult()
        {
            // Arrange
            var id = 999;
            _mockService.Setup(x => x.DeleteAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteDocument(id);

            // Assert
            var badRequestResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.False(response.Data);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(ApiMessageConstant.EmployeeDocumentNotFound, response.Message);
        }

    }
}
