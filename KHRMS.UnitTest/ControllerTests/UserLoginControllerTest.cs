using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ControllerTests
{
    public class UserLoginControllerTest
    {
        public UserLoginControllerTest()
        {
            
        }

        //[Fact]
        //public void LoginReturnPass()
        //{
        //    var Id = 1;
        //    var mock = new Mock<IUserLoginService>();
        //    var controller = new UserLoginController(mock.Object);
        //    //mock.Setup(x => x.GetUserLoginById());
        //    //UserLogin user = new UserLogin()
        //    //{
        //    //    Id = 1,
        //    //    UserId = 0,
        //    //    UserName = "",
        //    //    Password = "",
        //    //    Email = "",
        //    //    //LastLoginDate = new DateTime(0, 0, 0, 0, 0, 0, 0)
        //    //};

        //    // Arrange

        //    var model = new UserLogin
        //    {
        //        Email = "testuser@gmail.com",
        //        Password = "Test@123"
        //    };
        //    var result = await controller.Login(model);
        //    Assert.NotNull(result);
        //    Assert.Equal(1, 1);
        //}
        [Fact]
        public async Task LoginReturnPass()
        {
            // Arrange
            var mockService = new Mock<IUserLoginService>();

            // ✅ Use the correct namespace for `UserLogin`
            var model = new KHRMS.Services.Request.UserLogin
            {
                Email = "testuser@gmail.com",
                Password = "Test@123"
            };

            // ✅ Mock the service method to return `true`
            mockService.Setup(x => x.GetUserLoginById(model.Email, model.Password))
                       .ReturnsAsync(true); // Simulating a successful login

            var controller = new UserLoginController(mockService.Object);

            // Act
            var result = await controller.Login(model);

            // Assert
            Assert.NotNull(result);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }


    }
}
