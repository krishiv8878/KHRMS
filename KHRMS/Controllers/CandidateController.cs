using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController(ICandidateService candidateService) : ControllerBase
    {
        public readonly ICandidateService _candidateService = candidateService;



        /// <summary>
        /// Get the list of candidates
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCandidates")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations,Manager,Management,Employee")]
        public async Task<IActionResult> GetCandidates()
        {
            Log.Information("GetCandidates API called.");

            var isPrivileged = User.IsInRole("Admin") || User.IsInRole("System Admin") || User.IsInRole("HR") || User.IsInRole("HR Operations");
            long currentEmpId = 0;
            var claimVal = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out long parsedId))
            {
                currentEmpId = parsedId;
            }

            var candidates = await _candidateService.GetAllCandidates();
            if (candidates == null || !candidates.Any())
            {
                Log.Information("No candidate records found.");
                return Ok(new ApiResponse<List<Candidate>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.NoCandidateFound,
                    Data = null
                });
            }

            // Non-HR/Admin users (Interviewers: Managers/Employees) only receive candidates currently assigned to them and pending their feedback
            if (!isPrivileged)
            {
                candidates = candidates.Where(c =>
                {
                    if (string.IsNullOrWhiteSpace(c.RelevantExperience)) return false;
                    var exp = c.RelevantExperience.Trim();
                    if (!exp.StartsWith("{")) return false;
                    // If candidate is assigned to this employee
                    if (currentEmpId > 0 && (exp.Contains($"\"interviewerId\":{currentEmpId}") || exp.Contains($"\"interviewerId\":\"{currentEmpId}\"")))
                    {
                        // If feedback was already submitted, it has been handed back to HR and removed from interviewer's view
                        if (exp.Contains("\"status\":\"Feedback Submitted") || exp.Contains("Awaiting HR Review") || exp.Contains("Pending HR Decision"))
                        {
                            return false;
                        }
                        return true;
                    }
                    return false;
                }).ToList();
            }

            Log.Information("Candidate records found successfully.");
            return Ok(new ApiResponse<List<Candidate>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.CandidateFound,
                Data = candidates.ToList()
            });
        }


        /// <summary>
        /// Add a new candidate
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>

        [HttpPost("AddCandidate")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        public async Task<IActionResult> AddCandidate(Candidate candidate)
        {
            Log.Information("AddCandidate API called.");

            var isCandidateAdded = await _candidateService.CreateCandidate(candidate);
            if (isCandidateAdded)
            {
                Log.Information("Candidate added successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.CandidateAdded,
                    Data = true
                });
            }

            Log.Warning("Failed to add candidate.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.CandidateNotAdded,
                Data = false
            });
        }


        /// <summary>
        /// Update a existing candidate
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>

        [HttpPut("UpdateCandidate")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations,Manager,Management,Employee")]
        public async Task<IActionResult> UpdateCandidate(Candidate candidate)
        {
            Log.Information("UpdateCandidate API called.");

            var isCandidateUpdated = await _candidateService.UpdateCandidate(candidate);
            if (isCandidateUpdated)
            {
                Log.Information("Candidate updated successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.CandidateUpdated,
                    Data = true
                });
            }

            Log.Warning("Failed to update candidate.");
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.CandidateNotUpdated,
                Data = false
            });
        }


        /// <summary>
        /// Delete existing candidate
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns></returns>

        [HttpDelete("DeleteCandidate/{candidateId}")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        public async Task<IActionResult> DeleteCandidate(long candidateId)
        {
            Log.Information("DeleteCandidate API called for ID {CandidateId}.", candidateId);

            var isCandidateDeleted = await _candidateService.DeleteCandidate(candidateId);
            if (isCandidateDeleted)
            {
                Log.Information("Candidate with ID {CandidateId} deleted successfully.", candidateId);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.CandidateDeleted,
                    Data = true
                });
            }

            Log.Warning("Failed to delete candidate with ID {CandidateId}.", candidateId);
            return BadRequest(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = ApiMessageConstant.CandidateNotDeleted,
                Data = false
            });
        }

        /// <summary>
        /// Onboard a candidate to an Employee with credentials and CTC breakdown
        /// </summary>
        [HttpPost("OnboardCandidate")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        public async Task<IActionResult> OnboardCandidate([FromBody] KHRMS.Services.Request.CandidateOnboardRequest request)
        {
            Log.Information("OnboardCandidate API called for CandidateId {CandidateId}, Email {Email}.", request?.CandidateId, request?.EmailAddress);

            if (request == null || string.IsNullOrWhiteSpace(request.EmailAddress))
            {
                return BadRequest(new ApiResponse<KHRMS.Services.Request.CandidateOnboardResponse>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid candidate onboarding details and email are required.",
                    Data = null
                });
            }

            try
            {
                var response = await _candidateService.OnboardCandidate(request);
                return Ok(new ApiResponse<KHRMS.Services.Request.CandidateOnboardResponse>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Candidate onboarded as employee successfully.",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while onboarding candidate {CandidateId}", request.CandidateId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<KHRMS.Services.Request.CandidateOnboardResponse>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = $"Failed to onboard candidate: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}
