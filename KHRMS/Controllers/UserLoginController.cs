using KHRMS.Core;
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
     
        /// <summary>
        /// Handles user login using email and password.
        /// </summary>
            [HttpPost("Login")]
            public async Task<IActionResult> Login( UserLoginModel model)
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
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.ClientUrl))
                return BadRequest("Email and client URL are required.");

            var result = await _userLoginService.ForgotPasswordMail(model);

            if (!result)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Email not found.",
                    Data = null
                });
            }
            return Ok(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Password reset email sent if the email exists.",
                    Data = null
                });
            
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserLoginModel userLogin)
        {
            if (userLogin == null || string.IsNullOrEmpty(userLogin.Email) || string.IsNullOrEmpty(userLogin.Password))
                return BadRequest("Email and new password are required.");

            var result = await _userLoginService.ResetPassword(userLogin);

            if (!result)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Failed to reset password.",
                    Data = null
                });
            }
            return Ok(new ApiResponse<string>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Password reset successfully.",
                Data = null
            });
        }
    }
}
