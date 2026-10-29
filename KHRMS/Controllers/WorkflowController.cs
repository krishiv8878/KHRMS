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
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowEngineService _workflowEngine;
        private readonly IWorkflowAdminService _adminService;
        private readonly IUserContextService _userContext;
        private readonly IPermissionService _permissionService;

        public WorkflowController(
            IWorkflowEngineService workflowEngine,
            IWorkflowAdminService adminService,
            IUserContextService userContext,
            IPermissionService permissionService)
        {
            _workflowEngine = workflowEngine;
            _adminService = adminService;
            _userContext = userContext;
            _permissionService = permissionService;
        }

        private async Task<bool> HasConfigurePermissionAsync(long currentUserId)
        {
            if (_userContext.IsAdmin()) return true;
            return await _permissionService.HasPermissionAsync(currentUserId, "WORKFLOW_CONFIGURE");
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetPendingInbox()
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (currentUserId <= 0)
            {
                return Unauthorized(new ApiResponse<List<WorkflowInboxItemDTO>>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Valid employee ID required.",
                    Data = null
                });
            }

            var inbox = await _workflowEngine.GetPendingInboxAsync(currentUserId);
            return Ok(new ApiResponse<List<WorkflowInboxItemDTO>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"{inbox.Count} pending approval(s) found in your inbox.",
                Data = inbox
            });
        }

        [HttpGet("in-scope")]
        public async Task<IActionResult> GetRequestsInScope([FromQuery] string? requestType, [FromQuery] string? status)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (currentUserId <= 0)
            {
                return Unauthorized(new ApiResponse<List<WorkflowScopedRequestDTO>>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Valid employee ID required.",
                    Data = null
                });
            }

            var requests = await _workflowEngine.GetRequestsInScopeAsync(currentUserId, requestType, status);
            return Ok(new ApiResponse<List<WorkflowScopedRequestDTO>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"{requests.Count} request(s) found within your permission scope.",
                Data = requests
            });
        }

        [HttpGet("instance/{id}")]
        public async Task<IActionResult> GetInstanceDetails(long id)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (currentUserId <= 0)
            {
                return Unauthorized(new ApiResponse<WorkflowInstanceDetailDTO>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Valid employee ID required.",
                    Data = null
                });
            }

            var details = await _workflowEngine.GetInstanceDetailsAsync(id, currentUserId);
            if (details == null)
            {
                return NotFound(new ApiResponse<WorkflowInstanceDetailDTO>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = $"Workflow instance #{id} not found or you lack permission to view it.",
                    Data = null
                });
            }

            return Ok(new ApiResponse<WorkflowInstanceDetailDTO>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Workflow instance details retrieved successfully.",
                Data = details
            });
        }

        [HttpPost("instance/{id}/action")]
        public async Task<IActionResult> ActOnStep(long id, [FromBody] WorkflowActionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Action))
            {
                return BadRequest(new ApiResponse<WorkflowActionResult>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "A valid action ('Approve', 'Reject', 'OverrideApprove', 'OverrideReject', or 'Reassign') is required.",
                    Data = null
                });
            }

            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (currentUserId <= 0)
            {
                return Unauthorized(new ApiResponse<WorkflowActionResult>
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Valid employee ID required.",
                    Data = null
                });
            }

            var result = await _workflowEngine.ActOnStepAsync(id, currentUserId, request);
            if (!result.Success)
            {
                if (result.StatusCode == "AlreadyActioned")
                {
                    return Conflict(new ApiResponse<WorkflowActionResult>
                    {
                        StatusCode = (int)HttpStatusCode.Conflict,
                        Message = result.Message,
                        Data = result
                    });
                }
                if (result.StatusCode == "Forbidden")
                {
                    return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<WorkflowActionResult>
                    {
                        StatusCode = (int)HttpStatusCode.Forbidden,
                        Message = result.Message,
                        Data = result
                    });
                }
                if (result.StatusCode == "NotFound")
                {
                    return NotFound(new ApiResponse<WorkflowActionResult>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = result.Message,
                        Data = result
                    });
                }

                return BadRequest(new ApiResponse<WorkflowActionResult>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = result.Message,
                    Data = result
                });
            }

            return Ok(new ApiResponse<WorkflowActionResult>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = result.Message,
                Data = result
            });
        }

        [HttpPost("instance/{id}/cancel")]
        public async Task<IActionResult> CancelWorkflow(long id, [FromBody] WorkflowCancelRequest request)
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

            try
            {
                bool success = await _workflowEngine.CancelWorkflowAsync(id, currentUserId, request?.Reason ?? "Cancelled by user");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Workflow request cancelled successfully.",
                    Data = success
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = ex.Message,
                    Data = false
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

        // ==============================================================
        // ADMIN CONFIGURATION ENDPOINTS (Permission: WORKFLOW_CONFIGURE)
        // ==============================================================

        [HttpGet("definitions")]
        public async Task<IActionResult> GetDefinitions()
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (!await HasConfigurePermissionAsync(currentUserId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<List<WorkflowDefinitionSummaryDTO>>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Missing required permission 'WORKFLOW_CONFIGURE'.",
                    Data = null
                });
            }

            var summaries = await _adminService.GetDefinitionSummariesAsync();
            return Ok(new ApiResponse<List<WorkflowDefinitionSummaryDTO>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Workflow definitions retrieved successfully.",
                Data = summaries
            });
        }

        [HttpGet("definitions/{requestType}")]
        public async Task<IActionResult> GetDefinitionDetail(string requestType, [FromQuery] int? version)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (!await HasConfigurePermissionAsync(currentUserId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<WorkflowDefinitionDetailDTO>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Missing required permission 'WORKFLOW_CONFIGURE'.",
                    Data = null
                });
            }

            var detail = await _adminService.GetDefinitionDetailAsync(requestType, version);
            if (detail == null)
            {
                return NotFound(new ApiResponse<WorkflowDefinitionDetailDTO>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = $"Workflow definition for '{requestType}' not found.",
                    Data = null
                });
            }

            return Ok(new ApiResponse<WorkflowDefinitionDetailDTO>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"Workflow definition details for '{requestType}' retrieved successfully.",
                Data = detail
            });
        }

        [HttpPost("definitions/validate")]
        public async Task<IActionResult> ValidateDefinition([FromBody] WorkflowDefinitionSaveDTO dto)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (!await HasConfigurePermissionAsync(currentUserId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<WorkflowValidationResultDTO>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Missing required permission 'WORKFLOW_CONFIGURE'.",
                    Data = null
                });
            }

            var validation = await _adminService.ValidateDefinitionAsync(dto);
            return Ok(new ApiResponse<WorkflowValidationResultDTO>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = validation.IsValid ? "Workflow validation passed successfully." : "Workflow validation reported issues.",
                Data = validation
            });
        }

        [HttpPost("definitions")]
        public async Task<IActionResult> SaveDefinition([FromBody] WorkflowDefinitionSaveDTO dto)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (!await HasConfigurePermissionAsync(currentUserId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<WorkflowDefinitionDetailDTO>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Missing required permission 'WORKFLOW_CONFIGURE'.",
                    Data = null
                });
            }

            try
            {
                var saved = await _adminService.SaveAndPublishDefinitionAsync(dto, currentUserId);
                return Ok(new ApiResponse<WorkflowDefinitionDetailDTO>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Workflow definition for '{dto.RequestType}' published as version {saved.Definition.Version}.",
                    Data = saved
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<WorkflowDefinitionDetailDTO>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving workflow definition for {RequestType}", dto.RequestType);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<WorkflowDefinitionDetailDTO>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Failed to publish workflow definition.",
                    Data = null
                });
            }
        }

        [HttpPost("test-routing")]
        public async Task<IActionResult> TestRouting([FromBody] TestRoutingRequestDTO dto)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (!await HasConfigurePermissionAsync(currentUserId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<TestRoutingResponseDTO>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Missing required permission 'WORKFLOW_CONFIGURE'.",
                    Data = null
                });
            }

            var response = await _adminService.TestRoutingAsync(dto);
            return Ok(new ApiResponse<TestRoutingResponseDTO>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = response.Message,
                Data = response
            });
        }

        [HttpGet("definitions/{requestType}/history")]
        public async Task<IActionResult> GetVersionHistory(string requestType)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (!await HasConfigurePermissionAsync(currentUserId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<List<WorkflowVersionHistoryDTO>>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Missing required permission 'WORKFLOW_CONFIGURE'.",
                    Data = null
                });
            }

            var history = await _adminService.GetVersionHistoryAsync(requestType);
            return Ok(new ApiResponse<List<WorkflowVersionHistoryDTO>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"Found {history.Count} version(s) for '{requestType}'.",
                Data = history
            });
        }

        [HttpPost("definitions/{requestType}/rollback/{version}")]
        public async Task<IActionResult> RollbackVersion(string requestType, int version)
        {
            long currentUserId = _userContext.GetCurrentEmployeeId();
            if (!await HasConfigurePermissionAsync(currentUserId))
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = "Missing required permission 'WORKFLOW_CONFIGURE'.",
                    Data = false
                });
            }

            bool success = await _adminService.RollbackVersionAsync(requestType, version, currentUserId);
            if (!success)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = $"Version {version} of '{requestType}' not found.",
                    Data = false
                });
            }

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"Successfully rolled back '{requestType}' active workflow to version {version}.",
                Data = true
            });
        }
    }

    public class WorkflowCancelRequest
    {
        public string? Reason { get; set; }
    }
}
