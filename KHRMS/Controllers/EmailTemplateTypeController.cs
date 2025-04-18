using KHRMS.Infrastructure;
using KHRMS.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using KHRMS.Core.Models;
using Serilog;

namespace KHRMS
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplateTypeController(IEmailTemplateTypeMasterService emailTemplateTypeMasterService) : ControllerBase

    {
        private readonly IEmailTemplateTypeMasterService _emailTemplateTypeService = emailTemplateTypeMasterService;


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("GetAll EmailTemplateTypes API called.");
            var types = await _emailTemplateTypeService.GetAllAsync();

            if (types == null || !types.Any())
            {
                Log.Warning("No email template types found.");
                return Ok(new ApiResponse<IEnumerable<EmailTemplateTypeMaster>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AllEmailTemplateTypeNotFound,
                    Data = null
                });
            }

            Log.Information("Email template types retrieved successfully.");
            return Ok(new ApiResponse<IEnumerable<EmailTemplateTypeMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailTemplateTypeFound,
                Data = types
            });
        }


        [HttpPost("AddEmailTemplateType")]
        public async Task<IActionResult> AddEmailTemplateType([FromBody] EmailTemplateTypeMaster emailTemplateType)
        {
            Log.Information("AddEmailTemplateType API called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state while adding email template type.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmailTemplateType,
                    Data = false
                });
            }

            await _emailTemplateTypeService.AddAsync(emailTemplateType);

            Log.Information("Email template type added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateTypeAdd,
                Data = true
            });
        }


        [HttpPut("UpdateEmailTemplateType")]
        public async Task<IActionResult> UpdateEmailTemplateType([FromBody] EmailTemplateTypeMaster emailTemplateType)
        {
            Log.Information("UpdateEmailTemplateType API called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid model state while updating email template type.");
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.InvalidDataofEmailTemplateType,
                    Data = false
                });
            }

            await _emailTemplateTypeService.UpdateAsync(emailTemplateType);

            Log.Information("Email template type updated successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateTypeUpdated,
                Data = true
            });
        }



        [HttpDelete]
        [Route("DeleteEmailTemplateType")]
        public async Task<IActionResult> DeletEmailTemplateType(long id)
        {
            Log.Information("DeleteEmailTemplateType API called for ID: {Id}", id);

            var type = await _emailTemplateTypeService.GetByIdAsync(id);
            if (type == null)
            {
                Log.Warning("Email template type with ID: {Id} not found.", id);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.AllEmailTemplateTypeNotFound,
                    Data = false
                });
            }

            await _emailTemplateTypeService.DeleteAsync(id);
            Log.Information("Email template type with ID: {Id} deleted successfully.", id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateTypeDeleted,
                Data = true
            });
        }
    }
}
