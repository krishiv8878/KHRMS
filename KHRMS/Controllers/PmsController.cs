using System.Net;
using KHRMS.Authorization;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHRMS.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PmsController : ControllerBase
    {
        private readonly IPmsService _pmsService;
        private readonly IUserContextService _userContext;

        public PmsController(IPmsService pmsService, IUserContextService userContext)
        {
            _pmsService = pmsService;
            _userContext = userContext;
        }

        #region 1. Goals & OKRs

        [HttpGet("goals")]
        public async Task<IActionResult> GetGoals([FromQuery] long? employeeId, [FromQuery] string? period, [FromQuery] string? category)
        {
            var goals = await _pmsService.GetGoalsAsync(employeeId, period, category);
            return Ok(new ApiResponse<List<PmsGoalDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Goals retrieved successfully",
                Data = goals
            });
        }

        [HttpGet("goals/{id}")]
        public async Task<IActionResult> GetGoalById(long id)
        {
            var goal = await _pmsService.GetGoalByIdAsync(id);
            if (goal == null) return NotFound(new ApiResponse<string> { StatusCode = (int)HttpStatusCode.NotFound, Message = "Goal not found" });

            return Ok(new ApiResponse<PmsGoalDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Goal details retrieved successfully",
                Data = goal
            });
        }

        [HttpPost("goals")]
        public async Task<IActionResult> CreateGoal([FromBody] CreatePmsGoalRequest request)
        {
            var result = await _pmsService.CreateGoalAsync(request);
            return Ok(new ApiResponse<PmsGoalDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Goal created successfully",
                Data = result
            });
        }

        [HttpPut("goals/{id}")]
        public async Task<IActionResult> UpdateGoal(long id, [FromBody] UpdatePmsGoalRequest request)
        {
            request.Id = id;
            var result = await _pmsService.UpdateGoalAsync(request);
            return Ok(new ApiResponse<PmsGoalDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Goal updated successfully",
                Data = result
            });
        }

        [HttpDelete("goals/{id}")]
        public async Task<IActionResult> DeleteGoal(long id)
        {
            var success = await _pmsService.DeleteGoalAsync(id);
            if (!success) return NotFound(new ApiResponse<string> { StatusCode = (int)HttpStatusCode.NotFound, Message = "Goal not found" });

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Goal deleted successfully",
                Data = true
            });
        }

        [HttpPost("goals/{id}/checkin")]
        public async Task<IActionResult> RecordCheckIn(long id, [FromBody] PmsGoalCheckInRequest request)
        {
            request.GoalId = id;
            var result = await _pmsService.RecordCheckInAsync(request);
            return Ok(new ApiResponse<PmsGoalDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Goal progress updated successfully",
                Data = result
            });
        }

        #endregion

        #region 2. Review Cycles & Appraisals

        [HttpGet("cycles")]
        public async Task<IActionResult> GetReviewCycles()
        {
            var cycles = await _pmsService.GetReviewCyclesAsync();
            return Ok(new ApiResponse<List<PmsReviewCycle>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Review cycles retrieved successfully",
                Data = cycles
            });
        }

        [HttpPost("cycles")]
        [RequirePermission("PMS_MANAGE")]
        public async Task<IActionResult> CreateReviewCycle([FromBody] CreateReviewCycleRequest request)
        {
            var result = await _pmsService.CreateReviewCycleAsync(request);
            return Ok(new ApiResponse<PmsReviewCycle>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Review cycle launched successfully",
                Data = result
            });
        }

        [HttpGet("appraisals")]
        public async Task<IActionResult> GetAppraisals([FromQuery] long? cycleId, [FromQuery] long? employeeId, [FromQuery] long? managerId)
        {
            var appraisals = await _pmsService.GetAppraisalsAsync(cycleId, employeeId, managerId);
            return Ok(new ApiResponse<List<PmsAppraisalDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Appraisals retrieved successfully",
                Data = appraisals
            });
        }

        [HttpGet("appraisals/{id}")]
        public async Task<IActionResult> GetAppraisalById(long id)
        {
            var appraisal = await _pmsService.GetAppraisalByIdAsync(id);
            if (appraisal == null) return NotFound(new ApiResponse<string> { StatusCode = (int)HttpStatusCode.NotFound, Message = "Appraisal not found" });

            return Ok(new ApiResponse<PmsAppraisalDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Appraisal retrieved successfully",
                Data = appraisal
            });
        }

        [HttpPost("appraisals/submit-self")]
        public async Task<IActionResult> SubmitSelfReview([FromBody] SubmitSelfReviewRequest request)
        {
            var result = await _pmsService.SubmitSelfReviewAsync(request);
            return Ok(new ApiResponse<PmsAppraisalDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Self-appraisal submitted successfully to manager",
                Data = result
            });
        }

        [HttpPost("appraisals/submit-manager")]
        public async Task<IActionResult> SubmitManagerReview([FromBody] SubmitManagerReviewRequest request)
        {
            var result = await _pmsService.SubmitManagerReviewAsync(request);
            return Ok(new ApiResponse<PmsAppraisalDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Manager evaluation submitted successfully",
                Data = result
            });
        }

        [HttpPost("appraisals/finalize")]
        [RequirePermission("PMS_MANAGE")]
        public async Task<IActionResult> FinalizeAppraisal([FromBody] FinalizeAppraisalRequest request)
        {
            var result = await _pmsService.FinalizeAppraisalAsync(request);
            return Ok(new ApiResponse<PmsAppraisalDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Appraisal finalized and published successfully",
                Data = result
            });
        }

        #endregion

        #region 3. Continuous Feedback

        [HttpGet("feedback")]
        public async Task<IActionResult> GetFeedback([FromQuery] long? employeeId, [FromQuery] string? feedbackType)
        {
            var feedbacks = await _pmsService.GetFeedbackAsync(employeeId, feedbackType);
            return Ok(new ApiResponse<List<PmsFeedbackDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Feedbacks retrieved successfully",
                Data = feedbacks
            });
        }

        [HttpPost("feedback")]
        public async Task<IActionResult> GiveFeedback([FromBody] PmsFeedbackRequest request)
        {
            var result = await _pmsService.GiveFeedbackAsync(request);
            return Ok(new ApiResponse<PmsFeedbackDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Feedback shared successfully",
                Data = result
            });
        }

        #endregion

        #region 4. 1-on-1 Meetings

        [HttpGet("one-on-one")]
        public async Task<IActionResult> GetOneOnOnes([FromQuery] long? managerId, [FromQuery] long? employeeId)
        {
            var list = await _pmsService.GetOneOnOnesAsync(managerId, employeeId);
            return Ok(new ApiResponse<List<PmsOneOnOneDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "1-on-1 meetings retrieved successfully",
                Data = list
            });
        }

        [HttpPost("one-on-one")]
        public async Task<IActionResult> SaveOneOnOne([FromBody] PmsOneOnOneRequest request)
        {
            var result = await _pmsService.SaveOneOnOneAsync(request);
            return Ok(new ApiResponse<PmsOneOnOneDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "1-on-1 meeting saved successfully",
                Data = result
            });
        }

        #endregion

        #region 5. Performance Improvement Plans (PIP)

        [HttpGet("pip")]
        public async Task<IActionResult> GetPips([FromQuery] long? employeeId)
        {
            var list = await _pmsService.GetPipsAsync(employeeId);
            return Ok(new ApiResponse<List<PmsPipDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Performance Improvement Plans retrieved successfully",
                Data = list
            });
        }

        [HttpPost("pip")]
        public async Task<IActionResult> SavePip([FromBody] PmsPipRequest request)
        {
            var result = await _pmsService.SavePipAsync(request);
            return Ok(new ApiResponse<PmsPipDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "PIP track saved successfully",
                Data = result
            });
        }

        #endregion
    }
}
