using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS

{
    [Route("api/[controller]")]
    [ApiController]

    public class UserLoginController(IUserLoginService userLoginService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        public readonly IUserLoginService _userLoginService = userLoginService;

        public readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        //[HttpPost]
        //[Route("Login")]

        //public async Task<IActionResult> Login([FromBody] UserLogin model)
        //{
        //    var isuserLoginAdded = await _userLoginService.GetUserLoginById(model.Email,model.Password);
        //    if (isuserLoginAdded)
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.UserLoginByIdAdded,
        //            Data = isuserLoginAdded
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidCredentials,
        //            Data = isuserLoginAdded
        //        };
        //        return BadRequest(response);
        //    }
        //}

        //[HttpPost]
        //[Route("Login")]
        //public async Task<IActionResult> Login([FromBody] UserLogin model)
        //{
        //    var employeeId = await _userLoginService.GetUserLoginById(model.Email, model.Password);

        //    if (employeeId.HasValue)
        //    {
        //        HttpContext.Items["EmployeeId"] = employeeId.Value;

        //        var response = new ApiResponse<long>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.UserLoginByIdAdded,
        //            Data = employeeId.Value
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<string>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidCredentials,
        //            Data = null
        //        };
        //        return BadRequest(response);
        //    }
        //}
        //[HttpPost]
        //[Route("Login")]
        //public async Task<IActionResult> Login([FromBody] UserLogin model)
        //{
        //    var employeeId = await _userLoginService.GetUserLoginById(model.Email, model.Password);

        //    if (employeeId.HasValue)
        //    {
        //        // Store employee ID in HttpContext.Items
        //        HttpContext.Session.SetString("EmployeeId", employeeId.Value.ToString());

        //        var response = new ApiResponse<long>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.UserLoginByIdAdded,
        //            Data = employeeId.Value
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<string>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidCredentials,
        //            Data = null
        //        };
        //        return BadRequest(response);
        //    }
        //}


        /// <summary>
        /// Handles user login using email and password.
        /// </summary>
        [HttpPost("Login")]
        public async Task<IActionResult> Login( UserLogin model)
        {
            Log.Information("UserLoginController - Login attempt for Email: {Email}", model.Email);

            var employeeId = await _userLoginService.GetUserLoginById(model.Email, model.Password);

            if (employeeId.HasValue)
            {
                // Store employee ID in session
                _httpContextAccessor.HttpContext?.Session.SetString("EmployeeId", employeeId.Value.ToString());

                Log.Information("UserLoginController - Login successful for EmployeeId: {EmployeeId}", employeeId.Value);

                return Ok(new ApiResponse<long>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.UserLoginByIdAdded,
                    Data = employeeId.Value
                });
            }

            Log.Warning("UserLoginController - Login failed. Invalid credentials for Email: {Email}", model.Email);

            return BadRequest(new ApiResponse<string>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.InvalidCredentials,
                Data = null
            });
        }
    }
}
