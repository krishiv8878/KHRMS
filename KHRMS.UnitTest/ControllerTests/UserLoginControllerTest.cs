
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KHRMS.UnitTest.ControllerTests
{
    public class UserLoginControllerTest
    {
        public UserLoginControllerTest()
        {

        }


        //[Fact]
        //public async Task Login_ShouldReturnOk_WhenCredentialsAreValid()
        //{
        //    // Arrange
        //    var mockService = new Mock<IUserLoginService>();

        //    // ✅ Use the correct namespace for `UserLogin`
        //    var model = new KHRMS.Services.Request.UserLogin
        //    {
        //        Email = "testuser@gmail.com",
        //        Password = "Test@123"
        //    };

        //    // ✅ Mock the service method to return `true`
        //    mockService.Setup(x => x.GetUserLoginById(model.Email, model.Password))
        //               .ReturnsAsync(true); // Simulating a successful login

        //    var controller = new UserLoginController(mockService.Object);

        //    // Act
        //    var result = await controller.Login(model);

        //    // Assert
        //    Assert.NotNull(result);
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    Assert.Equal(200, okResult.StatusCode);
        //}


    }
}
