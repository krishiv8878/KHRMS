using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace KHRMS.UnitTest.ControllerTests
{
    public class UserRegistrationControllerTest
    {
        private readonly Mock<IUserRegistrationService> _mock;
        private readonly UserRegistrationController _controller;

        public UserRegistrationControllerTest()
        {
            _mock = new Mock<IUserRegistrationService>();
            _controller = new UserRegistrationController(_mock.Object);
        }

        [Fact]
        public void Registration_ShouldReturnPass_WhenValidDataIsProvided()
        {
            UserRegistration userregistration = new UserRegistration()
            {
                Id = 1,
                FirstName = "",
                LastName = "",
                Email = "",
                MobileNumber = "",
                Address = "",
                Password = ""
            };
            _mock.Setup(x => x.GetRegistrationByUser(It.IsAny<UserRegistration>())).ReturnsAsync(true);
            var result = _controller.Registration(userregistration);
            Assert.NotNull(result);
            _mock.Verify(x => x.GetRegistrationByUser(It.IsAny<UserRegistration>()), Times.Once());
        }

        [Fact]
        public async Task InviteEmployee_ShouldReturnOk_WhenServiceSucceeds()
        {
            var request = new InviteEmployeeRequest { Email = "newhire@example.com", FirstName = "John", LastName = "Doe" };
            var response = new InviteEmployeeResponse { Success = true, Message = "Success", InvitationToken = "sample-token", InvitationUrl = "http://example.com/invite" };
            _mock.Setup(x => x.InviteEmployeeAsync(It.IsAny<InviteEmployeeRequest>(), It.IsAny<long>())).ReturnsAsync(response);

            var result = await _controller.InviteEmployee(request) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task VerifyInvitation_ShouldReturnOk_WhenTokenIsValid()
        {
            var response = new VerifyInvitationResponse { IsValid = true, Email = "newhire@example.com" };
            _mock.Setup(x => x.VerifyInvitationTokenAsync("valid-token")).ReturnsAsync(response);

            var result = await _controller.VerifyInvitation("valid-token") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task AcceptInvitation_ShouldReturnOk_WhenAcceptanceSucceeds()
        {
            var request = new AcceptInvitationRequest { Token = "valid-token", Password = "StrongPassword123!" };
            _mock.Setup(x => x.AcceptInvitationAsync(It.IsAny<AcceptInvitationRequest>())).ReturnsAsync(true);

            var result = await _controller.AcceptInvitation(request) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }
    }
}
