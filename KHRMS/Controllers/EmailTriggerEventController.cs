using System.Net;
using System.Text.Json;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmailTriggerEventController : ControllerBase
    {
        private readonly IEmailTriggerEventService _triggerService;
        private readonly ISendEmailService _sendEmailService;

        public EmailTriggerEventController(IEmailTriggerEventService triggerService, ISendEmailService sendEmailService)
        {
            _triggerService = triggerService;
            _sendEmailService = sendEmailService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            Log.Information("GetAll EmailTriggerEvents API called.");
            var list = await _triggerService.GetAllAsync();

            return Ok(new ApiResponse<IEnumerable<EmailTriggerEvent>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Email trigger events retrieved successfully",
                Data = list
            });
        }

        [HttpGet("GetByEventCode")]
        public async Task<IActionResult> GetByEventCode([FromQuery] string eventCode)
        {
            if (string.IsNullOrWhiteSpace(eventCode))
            {
                return BadRequest(new ApiResponse<EmailTriggerEvent>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "eventCode is required"
                });
            }

            var evt = await _triggerService.GetByEventCodeAsync(eventCode);
            if (evt == null)
            {
                return NotFound(new ApiResponse<EmailTriggerEvent>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = $"Trigger event '{eventCode}' not found"
                });
            }

            return Ok(new ApiResponse<EmailTriggerEvent>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Trigger event found",
                Data = evt
            });
        }

        public class UpdateTriggerBindingRequest
        {
            public long Id { get; set; }
            public long? ActiveTemplateId { get; set; }
            public bool IsEnabled { get; set; } = true;
            public string? DefaultSubject { get; set; }
        }

        [HttpPut("UpdateBinding")]
        public async Task<IActionResult> UpdateBinding([FromBody] UpdateTriggerBindingRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid binding update request"
                });
            }

            var success = await _triggerService.UpdateBindingAsync(request.Id, request.ActiveTemplateId, request.IsEnabled, request.DefaultSubject);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = success ? "Email trigger binding updated successfully" : "Failed to update binding",
                Data = success
            });
        }

        [HttpPost("SeedDefaults")]
        public async Task<IActionResult> SeedDefaults()
        {
            var result = await _triggerService.SeedDefaultEventsAsync();
            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = result ? "Default trigger events seeded successfully" : "Defaults already present or could not be seeded",
                Data = result
            });
        }

        public class SendTestEmailRequest
        {
            public string EventCode { get; set; } = string.Empty;
            public string ToEmail { get; set; } = string.Empty;
        }

        [HttpPost("SendTestEmail")]
        public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ToEmail) || string.IsNullOrWhiteSpace(request.EventCode))
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "EventCode and ToEmail are required"
                });
            }

            var evt = await _triggerService.GetByEventCodeAsync(request.EventCode);
            if (evt == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = $"Event '{request.EventCode}' not found"
                });
            }

            // Build test sample dictionary from event's available variables
            var sampleDict = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(evt.AvailableVariablesJson))
                {
                    using var doc = JsonDocument.Parse(evt.AvailableVariablesJson);
                    foreach (var elem in doc.RootElement.EnumerateArray())
                    {
                        if (elem.TryGetProperty("key", out var k) && elem.TryGetProperty("sample", out var s))
                        {
                            sampleDict[k.GetString() ?? ""] = s.GetString() ?? "";
                        }
                    }
                }
            }
            catch
            {
            }

            // Provide default values if empty
            if (!sampleDict.ContainsKey("EmployeeName")) sampleDict["EmployeeName"] = "Alex Morgan";
            if (!sampleDict.ContainsKey("ManagerName")) sampleDict["ManagerName"] = "Sarah Jenkins";
            if (!sampleDict.ContainsKey("CompanyName")) sampleDict["CompanyName"] = "KHRMS Enterprise";
            if (!sampleDict.ContainsKey("ActionUrl")) sampleDict["ActionUrl"] = "https://khrms.internal/login";

            var subject = $"[TEST] {evt.DefaultSubject}";
            var sent = await _sendEmailService.SendEventEmailAsync(request.EventCode, request.ToEmail, sampleDict, subject);

            return Ok(new ApiResponse<bool>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = sent ? $"Test email for '{evt.EventName}' sent to {request.ToEmail}!" : "Failed to send test email. Check SMTP settings or template mapping.",
                Data = sent
            });
        }
    }
}
