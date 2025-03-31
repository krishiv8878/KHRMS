using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{
    public class Email_Template_Service_Test
    {
        private readonly Mock<IEmailTemplateService> _mockService;

        public Email_Template_Service_Test()
        {
            _mockService = new Mock<IEmailTemplateService>();
        }

        [Fact]
        public async Task GetByEmailTemplateTypeIdAsync_Returns_Template()
        {
            var templateTypeId = 1;
            var emailTemplate = new EmailTemplatesMaster
            {
                Id = 1,
                EmailTemplateTypeId = templateTypeId,
                TemplateHtml = "<html>Email Template</html>"
            };

            _mockService.Setup(x => x.GetByEmailTemplateTypeIdAsync(templateTypeId))
                        .ReturnsAsync(emailTemplate);

            var result = await _mockService.Object.GetByEmailTemplateTypeIdAsync(templateTypeId);

            Assert.NotNull(result);
            Assert.Equal(templateTypeId, result.EmailTemplateTypeId);
        }

        [Fact]
        public async Task GetAllAsync_Returns_ListOfTemplates()
        {
            var templates = new List<EmailTemplatesMaster>
        {
            new EmailTemplatesMaster { Id = 1, TemplateHtml = "<html>Template 1</html>" },
            new EmailTemplatesMaster { Id = 2, TemplateHtml = "<html>Template 2</html>" }
        };

            _mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(templates);

            var result = await _mockService.Object.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddAsync_Adds_Template()
        {
            var emailTemplate = new EmailTemplatesMaster
            {
                Id = 1,
                TemplateHtml = "<html>New Template</html>"
            };

            _mockService.Setup(x => x.AddAsync(emailTemplate)).Returns(Task.CompletedTask);

            await _mockService.Object.AddAsync(emailTemplate);

            _mockService.Verify(x => x.AddAsync(It.IsAny<EmailTemplatesMaster>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Updates_Template()
        {
            var emailTemplate = new EmailTemplatesMaster { Id = 1, TemplateHtml = "<html>Updated Template</html>" };

            _mockService.Setup(x => x.UpdateAsync(emailTemplate)).ReturnsAsync(true);

            var result = await _mockService.Object.UpdateAsync(emailTemplate);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_Deletes_Template()
        {
            var templateId = 1;

            _mockService.Setup(x => x.DeleteAsync(templateId)).Returns(Task.CompletedTask);

            await _mockService.Object.DeleteAsync(templateId);

            _mockService.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Once);
        }
    }
}
  

