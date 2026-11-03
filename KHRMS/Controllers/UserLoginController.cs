using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Net;

namespace KHRMS
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    [EnableRateLimiting("auth-policy")]
    public class UserLoginController(IUserLoginService userLoginService, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        public readonly IUserLoginService _userLoginService = userLoginService;
        public readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Handles user login using email and password.
        /// </summary>
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            Log.Information("UserLoginController - Login attempt for Email: {Email}", model.Email);

            try
            {
                var employeeId = await _userLoginService.GetUserLoginById(model.Email, model.Password);

                if (employeeId != null && employeeId.UserId.HasValue)
                {
                    // Store employee ID in session
                    _httpContextAccessor.HttpContext?.Session.SetString("EmployeeId", employeeId.UserId.Value.ToString());

                    Log.Information("UserLoginController - Login successful for EmployeeId: {EmployeeId}", employeeId.UserId.Value);

                    return Ok(new ApiResponse<UserLoginModel>
                    {
                        StatusCode = (int)HttpStatusCode.OK,
                        Message = ApiMessageConstant.UserLoginByIdAdded,
                        Data = employeeId,
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
            catch (InvalidOperationException ex)
            {
                Log.Warning("UserLoginController - Login blocked by lockout: {Message}", ex.Message);
                return StatusCode((int)HttpStatusCode.TooManyRequests, new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.TooManyRequests,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestModel model)
        {
            Log.Information("UserLoginController - Forgot Password attempt for Email: {Email}", model.Email);

            if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.ClientUrl))
                return BadRequest("Email and client URL are required.");

            var result = await _userLoginService.ForgotPasswordMail(model);

            if (!result)
            {
                Log.Warning("UserLoginController - Forgot Password for Email Not Found: {Email}", model.Email);
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmailNotfound,
                    Data = null
                });
            }
            Log.Information("UserLoginController - Password Forgot Link Sent Success for Email: {email}", model.Email);
            return Ok(new ApiResponse<string>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.sentPasswordResetMail,
                Data = null
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel userLogin)
        {
            Log.Information("UserLoginController - Reset Password attempt");
            if (userLogin == null || string.IsNullOrEmpty(userLogin.Password))
                return BadRequest("Email and new password are required.");

            try
            {
                var result = await _userLoginService.ResetPassword(userLogin);

                if (!result)
                {
                    Log.Warning("UserLoginController - Reset Password Is Failed");

                    return BadRequest(new ApiResponse<string>
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = ApiMessageConstant.resetPasswordFail,
                        Data = null
                    });
                }
                Log.Information("UserLoginController - Password Reset Success");
                return Ok(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.resetPasswordSuccess,
                    Data = null
                });
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning("UserLoginController - Reset Password token validation failed: {Message}", ex.Message);
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
        }
    }
}
