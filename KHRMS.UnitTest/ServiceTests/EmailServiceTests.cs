using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services;
using Moq;

namespace KHRMS.UnitTest.ServiceTests
{

    public class EmailServiceTests
    {
        private readonly Mock<IEmailService> _mockService;

        public EmailServiceTests()
        {
            _mockService = new Mock<IEmailService>();
        }

        [Fact]
        public async Task GetByEmailTemplatesIdAsync_Returns_Email()
        {
            var templateId = 1;
            var email = new Email
            {
                Id = 1,
                EmailTemplateId = templateId,
                EmailSubject = "Test Subject",
                EmailBody = "Test Body",
                FromEmail = "test@domain.com",
                ToEmail = "receiver@domain.com"
            };

            _mockService.Setup(x => x.GetByEmailTemplatesIdAsync(templateId))
                        .ReturnsAsync(email);

            var result = await _mockService.Object.GetByEmailTemplatesIdAsync(templateId);

            Assert.NotNull(result);
            Assert.Equal(templateId, result.EmailTemplateId);
        }

        [Fact]
        public async Task GetAllAsync_Returns_ListOfEmails()
        {
            var emails = new List<Email>
            {
                new Email { Id = 1, EmailSubject = "Email 1" },
                new Email { Id = 2, EmailSubject = "Email 2" }
            };

            _mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(emails);

            var result = await _mockService.Object.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddAsync_Adds_Email()
        {
            var email = new Email
            {
                Id = 1,
                EmailSubject = "New Email",
                EmailBody = "Test Body"
            };

            _mockService.Setup(x => x.AddAsync(email)).Returns(Task.CompletedTask);

            await _mockService.Object.AddAsync(email);

            _mockService.Verify(x => x.AddAsync(It.IsAny<Email>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Updates_Email()
        {
            var email = new Email { Id = 1, EmailSubject = "Updated Subject" };

            _mockService.Setup(x => x.UpdateAsync(email)).ReturnsAsync(true);

            var result = await _mockService.Object.UpdateAsync(email);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_Deletes_Email()
        {
            var emailId = 1;

            _mockService.Setup(x => x.DeleteAsync(emailId)).Returns(Task.CompletedTask);

            await _mockService.Object.DeleteAsync(emailId);

            _mockService.Verify(x => x.DeleteAsync(It.IsAny<long>()), Times.Once);
        }
    }
}