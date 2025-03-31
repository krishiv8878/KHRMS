using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace KHRMS.UnitTest.ControllerTests
{

    //internal class EmailTemplateMasterControllerTests
    //{
    //}
    public class EmailTemplateMasterControllerTests
    {
        private readonly Mock<IEmailTemplateTypeMasterService> _mockService;
        private readonly EmailTemplateTypeController _controller;

        public EmailTemplateMasterControllerTests()
        {
            _mockService = new Mock<IEmailTemplateTypeMasterService>();
            _controller = new EmailTemplateTypeController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithEmailTemplateTypes()
        {
            // Arrange
            var templateTypes = new List<EmailTemplateTypeMaster> { new EmailTemplateTypeMaster { Id = 1, TemplateType = "Welcome Email" } };
            _mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(templateTypes);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<IEnumerable<EmailTemplateTypeMaster>>>(okResult.Value);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task AddEmailTemplateType_ReturnsOk_WhenValidData()
        {
            // Arrange
            var emailTemplate = new EmailTemplateTypeMaster { Id = 1, TemplateType = "Welcome Email" };
            _mockService.Setup(service => service.AddAsync(It.IsAny<EmailTemplateTypeMaster>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddEmailTemplateType(emailTemplate);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.Data);
        }

        [Fact]
        public async Task AddEmailTemplateType_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Required");

            var templateType = new EmailTemplateTypeMaster { Id = 1 };

            // Act
            var result = await _controller.AddEmailTemplateType(templateType);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.False(response.Data);
        }

        [Fact]
        public async Task UpdateEmailTemplateType_ReturnsOk_WhenUpdatedSuccessfully()
        {
            // Arrange
            var templateType = new EmailTemplateTypeMaster { Id = 1, TemplateType = "Welcome Email" };
            _mockService.Setup(service => service.GetByIdAsync(It.IsAny<long>())).ReturnsAsync(templateType);

            // Act
            var result = await _controller.UpdateEmailTemplateType(templateType);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.Data);
        }

        [Fact]
        public async Task DeleteEmailTemplateType_ReturnsNotFound_WhenTemplateDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetByIdAsync(It.IsAny<long>())).ReturnsAsync((EmailTemplateTypeMaster)null);

            // Act
            var result = await _controller.DeletEmailTemplateType(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
