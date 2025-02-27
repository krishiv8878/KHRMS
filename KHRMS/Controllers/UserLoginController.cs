using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace KHRMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLoginController(IUserLoginService userLoginService) : ControllerBase
    {
        public readonly IUserLoginService _userLoginService = userLoginService;

        [HttpPost]
        [Route("Login")]

        public async Task<IActionResult> Login([FromBody] UserLogin model)
        {
            var isuserLoginAdded = await _userLoginService.GetUserLoginById(model.Email,model.Password);
            if (isuserLoginAdded)
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.UserLoginByIdAdded,
                    Data = isuserLoginAdded
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.UserLoginByIdNotAdded,
                    Data = isuserLoginAdded
                };
                return BadRequest(response);
            }
        }

       
    }
}
