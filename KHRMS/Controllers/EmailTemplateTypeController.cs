using KHRMS.Infrastructure;
using KHRMS.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using KHRMS.Core.Models;

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
            var emailTemplateTypes = await _emailTemplateTypeService.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<EmailTemplateTypeMaster>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.AllEmailTemplateTypeFound,
                Data = emailTemplateTypes
            });
        }


        [HttpPost]
        [Route("AddEmailTemplateType")]
        public async Task<IActionResult> AddEmailTemplateType([FromBody] EmailTemplateTypeMaster emailTemplateType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmailTemplateType,
                    Data = false
                });
            }
            await _emailTemplateTypeService.AddAsync(emailTemplateType);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateTypeAdd,
                Data = true
            };
            return Ok(response);
        }

        [HttpPut]
        [Route("UpdateEmailTemplateType")]
        public async Task<IActionResult> UpdateEmailTemplateType(EmailTemplateTypeMaster emailTemplateType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidDataofEmailTemplateType,
                    Data = false
                });
            }
            await _emailTemplateTypeService.UpdateAsync(emailTemplateType);
            var response = new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateTypeUpdated,
                Data = true
            };
            return Ok(response);

        }

        [HttpDelete]
        [Route("DeleteEmailTemplateType/{id}")]
        public async Task<IActionResult> DeletEmailTemplateType(long id)
        {

            var emailTemplateType = await _emailTemplateTypeService.GetByIdAsync(id);
            if (emailTemplateType == null)
                return NotFound();

            await _emailTemplateTypeService.DeleteAsync(id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmailTemplateTypeDeleted,
                Data = true
            });

        }
    }
}
