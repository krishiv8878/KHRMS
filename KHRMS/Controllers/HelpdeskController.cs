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
    public class HelpdeskController : ControllerBase
    {
        private readonly IHelpdeskService _helpdeskService;
        private readonly IUserContextService _userContext;

        public HelpdeskController(IHelpdeskService helpdeskService, IUserContextService userContext)
        {
            _helpdeskService = helpdeskService ?? throw new ArgumentNullException(nameof(helpdeskService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets(
            [FromQuery] string? status,
            [FromQuery] string? category,
            [FromQuery] string? priority,
            [FromQuery] long? requesterId,
            [FromQuery] long? assigneeId)
        {
            try
            {
                var tickets = await _helpdeskService.GetTicketsAsync(status, category, priority, requesterId, assigneeId);
                return Ok(new ApiResponse<List<HelpdeskTicketDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"{tickets.Count} helpdesk ticket(s) retrieved successfully.",
                    Data = tickets
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error fetching tickets");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<HelpdeskTicketDto>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while fetching helpdesk tickets.",
                    Data = null
                });
            }
        }

        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetTicketById(long id)
        {
            try
            {
                var ticket = await _helpdeskService.GetTicketByIdAsync(id);
                if (ticket == null)
                {
                    return NotFound(new ApiResponse<HelpdeskTicketDto>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = $"Helpdesk ticket #{id} not found.",
                        Data = null
                    });
                }

                long currentEmpId = _userContext.GetCurrentEmployeeId();
                bool isAdminOrHr = _userContext.IsAdmin() || _userContext.IsHR();
                bool isManager = _userContext.IsManager();
                if (!isAdminOrHr && !isManager && ticket.RequesterEmployeeId != currentEmpId && ticket.AssignedToEmployeeId != currentEmpId)
                {
                    return Forbid();
                }

                return Ok(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Helpdesk ticket retrieved successfully.",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error fetching ticket {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while fetching the ticket details.",
                    Data = null
                });
            }
        }

        [HttpPost("tickets")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateHelpdeskTicketRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Subject and Description are required.",
                    Data = null
                });
            }

            try
            {
                long currentEmpId = _userContext.GetCurrentEmployeeId();
                if (currentEmpId <= 0)
                {
                    return Unauthorized(new ApiResponse<HelpdeskTicketDto>
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized,
                        Message = "Valid employee ID required to file a ticket.",
                        Data = null
                    });
                }

                var ticket = await _helpdeskService.CreateTicketAsync(request, currentEmpId);
                Log.Information("HelpdeskController - Ticket {TicketNumber} created by Employee {EmployeeId}", ticket.TicketNumber, currentEmpId);

                return CreatedAtAction(nameof(GetTicketById), new { id = ticket.Id }, new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Message = $"Helpdesk ticket #{ticket.TicketNumber} submitted successfully.",
                    Data = ticket
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error creating ticket");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while creating the helpdesk ticket.",
                    Data = null
                });
            }
        }

        [HttpPut("tickets/status")]
        public async Task<IActionResult> UpdateTicketStatus([FromBody] UpdateHelpdeskTicketStatusRequest request)
        {
            if (request == null || request.TicketId <= 0 || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid TicketId and Status are required.",
                    Data = null
                });
            }

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var updated = await _helpdeskService.UpdateTicketStatusAsync(request, currentUserId);

                Log.Information("HelpdeskController - Ticket {TicketNumber} status updated to {Status} by User {UserId}",
                    updated.TicketNumber, updated.Status, currentUserId);

                return Ok(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Helpdesk ticket #{updated.TicketNumber} status updated to '{updated.Status}'.",
                    Data = updated
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error updating ticket status for ID {Id}", request.TicketId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while updating the ticket status.",
                    Data = null
                });
            }
        }

        [HttpPost("tickets/assign")]
        public async Task<IActionResult> AssignTicket([FromBody] AssignTicketRequest request)
        {
            if (request == null || request.TicketId <= 0 || request.AssignToEmployeeId <= 0)
            {
                return BadRequest(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid TicketId and AssignToEmployeeId are required.",
                    Data = null
                });
            }

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var updated = await _helpdeskService.AssignTicketAsync(request, currentUserId);
                return Ok(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Ticket #{updated.TicketNumber} assigned to {updated.AssigneeName}.",
                    Data = updated
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error assigning ticket {TicketId}", request.TicketId);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while assigning the ticket.",
                    Data = null
                });
            }
        }

        [HttpPost("tickets/{id}/claim")]
        public async Task<IActionResult> ClaimTicket(long id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid ticket ID is required.",
                    Data = null
                });
            }

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var updated = await _helpdeskService.ClaimTicketAsync(id, currentUserId);
                return Ok(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"Ticket #{updated.TicketNumber} claimed by {updated.AssigneeName}.",
                    Data = updated
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error claiming ticket {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<HelpdeskTicketDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while claiming the ticket.",
                    Data = null
                });
            }
        }

        [HttpPost("tickets/{id}/comments")]
        public async Task<IActionResult> AddComment(long id, [FromBody] AddTicketCommentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Comment))
            {
                return BadRequest(new ApiResponse<HelpdeskTicketActivityDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Comment text is required.",
                    Data = null
                });
            }

            request.TicketId = id;

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var activity = await _helpdeskService.AddCommentAsync(request, currentUserId);
                return Ok(new ApiResponse<HelpdeskTicketActivityDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Comment posted successfully.",
                    Data = activity
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<HelpdeskTicketActivityDto>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<HelpdeskTicketActivityDto>
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = ex.Message,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error adding comment to ticket {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<HelpdeskTicketActivityDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while adding comment.",
                    Data = null
                });
            }
        }

        [HttpGet("tickets/{id}/activities")]
        public async Task<IActionResult> GetActivities(long id)
        {
            try
            {
                var activities = await _helpdeskService.GetTicketActivitiesAsync(id);
                return Ok(new ApiResponse<List<HelpdeskTicketActivityDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"{activities.Count} activity item(s) retrieved.",
                    Data = activities
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error fetching activities for ticket {Id}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<HelpdeskTicketActivityDto>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while retrieving ticket activities.",
                    Data = null
                });
            }
        }

        [HttpGet("support-agents")]
        public async Task<IActionResult> GetSupportAgents([FromQuery] string? category)
        {
            try
            {
                var agents = await _helpdeskService.GetSupportAgentsAsync(category);
                return Ok(new ApiResponse<List<EmployeeLookupDto>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = $"{agents.Count} support agent(s) available.",
                    Data = agents
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error fetching support agents");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<List<EmployeeLookupDto>>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while retrieving support agents.",
                    Data = null
                });
            }
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] string? departmentCategory)
        {
            try
            {
                var metrics = await _helpdeskService.GetHelpdeskMetricsAsync(departmentCategory);
                return Ok(new ApiResponse<HelpdeskMetricsDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Helpdesk SLA and volume metrics retrieved successfully.",
                    Data = metrics
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskController - Error fetching metrics");
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<HelpdeskMetricsDto>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while retrieving helpdesk metrics.",
                    Data = null
                });
            }
        }
    }
}
