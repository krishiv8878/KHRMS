using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace KHRMS.UnitTest.ControllerTests
{

    public class EmailTemplateControllerTest
    {
        private readonly Mock<IEmailTemplateService> _mockService;
        private readonly EmailTemplateController _controller;

        public EmailTemplateControllerTest()
        {
            _mockService = new Mock<IEmailTemplateService>();
            _controller = new EmailTemplateController(_mockService.Object);
        }

        [Fact]
        public async Task GetAll_WhenCalled_ReturnsOkResultWithTemplates()
        {
            var emailTemplates = new List<EmailTemplatesMaster>
            {
                new EmailTemplatesMaster { Id = 1, EmailTemplateTypeId = 101, TemplateHtml = "<h1>Template 1</h1>" },
                new EmailTemplatesMaster { Id = 2, EmailTemplateTypeId = 102, TemplateHtml = "<h1>Template 2</h1>" }
            };

            _mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(emailTemplates);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<IEnumerable<EmailTemplatesMaster>>>(okResult.Value);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.Equal(2, ((List<EmailTemplatesMaster>)response.Data).Count);
        }

        [Fact]
        public async Task GetByTemplateTypeId_ExistingId_ReturnsOkResult()
        {
            var template = new EmailTemplatesMaster { Id = 1, EmailTemplateTypeId = 101, TemplateHtml = "<h1>Template</h1>" };
            _mockService.Setup(service => service.GetByEmailTemplateTypeIdAsync(101)).ReturnsAsync(template);

            var result = await _controller.GetByTemplateTypeId(101);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<EmailTemplatesMaster>>(okResult.Value);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Data);
            Assert.Equal(101, response.Data.EmailTemplateTypeId);
        }

        [Fact]
        public async Task GetByTemplateTypeId_NonExistingId_ReturnsNotFound()
        {
            _mockService.Setup(service => service.GetByEmailTemplateTypeIdAsync(999)).ReturnsAsync((EmailTemplatesMaster)null);

            var result = await _controller.GetByTemplateTypeId(999);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse<EmailTemplatesMaster>>(notFoundResult.Value);
            Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
            Assert.Null(response.Data);
        }

        [Fact]
        public async Task AddEmailTemplates_ValidTemplate_ReturnsOk()
        {
            var template = new EmailTemplatesMaster { Id = 3, EmailTemplateTypeId = 103, TemplateHtml = "<h1>New Template</h1>" };

            var result = await _controller.AddEmailTemplates(template);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Data);
        }

        [Fact]
        public async Task UpdateEmailTemplates_ValidTemplate_ReturnsOk()
        {
            var template = new EmailTemplatesMaster { Id = 3, EmailTemplateTypeId = 103, TemplateHtml = "<h1>Updated Template</h1>" };
            _mockService.Setup(service => service.UpdateAsync(template)).ReturnsAsync(true);

            var result = await _controller.UpdateEmailTemplates(template);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Data);
        }

        [Fact]
        public async Task DeleteEmailTemplatesMaster_ExistingId_ReturnsOk()
        {
            var template = new EmailTemplatesMaster { Id = 1, EmailTemplateTypeId = 101, TemplateHtml = "<h1>Template</h1>" };
            _mockService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(template);

            var result = await _controller.DeleteemailTemplatesMaster(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Data);
        }

        [Fact]
        public async Task DeleteEmailTemplatesMaster_NonExistingId_ReturnsNotFound()
        {
            _mockService.Setup(service => service.GetByIdAsync(999)).ReturnsAsync((EmailTemplatesMaster)null);

            var result = await _controller.DeleteemailTemplatesMaster(999);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(notFoundResult.Value);
            Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
            Assert.False(response.Data);
        }

    }
}
