using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Authorization;
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
    public class RecruitmentController : ControllerBase
    {
        private readonly IRecruitmentService _recruitmentService;

        public RecruitmentController(IRecruitmentService recruitmentService)
        {
            _recruitmentService = recruitmentService;
        }

        // ==========================================
        // REQUISITIONS
        // ==========================================
        [HttpGet("requisitions")]
        public async Task<IActionResult> GetAllRequisitions([FromQuery] string? status = null)
        {
            try
            {
                var list = await _recruitmentService.GetAllRequisitions(status);
                return Ok(list);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching job requisitions");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("requisitions/{id}")]
        public async Task<IActionResult> GetRequisitionById(long id)
        {
            try
            {
                var req = await _recruitmentService.GetRequisitionById(id);
                if (req == null) return NotFound(new { message = $"Requisition {id} not found" });
                return Ok(req);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching job requisition {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("requisitions")]
        [RequirePermission("RECRUITMENT_MANAGE")]
        public async Task<IActionResult> CreateRequisition([FromBody] CreateJobRequisitionRequest request)
        {
            try
            {
                var created = await _recruitmentService.CreateRequisition(request);
                return Ok(created);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating job requisition");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("requisitions/{id}")]
        [RequirePermission("RECRUITMENT_MANAGE")]
        public async Task<IActionResult> UpdateRequisition(long id, [FromBody] CreateJobRequisitionRequest request)
        {
            try
            {
                var updated = await _recruitmentService.UpdateRequisition(id, request);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating job requisition {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("requisitions/{id}/status")]
        [RequirePermission("RECRUITMENT_MANAGE")]
        public async Task<IActionResult> UpdateRequisitionStatus(long id, [FromBody] UpdateCandidateStageRequest req)
        {
            try
            {
                var success = await _recruitmentService.UpdateRequisitionStatus(id, req.NewStage);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating requisition status {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("requisitions/{id}")]
        [RequirePermission("RECRUITMENT_MANAGE")]
        public async Task<IActionResult> DeleteRequisition(long id)
        {
            try
            {
                var success = await _recruitmentService.DeleteRequisition(id);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting requisition {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // PIPELINE & CANDIDATES
        // ==========================================
        [HttpGet("pipeline")]
        public async Task<IActionResult> GetPipeline([FromQuery] long? requisitionId = null, [FromQuery] string? stage = null)
        {
            try
            {
                var list = await _recruitmentService.GetPipelineCandidates(requisitionId, stage);
                return Ok(list);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching candidate pipeline");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("pipeline/{candidateId}")]
        public async Task<IActionResult> GetCandidateDetail(long candidateId)
        {
            try
            {
                var cand = await _recruitmentService.GetCandidatePipelineDetail(candidateId);
                if (cand == null) return NotFound(new { message = "Candidate not found" });
                return Ok(cand);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching candidate detail {CandidateId}", candidateId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("pipeline/stage")]
        [RequirePermission("RECRUITMENT_MANAGE")]
        public async Task<IActionResult> UpdateCandidateStage([FromBody] UpdateCandidateStageRequest request)
        {
            try
            {
                var success = await _recruitmentService.UpdateCandidateStage(request);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error advancing candidate stage");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // INTERVIEWS
        // ==========================================
        [HttpGet("interviews")]
        public async Task<IActionResult> GetInterviews([FromQuery] long? candidateId = null, [FromQuery] long? interviewerId = null, [FromQuery] string? status = null)
        {
            try
            {
                var isPrivileged = User.IsInRole("Admin") || User.IsInRole("System Admin") || User.IsInRole("HR") || User.IsInRole("HR Operations");
                long currentEmpId = 0;
                var claimVal = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out long parsedId))
                {
                    currentEmpId = parsedId;
                }

                // If non-privileged (employee / interviewer), automatically filter to interviews assigned to them
                if (!isPrivileged && currentEmpId > 0 && !interviewerId.HasValue)
                {
                    interviewerId = currentEmpId;
                }

                var list = await _recruitmentService.GetInterviews(candidateId, interviewerId, status);
                return Ok(list);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching interviews");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("interviews/schedule")]
        [RequirePermission("RECRUITMENT_MANAGE")]
        public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleInterviewRequest request)
        {
            try
            {
                var interview = await _recruitmentService.ScheduleInterview(request);
                return Ok(interview);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error scheduling interview");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("interviews/feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] SubmitInterviewFeedbackRequest request)
        {
            try
            {
                var updated = await _recruitmentService.SubmitInterviewFeedback(request);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error submitting interview feedback");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("interviews/{id}/cancel")]
        public async Task<IActionResult> CancelInterview(long id, [FromBody] UpdateCandidateStageRequest req)
        {
            try
            {
                var success = await _recruitmentService.CancelInterview(id, req.Notes ?? "Cancelled by organizer");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error cancelling interview {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // OFFERS
        // ==========================================
        [HttpGet("offers")]
        public async Task<IActionResult> GetOffers([FromQuery] long? requisitionId = null, [FromQuery] string? status = null)
        {
            try
            {
                var list = await _recruitmentService.GetOffers(requisitionId, status);
                return Ok(list);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching job offers");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("offers")]
        [RequirePermission("RECRUITMENT_MANAGE")]
        public async Task<IActionResult> CreateOffer([FromBody] CreateJobOfferRequest request)
        {
            try
            {
                var offer = await _recruitmentService.CreateOffer(request);
                return Ok(offer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating job offer");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("offers/{id}/status")]
        [RequirePermission("RECRUITMENT_MANAGE")]
        public async Task<IActionResult> UpdateOfferStatus(long id, [FromBody] UpdateCandidateStageRequest req)
        {
            try
            {
                var offer = await _recruitmentService.UpdateOfferStatus(id, req.NewStage, req.Notes);
                return Ok(offer);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating offer status {Id}", id);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // METRICS
        // ==========================================
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            try
            {
                var metrics = await _recruitmentService.GetDashboardMetrics();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching recruitment metrics");
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
