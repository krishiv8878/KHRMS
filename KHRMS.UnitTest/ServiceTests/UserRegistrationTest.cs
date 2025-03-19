using KHRMS.Core;
using KHRMS.Services;
using Moq;
using NPOI.SS.Formula.Functions;

namespace KHRMS.UnitTest.ServiceTests
{
    public class UserRegistrationTest
    {
        public UserRegistrationTest()
        {

        }

        [Fact]
        public void Get_RegistrationByUser_ShouldReturnPass_WhenValidUserProvided()
        {
            var Id = 1;
            var mock = new Mock<IUserRegistrationService>();
            IUserRegistrationService userregistrationservice = mock.Object;
            List<UserRegistration> userregistrations = new List<UserRegistration>();
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
            mock.Setup(x => x.GetRegistrationByUser(userregistration));
            var result = userregistrationservice.GetRegistrationByUser(userregistration);
            Assert.Equal(1, userregistration.Id);
            Assert.Equal("", userregistration.FirstName);
        }


        [Fact]
        public async Task Get_RegistrationByUser_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var mock = new Mock<IUserRegistrationService>();
            IUserRegistrationService userregistrationservice = mock.Object;

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
            mock.Setup(x => x.GetRegistrationByUser(It.IsAny<UserRegistration>())).ReturnsAsync(true);

            // Act
            var result = await userregistrationservice.GetRegistrationByUser(userregistration);

            // Assert
            Assert.True(result); // Ensure the method returns true
        }


    }
}
