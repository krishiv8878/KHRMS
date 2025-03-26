using KHRMS.Infrastructure;
using KHRMS.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using KHRMS.Core.Models;

namespace KHRMS
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmailController(IEmailService emailService) : ControllerBase

    {
        private readonly IEmailService _emailService = emailService;



        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var emails = await _emailService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<Email>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailsFound,
                Data = emails
            });
        }


        [HttpGet]
        [Route("GetByTemplateId/{TemplateId}")]
        public async Task<IActionResult> GetByTemplateId(long templateid)
        {
            var emailTemplates = await _emailService.GetByEmailTemplatesIdAsync(templateid);
            if (emailTemplates == null)
            {
                return NotFound(new ApiResponse<Email>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.AllEmailTemplateNotFound,
                    Data = null
                });
            }
            var response = new ApiResponse<Email>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailsFound,
                Data = emailTemplates
            };

            return Ok(response);
        }


        [HttpPost]
        [Route("AddEmails")]
        public async Task<IActionResult> AddEmails([FromBody] Email emails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmails,
                    Data = false
                });
            }
            await _emailService.AddAsync(emails);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailsAdd,
                Data = true
            };
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateEmails")]
        public async Task<IActionResult> UpdateEmails(Email emails)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmails,
                    Data = false
                });
            }
            await _emailService.UpdateAsync(emails);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailsUpdated,
                Data = true
            };
            return Ok(response);

        }

        [HttpDelete]
        [Route("Deleteemails/{id}")]
        public async Task<IActionResult> Deleteemails(long id)
        {

            var emails = await _emailService.GetByIdAsync(id);
            if (emails == null)
                return NotFound();

            await _emailService.DeleteAsync(id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailsDeleted,
                Data = true
            });

        }
    }

}
