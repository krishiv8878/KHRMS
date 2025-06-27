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
            Log.Information("UserLoginController - Password Forgot Link Sent Succeess for Email: {email}", model.Email);
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
            Log.Information("UserLoginController - Reset Password attempt Failed");
            if (userLogin == null || string.IsNullOrEmpty(userLogin.Password))
                return BadRequest("Email and new password are required.");

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
    }
}
