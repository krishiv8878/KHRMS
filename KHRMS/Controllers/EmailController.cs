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
    public class EmailController(IEmailService emailService) : ControllerBase

    {
        private readonly IEmailService _emailService = emailService;



        //[HttpGet("GetAll")]
        //public async Task<IActionResult> GetAll()
        //{
        //    var emails = await _emailService.GetAllAsync();
        //    return Ok(new ApiResponse<IEnumerable<Email>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AllEmailsFound,
        //        Data = emails
        //    });
        //}

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("GetAll Emails API called.");
            var emails = await _emailService.GetAllAsync();

            if (emails == null || !emails.Any())
            {
                Log.Information("No emails found.");
                return NotFound(new ApiResponse<IEnumerable<Email>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.EmailNotFound,
                    Data = null
                });
            }

            Log.Information("Emails retrieved successfully.");
            return Ok(new ApiResponse<IEnumerable<Email>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailsFound,
                Data = emails
            });
        }

        //[HttpGet]
        //[Route("GetByTemplateId/{TemplateId}")]
        //public async Task<IActionResult> GetByTemplateId(long templateid)
        //{
        //    var emailTemplates = await _emailService.GetByEmailTemplatesIdAsync(templateid);
        //    if (emailTemplates == null)
        //    {
        //        return NotFound(new ApiResponse<Email>
        //        {
        //            StatusCode = (int)HttpStatusCode.NotFound,
        //            Message = ApiMessageConstant.AllEmailTemplateNotFound,
        //            Data = null
        //        });
        //    }
        //    var response = new ApiResponse<Email>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.AllEmailsFound,
        //        Data = emailTemplates
        //    };

        //    return Ok(response);
        //}

        [HttpGet]
        [Route("GetByTemplateId/{TemplateId}")]
        public async Task<IActionResult> GetByTemplateId(long templateId)
        {
            Log.Information("GetByTemplateId API called for TemplateId {TemplateId}.", templateId);
            var emailTemplate = await _emailService.GetByEmailTemplatesIdAsync(templateId);

            if (emailTemplate == null)
            {
                Log.Warning("No email template found for TemplateId {TemplateId}.", templateId);
                return NotFound(new ApiResponse<Email>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.AllEmailTemplateNotFound,
                    Data = null
                });
            }

            Log.Information("Email template retrieved successfully for TemplateId {TemplateId}.", templateId);
            return Ok(new ApiResponse<Email>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailsFound,
                Data = emailTemplate
            });
        }

        //[HttpPost]
        //[Route("AddEmails")]
        //public async Task<IActionResult> AddEmails([FromBody] Email emails)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidDataofEmails,
        //            Data = false
        //        });
        //    }
        //    await _emailService.AddAsync(emails);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailsAdd,
        //        Data = true
        //    };
        //    return Ok(response);
        //}

        [HttpPost("AddEmails")]
        public async Task<IActionResult> AddEmails([FromBody] Email email)
        {
            Log.Information("AddEmails API called.");
            await _emailService.AddAsync(email);

            Log.Information("Email added successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailsAdd,
                Data = true
            });
        }

        //[HttpPut]
        //[Route("UpdateEmails")]
        //public async Task<IActionResult> UpdateEmails(Email emails)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidDataofEmails,
        //            Data = false
        //        });
        //    }
        //    await _emailService.UpdateAsync(emails);
        //    var response = new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailsUpdated,
        //        Data = true
        //    };
        //    return Ok(response);

        //}
        [HttpPut("UpdateEmails")]
        public async Task<IActionResult> UpdateEmails([FromBody] Email email)
                {
            Log.Information("UpdateEmails API called.");
            await _emailService.UpdateAsync(email);

            Log.Information("Email updated successfully.");
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailsUpdated,
                Data = true
            });
        }


        //[HttpDelete]
        //[Route("Deleteemails/{id}")]
        //public async Task<IActionResult> Deleteemails(long id)
        //{

        //    var emails = await _emailService.GetByIdAsync(id);
        //    if (emails == null)
        //        return NotFound();

        //    await _emailService.DeleteAsync(id);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailsDeleted,
        //        Data = true
        //    });

        //}
        [HttpDelete("Deleteemails/{id:long}")]
        public async Task<IActionResult> Deleteemails(long id)
        {
            Log.Information("DeleteEmails API called for ID {Id}.", id);
            var existingEmail = await _emailService.GetByIdAsync(id);

            if (existingEmail == null)
            {
                Log.Warning("Email with ID {Id} not found.", id);
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.EmailNotFound,
                    Data = false
                });
            }

            await _emailService.DeleteAsync(id);
            Log.Information("Email with ID {Id} deleted successfully.", id);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailsDeleted,
                Data = true
            });

        }
    }

}
