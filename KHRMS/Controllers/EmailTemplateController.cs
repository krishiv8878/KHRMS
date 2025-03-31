using KHRMS.Core;
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
    public class EmailTemplateController(IEmailTemplateService emailTemplateService) : ControllerBase

    {
        private readonly IEmailTemplateService _emailTemplateService = emailTemplateService;



        //[HttpGet("GetAll")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var emailTemplates = await _emailTemplateService.GetAllAsync();
        //    return Ok(new ApiResponse<IEnumerable<EmailTemplatesMaster>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AllEmailTemplateFound,
        //        Data = emailTemplates
        //    });
        //}
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("GetAll EmailTemplates API called.");
            var emailTemplates = await _emailTemplateService.GetAllAsync();

            if (emailTemplates == null || !emailTemplates.Any())
            {
                Log.Warning("No email templates found.");
                return NotFound(new ApiResponse<IEnumerable<EmailTemplatesMaster>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.AllEmailTemplateNotFound,
                    Data = null
                });
            }

            Log.Information("Email templates retrieved successfully.");
            return Ok(new ApiResponse<IEnumerable<EmailTemplatesMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailTemplateFound,
                Data = emailTemplates
            });
        }

        //[HttpGet]
        //[Route("GetByTemplateTypeId/{TemplateTypeId}")]
        //public async Task<IActionResult> GetByTemplateTypeId(long templateTypeid)
        //{
        //    var emailTemplates = await _emailTemplateService.GetByEmailTemplateTypeIdAsync(templateTypeid);
        //    if (emailTemplates == null)
        //    {
        //        return NotFound(new ApiResponse<EmailTemplatesMaster>
        //        {
        //            StatusCode = (int)HttpStatusCode.NotFound,
        //            Message = ApiMessageConstant.AllEmailTemplateNotFound,
        //            Data = null
        //        });
        //    }
        //    var response = new ApiResponse<EmailTemplatesMaster>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AllEmailTemplateFound,
        //        Data = emailTemplates
        //    };

        //    return Ok(response);
        //}

        [HttpGet]
        [Route("GetByTemplateTypeId/{TemplateTypeId}")]
        public async Task<IActionResult> GetByTemplateTypeId(long templateTypeId)
        {
            Log.Information("GetByTemplateTypeId API called with TemplateTypeId: {TemplateTypeId}.", templateTypeId);
            var emailTemplate = await _emailTemplateService.GetByEmailTemplateTypeIdAsync(templateTypeId);

            if (emailTemplate == null)
            {
                Log.Warning("Email template not found for TemplateTypeId: {TemplateTypeId}.", templateTypeId);
                return NotFound(new ApiResponse<EmailTemplatesMaster>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.AllEmailTemplateNotFound,
                    Data = null
                });
            }

            Log.Information("Email template found for TemplateTypeId: {TemplateTypeId}.", templateTypeId);
            return Ok(new ApiResponse<EmailTemplatesMaster>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailTemplateFound,
                Data = emailTemplate
            });
        }

        //[HttpPost]
        //[Route("AddEmailTemplates")]
        //public async Task<IActionResult> AddEmailTemplates([FromBody] EmailTemplatesMaster emailTemplatesMaster)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidDataofEmailTemplate,
        //            Data = false
        //        });
        //    }
        //    await _emailTemplateService.AddAsync(emailTemplatesMaster);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailTemplateAdd,
        //        Data = true
        //    };
        //    return Ok(response);
        //}

        [HttpPost("AddEmailTemplates")]
        public async Task<IActionResult> AddEmailTemplates([FromBody] EmailTemplatesMaster emailTemplatesMaster)
        {
            Log.Information("AddEmailTemplates API called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid data provided for adding email template.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmailTemplate,
                    Data = false
                });
            }

            await _emailTemplateService.AddAsync(emailTemplatesMaster);
            Log.Information("Email template added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateAdd,
                Data = true
            });
        }
        //[HttpPut]
        //[Route("UpdateEmailTemplates")]
        //public async Task<IActionResult> UpdateEmailTemplates(EmailTemplatesMaster emailTemplatesMaster)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidDataofEmailTemplate,
        //            Data = false
        //        });
        //    }
        //    await _emailTemplateService.UpdateAsync(emailTemplatesMaster);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailTemplateUpdated,
        //        Data = true
        //    };
        //    return Ok(response);

        //}
        [HttpPut("UpdateEmailTemplates")]
        public async Task<IActionResult> UpdateEmailTemplates(EmailTemplatesMaster emailTemplatesMaster)
        {
            Log.Information("UpdateEmailTemplates API called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("Invalid data provided for updating email template.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmailTemplate,
                    Data = false
                });
            }

            await _emailTemplateService.UpdateAsync(emailTemplatesMaster);
            Log.Information("Email template updated successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateUpdated,
                Data = true
            });
        }


        //[HttpDelete]
        //[Route("DeleteemailTemplatesMaster/{id}")]
        //public async Task<IActionResult> DeleteemailTemplatesMaster(long id)
        //{
        //    var emailTemplate = await _emailTemplateService.GetByIdAsync(id);
        //    if (emailTemplate == null)
        //    {
        //        return NotFound(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.NotFound,
        //            Message = "Email Template not found",
        //            Data = false
        //        });
        //    }

        //    await _emailTemplateService.DeleteAsync(id);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = "Email Template deleted successfully",
        //        Data = true
        //    });
        //}
        [HttpDelete("DeleteemailTemplatesMaster/{id:long}")]
        public async Task<IActionResult> DeleteemailTemplatesMaster(long id)
        {
            Log.Information("DeleteEmailTemplatesMaster API called for ID: {Id}.", id);
            var emailTemplate = await _emailTemplateService.GetByIdAsync(id);

            if (emailTemplate == null)
            {
                Log.Warning("Email template not found for ID: {Id}.", id);
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.AllEmailTemplateNotFound,
                    Data = false
                });
            }

            await _emailTemplateService.DeleteAsync(id);
            Log.Information("Email template with ID: {Id} deleted successfully.", id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Email Template deleted successfully",
                Data = true
            });
        }

    }

}
