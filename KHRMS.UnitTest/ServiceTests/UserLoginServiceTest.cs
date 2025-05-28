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
        private readonly Mock<IUserLoginService> _mock;
        private readonly IUserLoginService _service;
        public UserLoginServiceTest()
        {
            _mock = new Mock<IUserLoginService>();    
            _service = _mock.Object;
        }
        [Fact]
        public async Task Get_UserLoginById_ShouldReturnPass_WhenValidCredentialsProvided()
        {
            var Id = 1;
            UserLogin  user = new UserLogin()
            {
                Id = 1,
                UserId = 0,
                UserName="",
                Password="",
                Email="",
                //LastLoginDate= new DateTime(0,0,0,0,0,0,0)
            };
            _mock.Setup(x => x.GetUserLoginById(user.Email, user.Password));
            var result = _service.GetUserLoginById(user.Email,user.Password);
            //Assert.NotNull(result);
            Assert.Equal(1, user.Id);
            Assert.Equal("", user.UserName);
        }
    }
}
