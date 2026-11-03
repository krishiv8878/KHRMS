using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Authorization;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChecklistController : ControllerBase
    {
        private readonly IChecklistService _checklistService;
        private readonly IUserContextService _userContextService;

        public ChecklistController(IChecklistService checklistService, IUserContextService userContextService)
        {
            _checklistService = checklistService;
            _userContextService = userContextService;
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates([FromQuery] string? checklistType, [FromQuery] string? department)
        {
            Log.Information("GetTemplates API called: type={Type}, dept={Dept}", checklistType, department);
            var templates = await _checklistService.GetTemplatesAsync(checklistType, department);
            return Ok(new ApiResponse<IEnumerable<ChecklistTemplateDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Checklist templates retrieved successfully",
                Data = templates
            });
        }

        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplateById(long id)
        {
            var template = await _checklistService.GetTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound(new ApiResponse<ChecklistTemplateDto>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = $"Template #{id} not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<ChecklistTemplateDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Checklist template retrieved successfully",
                Data = template
            });
        }

        [HttpPost("templates")]
        public async Task<IActionResult> SaveTemplate([FromBody] ChecklistTemplateDto dto)
        {
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            if (!isAdminOrHr)
            {
                return Forbid();
            }

            var result = await _checklistService.SaveTemplateAsync(dto);
            return Ok(new ApiResponse<ChecklistTemplateDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Checklist template saved successfully",
                Data = result
            });
        }

        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeleteTemplate(long id)
        {
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            if (!isAdminOrHr)
            {
                return Forbid();
            }

            var deleted = await _checklistService.DeleteTemplateAsync(id);
            return Ok(new ApiResponse<bool>
            {
                StatusCode = deleted ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                Message = deleted ? "Template deleted successfully" : "Failed to delete template",
                Data = deleted
            });
        }

        [HttpPost("instantiate")]
        public async Task<IActionResult> InstantiateChecklist([FromQuery] long employeeId, [FromQuery] string checklistType, [FromQuery] long? templateId)
        {
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();
            if (!isAdminOrHr)
            {
                return Forbid();
            }

            var checklist = await _checklistService.InstantiateChecklistForEmployeeAsync(employeeId, checklistType, templateId);
            return Ok(new ApiResponse<EmployeeChecklistDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"{checklistType} checklist instantiated successfully",
                Data = checklist
            });
        }

        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeChecklists([FromQuery] long? employeeId, [FromQuery] string? checklistType)
        {
            var currentUserId = _userContextService.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContextService.IsAdmin() || _userContextService.IsHR();

            long? targetEmpId = employeeId;
            if (!isAdminOrHr && (!targetEmpId.HasValue || targetEmpId.Value <= 0))
            {
                targetEmpId = currentUserId;
            }

            var checklists = await _checklistService.GetEmployeeChecklistsAsync(targetEmpId, checklistType);
            return Ok(new ApiResponse<IEnumerable<EmployeeChecklistDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Employee checklists retrieved successfully",
                Data = checklists
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChecklistById(long id)
        {
            var checklist = await _checklistService.GetChecklistByIdAsync(id);
            if (checklist == null)
            {
                return NotFound(new ApiResponse<EmployeeChecklistDto>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = $"Checklist #{id} not found",
                    Data = null
                });
            }

            return Ok(new ApiResponse<EmployeeChecklistDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Checklist details retrieved successfully",
                Data = checklist
            });
        }

        [HttpPut("task/{taskId}/status")]
        public async Task<IActionResult> UpdateTaskStatus(long taskId, [FromBody] ChecklistTaskUpdateDto dto)
        {
            try
            {
                var updated = await _checklistService.UpdateTaskStatusAsync(taskId, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = updated ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = updated ? "Checklist task status updated successfully" : "Failed to update task status",
                    Data = updated
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        [HttpPut("task/{taskId}/assignee")]
        public async Task<IActionResult> UpdateTaskAssignee(long taskId, [FromBody] TaskAssigneeUpdateDto dto)
        {
            try
            {
                var updated = await _checklistService.UpdateTaskAssigneeAsync(taskId, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = updated ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = updated ? "Checklist task assignee updated successfully" : "Failed to update task assignee",
                    Data = updated
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        [HttpGet("asset-clearance/{employeeId}")]
        public async Task<IActionResult> VerifyAssetClearance(long employeeId)
        {
            var result = await _checklistService.VerifyAssetReturnsAsync(employeeId);
            return Ok(new ApiResponse<AssetReturnVerificationDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Asset return status verified successfully",
                Data = result
            });
        }

        [HttpGet("pending-tasks")]
        public async Task<IActionResult> GetPendingTasks()
        {
            var tasks = await _checklistService.GetPendingTasksForUserAsync();
            return Ok(new ApiResponse<IEnumerable<ChecklistTaskDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Pending checklist tasks retrieved successfully",
                Data = tasks
            });
        }
    }
}
