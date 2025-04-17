using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;

namespace KHRMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidateController(ICandidateService candidateService) : ControllerBase
    {
        public readonly ICandidateService _candidateService = candidateService;

        /// <summary>
        /// Get the list of candidates
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[Route("GetCandidates")]
        //public async Task<IActionResult> GetCandidates()
        //{
        //    var candidates = await _candidateService.GetAllCandidates();
        //    if (candidates == null)
        //    {
        //        return NotFound();
        //    }
        //    // Use the wrapper class to create a consistent response
        //    var response = new ApiResponse<List<Candidate>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = candidates.Any() ? ApiMessageConstant.CandidateFound : ApiMessageConstant.NoCandidateFound,
        //        Data = candidates.ToList()
        //    };
        //    return Ok(response);
        //}
        [HttpGet("GetCandidates")]
        public async Task<IActionResult> GetCandidates()
        {
            Log.Information("GetCandidates API called.");

            var candidates = await _candidateService.GetAllCandidates();
            if (candidates == null || !candidates.Any())
            {
                Log.Information("No candidate records found.");
                return NotFound(new ApiResponse<List<Candidate>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.NoCandidateFound,
                    Data = null
                });
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
        //[HttpPost]
        //[Route("AddCandidate")]
        //public async Task<IActionResult> AddCandidate(Candidate candidate)
        //{
        //    var isCandidatedAdded = await _candidateService.CreateCandidate(candidate);
        //    if (isCandidatedAdded)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.CandidateAdded,
        //            Data = isCandidatedAdded
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.CandidateNotAdded,
        //            Data = isCandidatedAdded
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpPost("AddCandidate")]
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
        //[HttpPut]
        //[Route("UpdateCandidate")]
        //public async Task<IActionResult> UpdateCandidate(Candidate candidate)
        //{
        //    var isCandidatedEdited = await _candidateService.UpdateCandidate(candidate);
        //    if (isCandidatedEdited)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.CandidateUpdated,
        //            Data = isCandidatedEdited
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.CandidateNotUpdated,
        //            Data = isCandidatedEdited
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpPut("UpdateCandidate")]
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
        //[HttpDelete]
        //[Route("DeleteCandidate")]
        //public async Task<IActionResult> DeleteCandidate(long candidateId)
        //{
        //    var isCandidatedDeleted = await _candidateService.DeleteCandidate(candidateId);
        //    if (isCandidatedDeleted)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.CandidateDeleted,
        //            Data = isCandidatedDeleted
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.CandidateNotDeleted,
        //            Data = isCandidatedDeleted
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpDelete("DeleteCandidate")]
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
    }
}
