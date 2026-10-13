using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserContextService _userContextService;

        public NotificationController(INotificationService notificationService, IUserContextService userContextService)
        {
            _notificationService = notificationService;
            _userContextService = userContextService;
        }

        private long ResolveTargetEmployeeId(long? requestedEmpId)
        {
            long currentEmpId = _userContextService.GetCurrentEmployeeId();
            if (currentEmpId <= 0 && requestedEmpId.HasValue && requestedEmpId.Value > 0)
            {
                return requestedEmpId.Value;
            }
            if (requestedEmpId.HasValue && requestedEmpId.Value > 0 && requestedEmpId.Value != currentEmpId)
            {
                // Only Admin or HR can access another employee's notifications
                if (_userContextService.IsAdmin() || _userContextService.IsHR())
                {
                    return requestedEmpId.Value;
                }
            }
            return currentEmpId;
        }

        [HttpGet("GetNotifications")]
        public async Task<IActionResult> GetNotifications([FromQuery] long? employeeId)
        {
            try
            {
                long empId = ResolveTargetEmployeeId(employeeId);
                var notifications = await _notificationService.GetNotifications(empId);

                return Ok(new ApiResponse<IEnumerable<Notification>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Notifications retrieved successfully.",
                    Data = notifications
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<Notification>>
                {
                    StatusCode = 500,
                    Message = $"Error retrieving notifications: {ex.Message}",
                    Data = new List<Notification>()
                });
            }
        }

        [HttpGet("GetUnreadCount")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] long? employeeId)
        {
            try
            {
                long empId = ResolveTargetEmployeeId(employeeId);
                int count = await _notificationService.GetUnreadCount(empId);

                return Ok(new ApiResponse<int>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Unread count retrieved successfully.",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<int>
                {
                    StatusCode = 500,
                    Message = $"Error retrieving unread count: {ex.Message}",
                    Data = 0
                });
            }
        }

        [HttpPost("MarkAsRead")]
        public async Task<IActionResult> MarkAsRead([FromQuery] long id, [FromQuery] long? employeeId)
        {
            try
            {
                long empId = ResolveTargetEmployeeId(employeeId);
                bool result = await _notificationService.MarkAsRead(id, empId);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = result ? "Notification marked as read." : "Notification not found.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    StatusCode = 500,
                    Message = $"Error marking notification as read: {ex.Message}",
                    Data = false
                });
            }
        }

        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] long? employeeId)
        {
            try
            {
                long empId = ResolveTargetEmployeeId(employeeId);
                bool result = await _notificationService.MarkAllAsRead(empId);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "All notifications marked as read.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    StatusCode = 500,
                    Message = $"Error marking all notifications as read: {ex.Message}",
                    Data = false
                });
            }
        }

        [HttpPost("DeleteNotification")]
        [HttpDelete("DeleteNotification")]
        public async Task<IActionResult> DeleteNotification([FromQuery] long id, [FromQuery] long? employeeId)
        {
            try
            {
                long empId = ResolveTargetEmployeeId(employeeId);
                bool result = await _notificationService.DeleteNotification(id, empId);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = result ? "Notification removed successfully." : "Notification not found.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    StatusCode = 500,
                    Message = $"Error deleting notification: {ex.Message}",
                    Data = false
                });
            }
        }

        [HttpPost("ClearAllNotifications")]
        [HttpDelete("ClearAllNotifications")]
        public async Task<IActionResult> ClearAllNotifications([FromQuery] long? employeeId)
        {
            try
            {
                long empId = ResolveTargetEmployeeId(employeeId);
                bool result = await _notificationService.ClearAllNotifications(empId);

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "All notifications cleared successfully.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    StatusCode = 500,
                    Message = $"Error clearing all notifications: {ex.Message}",
                    Data = false
                });
            }
        }

        [HttpPost("CreateNotification")]
        public async Task<IActionResult> CreateNotification([FromBody] Notification notification)
        {
            try
            {
                if (notification == null)
                {
                    return BadRequest(new ApiResponse<Notification>
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = "Notification payload is required.",
                        Data = null
                    });
                }

                var created = await _notificationService.CreateNotification(notification);

                return Ok(new ApiResponse<Notification>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = "Notification created successfully.",
                    Data = created
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<Notification>
                {
                    StatusCode = 500,
                    Message = $"Error creating notification: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}
