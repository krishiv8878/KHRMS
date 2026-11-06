using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using System.Net;
using System.Security.Claims;

namespace KHRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("auth-policy")]
    public class UserRegistrationController(IUserRegistrationService userRegistrationService) : ControllerBase
    {
        public readonly IUserRegistrationService _userRegistrationService = userRegistrationService;

        /// <summary>
        /// Registers a new user directly. Restricted to Admin and HR staff.
        /// Anonymous public registration is strictly disabled.
        /// </summary>
        [HttpPost("Registration")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> Registration(UserRegistration userRegistration)
        {
            Log.Information("UserRegistrationController - Direct registration attempt for user: {Email} by {Caller}", 
                userRegistration.Email, User.Identity?.Name);

            var isRegistrationUserAdded = await _userRegistrationService.GetRegistrationByUser(userRegistration);

            if (isRegistrationUserAdded)
            {
                Log.Information("UserRegistrationController - Registration successful for user: {Email}", userRegistration.Email);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.RegistrationUserAdded,
                    Data = true
                });
            }

            Log.Warning("UserRegistrationController - Registration failed for user: {Email}", userRegistration.Email);

            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.RegistrationUserNotAdded,
                Data = false
            });
        }

        /// <summary>
        /// Generates a secure single-use invitation link for an employee to complete their registration.
        /// Restricted to Admin and HR staff.
        /// </summary>
        [HttpPost("InviteEmployee")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> InviteEmployee([FromBody] InviteEmployeeRequest request)
        {
            var currentEmpIdStr = User.FindFirst("UserId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            long.TryParse(currentEmpIdStr, out long creatorEmpId);

            Log.Information("UserRegistrationController - InviteEmployee requested for {Email} by User #{CreatorId}", 
                request.Email, creatorEmpId);

            var result = await _userRegistrationService.InviteEmployeeAsync(request, creatorEmpId);
            if (!result.Success)
            {
                return BadRequest(new ApiResponse<InviteEmployeeResponse>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = result.Message,
                    Data = result
                });
            }

            return Ok(new ApiResponse<InviteEmployeeResponse>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = result.Message,
                Data = result
            });
        }

        /// <summary>
        /// Verifies whether an employee onboarding invitation token is valid, unexpired, and unused.
        /// Open to unauthenticated clients who possess a valid cryptographic token.
        /// </summary>
        [HttpGet("VerifyInvitation")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyInvitation([FromQuery] string token)
        {
            var result = await _userRegistrationService.VerifyInvitationTokenAsync(token);
            if (!result.IsValid)
            {
                return BadRequest(new ApiResponse<VerifyInvitationResponse>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = result.Message,
                    Data = result
                });
            }

            return Ok(new ApiResponse<VerifyInvitationResponse>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = result.Message,
                Data = result
            });
        }

        /// <summary>
        /// Completes employee account registration using a validated single-use cryptographic invitation token.
        /// Open to unauthenticated clients who possess a valid cryptographic token.
        /// </summary>
        [HttpPost("AcceptInvitation")]
        [AllowAnonymous]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
        {
            var success = await _userRegistrationService.AcceptInvitationAsync(request);
            if (success)
            {
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Account registration completed successfully. You may now log in with your credentials.",
                    Data = true
                });
            }

            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Failed to complete registration. The invitation link may be invalid, expired, or already used.",
                Data = false
            });
        }
    }
}
