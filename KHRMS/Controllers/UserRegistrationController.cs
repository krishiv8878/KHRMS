using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistrationController(IUserRegistrationService userRegistrationService) : ControllerBase
    {
        public readonly IUserRegistrationService _userRegistrationService = userRegistrationService;


        /// <summary>
        /// Registers a new user.
        /// </summary>
        [HttpPost("Registration")]
        public async Task<IActionResult> Registration(UserRegistration userRegistration)
        {
            Log.Information("UserRegistrationController - Registration attempt for user: {Email}", userRegistration.Email);

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

    }
}
