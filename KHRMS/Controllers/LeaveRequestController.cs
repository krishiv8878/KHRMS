using KHRMS.Core;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;


namespace KHRMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveRequestController(ILeaveRequestTypeService leaveRequestTypeService) : ControllerBase
    {
        public readonly ILeaveRequestTypeService _leaveRequestTypeService = leaveRequestTypeService;


        [HttpGet]
        [Route("GetAllLeaveRequest")]

        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetAll()
        {
            var result = await _leaveRequestTypeService.GetAllLeaveRequestType();
            return Ok(result);
        }

        //[HttpGet("{id:int}")]
        [HttpGet]

        [Route("GetLeaveRequestById/{id}")]
        public async Task<ActionResult<LeaveRequest>> GetById(int id)
        {
            var result = await _leaveRequestTypeService.GetLeaveRequestTypeById(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Route("AddLeaveRequest")]

        public async Task<IActionResult> Create([FromBody] LeaveRequest leaveRequest)
        {
            if (leaveRequest == null)
                return BadRequest("Invalid data.");

            await _leaveRequestTypeService.AddLeaveRequestType(leaveRequest);
            return CreatedAtAction(nameof(GetById), new { id = leaveRequest.Id }, leaveRequest);
        }

        [HttpPut]

        [Route("UpdateLeaveRequest/{id}")]

        public async Task<IActionResult> Update(long id, [FromBody] LeaveRequest leaveRequest)
        {
            if (id != leaveRequest.Id)
                return BadRequest("ID mismatch.");

            await _leaveRequestTypeService.UpdateLeaveRequestType(leaveRequest);
            return NoContent();
        }

        [HttpDelete]
        [Route("DeleteLeaveRequest/{id}")]

        public async Task<IActionResult> Delete(long id)
        {
            await _leaveRequestTypeService.DeleteLeaveRequestType(id);
            return NoContent();
        }
    }
}

   