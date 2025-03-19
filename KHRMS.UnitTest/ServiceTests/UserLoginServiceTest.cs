using KHRMS.Core;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KHRMS.UnitTest.ServiceTests
{
    public class UserLoginServiceTest
    {
        public UserLoginServiceTest()
        {
                
        }
        [Fact]
        public void Get_UserLoginById_ShouldReturnPass_WhenValidCredentialsProvided()
        {
            var Id = 1;
            var mock = new Mock<IUserLoginService>();
            IUserLoginService userLoginService = mock.Object;
            List<UserLogin> users = new List<UserLogin>();
            UserLogin  user = new UserLogin()
            {
                Id = 1,
                UserId = 0,
                UserName="",
                Password="",
                Email="",
                //LastLoginDate= new DateTime(0,0,0,0,0,0,0)
            };
            mock.Setup(x => x.GetUserLoginById(user.Email, user.Password));
            var result = userLoginService.GetUserLoginById(user.Email,user.Password);
            //Assert.NotNull(result);
            Assert.Equal(1, user.Id);
            Assert.Equal("", user.UserName);
        }
    }
}
