using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace KHRMS.UnitTest.ControllerTests
{
    public class EmailControllerTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly EmailController _emailController;

        public EmailControllerTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _emailController = new EmailController(_mockEmailService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOk_WithEmails()
        {
            var emails = new List<Email> { new Email { Id = 1, EmailSubject = "Test", EmailBody = "Body Content" } };
            _mockEmailService.Setup(service => service.GetAllAsync()).ReturnsAsync(emails);

            var result = await _emailController.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<IEnumerable<Email>>>(okResult.Value);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task GetByTemplateId_ReturnsNotFound_WhenNoEmailFound()
        {
            _mockEmailService.Setup(service => service.GetByEmailTemplatesIdAsync(It.IsAny<long>())).ReturnsAsync((Email)null);

            var result = await _emailController.GetByTemplateId(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse<Email>>(notFoundResult.Value);
            Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddEmails_ReturnsOk_WhenValidEmail()
        {
            var email = new Email { Id = 1, EmailSubject = "Test Email", EmailBody = "Sample Body" };
            _mockEmailService.Setup(service => service.AddAsync(It.IsAny<Email>())).Returns(Task.CompletedTask);

            var result = await _emailController.AddEmails(email);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.Data);
        }

        [Fact]
        public async Task UpdateEmails_ReturnsBadRequest_WhenModelIsInvalid()
        {
            _emailController.ModelState.AddModelError("EmailSubject", "Required");

            var email = new Email { Id = 1, EmailBody = "Updated Body" };

            var result = await _emailController.UpdateEmails(email);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(badRequestResult.Value);
            Assert.False(response.Data);
        }



        [Fact]
        public async Task DeleteEmails_ReturnsNotFound_WhenEmailDoesNotExist()
        {
            // Arrange
            _mockEmailService.Setup(service => service.GetByIdAsync(It.IsAny<long>()))
                             .ReturnsAsync((Email)null);

            // Act
            var result = await _emailController.Deleteemails(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(notFoundResult.Value);
            Assert.False(response.Data);
            Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ApiMessageConstant.EmailNotFound, response.Message);
        }

    }
}
