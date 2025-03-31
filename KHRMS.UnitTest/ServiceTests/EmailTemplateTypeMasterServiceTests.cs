using KHRMS.Core.Models;
using KHRMS.Core;
using KHRMS.Services;
using KHRMS.UnitTest.ServiceTests;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ServiceTests
{
    public class EmailTemplateTypeMasterServiceTests
    {
        private readonly Mock<IEmailTemplateTypeMasterService> _mockService;

        public EmailTemplateTypeMasterServiceTests()
        {
            _mockService = new Mock<IEmailTemplateTypeMasterService>();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsListOfEmailTemplateTypes()
        {
            // Arrange
            var emailTemplates = new List<EmailTemplateTypeMaster>
            {
                new EmailTemplateTypeMaster { Id = 1, TemplateType = "Welcome Email", Description = "Welcome Email Template" },
                new EmailTemplateTypeMaster { Id = 2, TemplateType = "Notification Email", Description = "Notification Template" }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(emailTemplates);

            // Act
            var result = await _mockService.Object.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsTemplate()
        {
            // Arrange
            var emailTemplate = new EmailTemplateTypeMaster { Id = 1, TemplateType = "Welcome Email", Description = "Welcome Template" };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(emailTemplate);

            // Act
            var result = await _mockService.Object.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Welcome Email", result.TemplateType);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((EmailTemplateTypeMaster)null);

            // Act
            var result = await _mockService.Object.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_AddsEmailTemplateType()
        {
            // Arrange
            var emailTemplate = new EmailTemplateTypeMaster { TemplateType = "New Template", Description = "New Template Description" };
            _mockService.Setup(s => s.AddAsync(emailTemplate)).Returns(Task.CompletedTask);

            // Act
            await _mockService.Object.AddAsync(emailTemplate);

            // Assert
            _mockService.Verify(s => s.AddAsync(emailTemplate), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ExistingTemplate_UpdatesSuccessfully()
        {
            // Arrange
            var emailTemplate = new EmailTemplateTypeMaster { Id = 1, TemplateType = "Updated Template", Description = "Updated Description" };
            _mockService.Setup(s => s.UpdateAsync(emailTemplate)).Returns(Task.CompletedTask);

            // Act
            await _mockService.Object.UpdateAsync(emailTemplate);

            // Assert
            _mockService.Verify(s => s.UpdateAsync(emailTemplate), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ExistingId_DeletesSuccessfully()
        {
            // Arrange
            _mockService.Setup(s => s.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            await _mockService.Object.DeleteAsync(1);

            // Assert
            _mockService.Verify(s => s.DeleteAsync(1), Times.Once);
        }
    }


}



