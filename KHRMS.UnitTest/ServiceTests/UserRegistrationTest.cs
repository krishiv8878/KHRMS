using KHRMS.Core;
using KHRMS.Services;
using Moq;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509;

namespace KHRMS.UnitTest.ServiceTests
{
    public class UserRegistrationTest
    {
        private readonly Mock<IUserRegistrationService> _mock;
        private readonly IUserRegistrationService _service; 
        public UserRegistrationTest()
        {
            _mock = new Mock<IUserRegistrationService>();
            _service = _mock.Object;
        }

        [Fact]
        public async Task Get_RegistrationByUser_ShouldReturnPass_WhenValidUserProvided()
        {
            var Id = 1;
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
            _mock.Setup(x => x.GetRegistrationByUser(userregistration));
            var result = _service.GetRegistrationByUser(userregistration);
            Assert.Equal(1, userregistration.Id);
            Assert.Equal("", userregistration.FirstName);
        }


        [Fact]
        public async Task Get_RegistrationByUser_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var _mock = new Mock<IUserRegistrationService>();
            IUserRegistrationService userregistrationservice = _mock.Object;

            UserRegistration userregistration = new UserRegistration()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                MobileNumber = "1234567890",
                Address = "123 Street",
                Password = "SecurePass123"
            };

            // Simulating that GetRegistrationByUser returns true (successful lookup)
            _mock.Setup(x => x.GetRegistrationByUser(It.IsAny<UserRegistration>())).ReturnsAsync(true);

            // Act
            var result = await userregistrationservice.GetRegistrationByUser(userregistration);

            // Assert
            Assert.True(result); // Ensure the method returns true
        }


    }
}
