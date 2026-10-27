using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowDelegationController : ControllerBase
    {
        private readonly IWorkflowDelegationService _delegationService;
        private readonly IUserContextService _userContext;

        public WorkflowDelegationController(
            IWorkflowDelegationService delegationService,
            IUserContextService userContext)
        {
            _delegationService = delegationService;
            _userContext = userContext;
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyDelegations()
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (currentUserId <= 0)
            {
                return Unauthorized(new ApiResponse<List<WorkflowDelegationDTO>>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Valid employee ID required.",
                    Data = null
                });
            }

            var delegations = await _delegationService.GetMyDelegationsAsync(currentUserId);
            return Ok(new ApiResponse<List<WorkflowDelegationDTO>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"{delegations.Count} delegation(s) found.",
                Data = delegations
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDelegation([FromBody] CreateWorkflowDelegationRequest request)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (currentUserId <= 0)
            {
                return Unauthorized(new ApiResponse<WorkflowDelegationDTO>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Valid employee ID required.",
                    Data = null
                });
            }

            try
            {
                var created = await _delegationService.CreateDelegationAsync(currentUserId, request);
                return Ok(new ApiResponse<WorkflowDelegationDTO>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Approval delegation created successfully.",
                    Data = created
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<WorkflowDelegationDTO>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating delegation for employee {EmployeeId}", currentUserId);
                return BadRequest(new ApiResponse<WorkflowDelegationDTO>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "An error occurred while creating delegation.",
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RevokeDelegation(long id)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (currentUserId <= 0)
            {
                return Unauthorized(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Valid employee ID required.",
                    Data = false
                });
            }

            bool revoked = await _delegationService.RevokeDelegationAsync(id, currentUserId);
            if (!revoked)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Delegation not found or not owned by you.",
                    Data = false
                });
            }

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Delegation revoked successfully.",
                Data = true
            });
        }
    }
}
