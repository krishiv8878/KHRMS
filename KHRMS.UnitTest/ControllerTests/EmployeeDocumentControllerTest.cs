using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

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
        public async Task GetAllDocuments_ReturnsSuccess()
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
        public async Task GetDocumentById_ReturnsSuccess()
        {
            var document = new EmployeeDocumentInfo { Id = 1, EmployeeId = 1001, FilePath = "path1.pdf" };
            _mockService.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(document);

            var result = await _controller.GetDocument(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetDocumentById_ReturnsNotFound()
        {
            _mockService.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((EmployeeDocumentInfo)null);

            var result = await _controller.GetDocument(999);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UploadDocument_ReturnsSuccess()
        {
            var mockFile = new Mock<IFormFile>();
            var content = new MemoryStream();
            var writer = new StreamWriter(content);
            writer.Write("Dummy content");
            writer.Flush();
            content.Position = 0;
            mockFile.Setup(_ => _.OpenReadStream()).Returns(content);
            mockFile.Setup(_ => _.FileName).Returns("sample.pdf");
            mockFile.Setup(_ => _.Length).Returns(content.Length);

            var document = new EmployeeDocumentInfo { Id = 1, EmployeeId = 1001, FilePath = "uploads/sample.pdf" };

            _mockService.Setup(x => x.AddAsync(It.IsAny<EmployeeDocumentInfo>()))
                .Callback<EmployeeDocumentInfo>(doc => doc.Id = 1)  // Assign ID in mock setup
                .Returns(Task.CompletedTask);

            _mockService.Setup(x => x.GetByIdAsync(It.IsAny<long>())).ReturnsAsync(document);

            var result = await _controller.Create(1001, mockFile.Object);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.NotNull(createdAtResult.Value);

            var returnedDocument = Assert.IsType<EmployeeDocumentInfo>(createdAtResult.Value);
            Assert.Equal(1, returnedDocument.Id);
        }



        [Fact]
        public async Task DeleteDocument_ReturnsSuccess()
        {
            var id = 1;
            _mockService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(new EmployeeDocumentInfo { Id = id });
            _mockService.Setup(x => x.DeleteAsync(id)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteDocument(id);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)((ApiResponse<bool>)okResult.Value).Data);
        }

        [Fact]
        public async Task DeleteDocument_ReturnsNotFound()
        {
            var id = 999;
            _mockService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((EmployeeDocumentInfo)null);

            var result = await _controller.DeleteDocument(id);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
