using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Authorization;
using KHRMS.Core.Models;
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
    public class CompanyPolicyController : ControllerBase
    {
        private readonly ICompanyPolicyService _policyService;
        private readonly IUserContextService _userContext;

        public CompanyPolicyController(ICompanyPolicyService policyService, IUserContextService userContext)
        {
            _policyService = policyService ?? throw new ArgumentNullException(nameof(policyService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] bool activeOnly = true)
        {
            try
            {
                long currentEmpId = _userContext.GetCurrentEmployeeId();
                var policies = await _policyService.GetAllPoliciesAsync(currentEmpId, category, activeOnly);
                return Ok(new ApiResponse<List<PolicyDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"{policies.Count} policy(ies) retrieved successfully.",
                    Data = policies
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompanyPolicyController - Error fetching policies");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<PolicyDto>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while retrieving policies.",
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            try
            {
                long currentEmpId = _userContext.GetCurrentEmployeeId();
                var policy = await _policyService.GetPolicyByIdAsync(id, currentEmpId);
                if (policy == null)
                {
                    return NotFound(new ApiResponse<PolicyDto>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = $"Policy #{id} not found.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<PolicyDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Policy retrieved successfully.",
                    Data = policy
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompanyPolicyController - Error fetching policy {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<PolicyDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while retrieving policy details.",
                    Data = null
                });
            }
        }

        [HttpPost]
        [RequirePermission("POLICY_MANAGE")]
        public async Task<IActionResult> Create([FromBody] CreateCompanyPolicyRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ContentHtml))
            {
                return BadRequest(new ApiResponse<CompanyPolicy>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Title and ContentHtml are required.",
                    Data = null
                });
            }

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var created = await _policyService.CreatePolicyAsync(request, currentUserId);
                Log.Information("CompanyPolicyController - Policy '{Title}' created by User {UserId}", created.Title, currentUserId);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ApiResponse<CompanyPolicy>
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Message = $"Policy '{created.Title}' created successfully.",
                    Data = created
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompanyPolicyController - Error creating policy");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<CompanyPolicy>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while creating the policy.",
                    Data = null
                });
            }
        }

        [HttpPut]
        [RequirePermission("POLICY_MANAGE")]
        public async Task<IActionResult> Update([FromBody] UpdateCompanyPolicyRequest request)
        {
            if (request == null || request.Id <= 0 || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ContentHtml))
            {
                return BadRequest(new ApiResponse<CompanyPolicy>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid Id, Title, and ContentHtml are required.",
                    Data = null
                });
            }

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var updated = await _policyService.UpdatePolicyAsync(request, currentUserId);
                Log.Information("CompanyPolicyController - Policy '{Title}' updated by User {UserId}", updated.Title, currentUserId);

                return Ok(new ApiResponse<CompanyPolicy>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Policy '{updated.Title}' updated successfully.",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<CompanyPolicy>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompanyPolicyController - Error updating policy {Id}", request.Id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<CompanyPolicy>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while updating the policy.",
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        [RequirePermission("POLICY_MANAGE")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var success = await _policyService.DeletePolicyAsync(id);
                if (!success)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = $"Policy #{id} not found or already deleted.",
                        Data = false
                    });
                }

                Log.Information("CompanyPolicyController - Policy {Id} deleted by User {UserId}", id, _userContext.GetCurrentEmployeeId());
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Policy deleted successfully.",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompanyPolicyController - Error deleting policy {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while deleting the policy.",
                    Data = false
                });
            }
        }

        [HttpPost("acknowledge")]
        public async Task<IActionResult> Acknowledge([FromBody] AcknowledgePolicyRequest request)
        {
            if (request == null || request.PolicyId <= 0 || request.PolicyVersion <= 0)
            {
                return BadRequest(new ApiResponse<PolicyAcknowledgement>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid PolicyId and PolicyVersion are required.",
                    Data = null
                });
            }

            try
            {
                long currentEmpId = _userContext.GetCurrentEmployeeId();
                if (currentEmpId <= 0)
                {
                    return Unauthorized(new ApiResponse<PolicyAcknowledgement>
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Message = "Valid employee ID required for policy acknowledgement.",
                        Data = null
                    });
                }

                string? clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                var ack = await _policyService.AcknowledgePolicyAsync(request.PolicyId, request.PolicyVersion, currentEmpId, clientIp);
                Log.Information("CompanyPolicyController - Policy {PolicyId} v{Version} acknowledged by Employee {EmployeeId}",
                    request.PolicyId, request.PolicyVersion, currentEmpId);

                return Ok(new ApiResponse<PolicyAcknowledgement>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Policy acknowledged successfully.",
                    Data = ack
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<PolicyAcknowledgement>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompanyPolicyController - Error acknowledging policy {PolicyId}", request.PolicyId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<PolicyAcknowledgement>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while recording policy acknowledgement.",
                    Data = null
                });
            }
        }

        [HttpGet("{id}/acknowledgements")]
        [RequirePermission("POLICY_MANAGE")]
        public async Task<IActionResult> GetAcknowledgements(long id)
        {
            try
            {
                var acks = await _policyService.GetPolicyAcknowledgementsAsync(id);
                return Ok(new ApiResponse<List<PolicyAcknowledgement>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"{acks.Count} acknowledgement(s) found.",
                    Data = acks
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompanyPolicyController - Error fetching acknowledgements for policy {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<PolicyAcknowledgement>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while fetching acknowledgements.",
                    Data = null
                });
            }
        }

        [HttpGet("compliance-summary")]
        [RequirePermission("POLICY_MANAGE")]
        public async Task<IActionResult> GetComplianceSummary()
        {
            try
            {
                var summary = await _policyService.GetComplianceSummaryAsync();
                return Ok(new ApiResponse<PolicyComplianceSummaryDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Policy compliance summary retrieved successfully.",
                    Data = summary
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompanyPolicyController - Error fetching compliance summary");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<PolicyComplianceSummaryDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while generating compliance summary.",
                    Data = null
                });
            }
        }
    }
}
