using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using KHRMS.Core.Models;

namespace KHRMS
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplateController(IEmailTemplateService emailTemplateService) : ControllerBase

    {
        private readonly IEmailTemplateService _emailTemplateService = emailTemplateService;



        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var emailTemplates = await _emailTemplateService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<EmailTemplatesMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailTemplateFound,
                Data = emailTemplates
            });
        }


        [HttpGet]
        [Route("GetByTemplateTypeId/{TemplateTypeId}")]
        public async Task<IActionResult> GetByTemplateTypeId(long templateTypeid)
        {
            var emailTemplates = await _emailTemplateService.GetByEmailTemplateTypeIdAsync(templateTypeid);
            if (emailTemplates == null)
            {
                return NotFound(new ApiResponse<EmailTemplatesMaster>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.AllEmailTemplateNotFound,
                    Data = null
                });
            }
            var response = new ApiResponse<EmailTemplatesMaster>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailTemplateFound,
                Data = emailTemplates
            };

            return Ok(response);
        }


        [HttpPost]
        [Route("AddEmailTemplates")]
        public async Task<IActionResult> AddEmailTemplates([FromBody] EmailTemplatesMaster emailTemplatesMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmailTemplate,
                    Data = false
                });
            }
            await _emailTemplateService.AddAsync(emailTemplatesMaster);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateAdd,
                Data = true
            };
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateEmailTemplates")]
        public async Task<IActionResult> UpdateEmailTemplates(EmailTemplatesMaster emailTemplatesMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmailTemplate,
                    Data = false
                });
            }
            await _emailTemplateService.UpdateAsync(emailTemplatesMaster);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateUpdated,
                Data = true
            };
            return Ok(response);

        }

        //[HttpDelete]
        //[Route("DeleteemailTemplatesMaster/{id}")]
        //public async Task<IActionResult> DeleteemailTemplatesMaster(long id)
        //{

        //    var emailTemplates = await _emailTemplateService.GetByIdAsync(id);
        //    if (emailTemplates == null)
        //        return NotFound();

        //    await _emailTemplateService.DeleteAsync(id);
        //    return Ok(new ApiResponse<bool>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = ApiMessageConstant.EmailTemplateDeleted,
        //        Data = true
        //    });

        //}

        [HttpDelete]
        [Route("DeleteemailTemplatesMaster/{id}")]
        public async Task<IActionResult> DeleteemailTemplatesMaster(long id)
        {
            var emailTemplate = await _emailTemplateService.GetByIdAsync(id);
            if (emailTemplate == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Email Template not found",
                    Data = false
                });
            }

            await _emailTemplateService.DeleteAsync(id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Email Template deleted successfully",
                Data = true
            });
        }

        }
    }

}
