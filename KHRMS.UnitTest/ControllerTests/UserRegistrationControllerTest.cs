using KHRMS.Controllers;
using KHRMS.Core;
using KHRMS.Services;
using Moq;


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
    }
}
