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



        //[HttpGet("GetAll")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var emailTemplateTypes = await _emailTemplateTypeService.GetAllAsync();
        //    return Ok(new ApiResponse<IEnumerable<EmailTemplateTypeMaster>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AllEmailTemplateTypeFound,
        //        Data = emailTemplateTypes
        //    });
        //}
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

        //[HttpPost]
        //[Route("AddEmailTemplateType")]
        //public async Task<IActionResult> AddEmailTemplateType([FromBody] EmailTemplateTypeMaster emailTemplateType)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidDataofEmailTemplateType,
        //            Data = false
        //        });
        //    }
        //    await _emailTemplateTypeService.AddAsync(emailTemplateType);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailTemplateTypeAdd,
        //        Data = true
        //    };
        //    return Ok(response);
        //}
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

        //[HttpPut]
        //[Route("UpdateEmailTemplateType")]
        //public async Task<IActionResult> UpdateEmailTemplateType(EmailTemplateTypeMaster emailTemplateType)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidDataofEmailTemplateType,
        //            Data = false
        //        });
        //    }
        //    await _emailTemplateTypeService.UpdateAsync(emailTemplateType);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailTemplateTypeUpdated,
        //        Data = true
        //    };
        //    return Ok(response);

        //}
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

        //[HttpDelete]
        //[Route("DeleteEmailTemplateType/{id}")]
        //public async Task<IActionResult> DeletEmailTemplateType(long id)
        //{

        //    var emailTemplateType = await _emailTemplateTypeService.GetByIdAsync(id);
        //    if (emailTemplateType == null)
        //        return NotFound();

        //    await _emailTemplateTypeService.DeleteAsync(id);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailTemplateTypeDeleted,
        //        Data = true
        //    });

        //}

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
