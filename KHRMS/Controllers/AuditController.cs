using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        /// <summary>
        /// Query system audit trail logs with filtering
        /// </summary>
        [HttpGet("logs")]
        [RequirePermission("AUDIT_VIEW")]
        public async Task<IActionResult> GetLogs(
            [FromQuery] string? moduleName = null,
            [FromQuery] string? entityName = null,
            [FromQuery] string? action = null,
            [FromQuery] long? actorEmployeeId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            Log.Information("AuditController - GetLogs called with filters: Module={Module}, Entity={Entity}, Action={Action}, Actor={Actor}",
                moduleName, entityName, action, actorEmployeeId);

            var logs = await _auditService.GetAuditLogsAsync(moduleName, entityName, action, actorEmployeeId, fromDate, toDate);

            return Ok(new ApiResponse<IEnumerable<AuditLog>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Audit logs retrieved successfully.",
                Data = logs
            });
        }

        /// <summary>
        /// Returns available modules for audit filtering
        /// </summary>
        [HttpGet("modules")]
        [RequirePermission("AUDIT_VIEW")]
        public IActionResult GetModules()
        {
            var modules = new List<string>
            {
                "Security & Governance",
                "Workforce",
                "Payroll",
                "Attendance",
                "Leave Management",
                "Assets & Equipment",
                "System Operations",
                "Expense Management",
                "Recruitment",
                "Performance Management"
            };

            return Ok(new ApiResponse<List<string>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Audit modules retrieved successfully.",
                Data = modules
            });
        }
    }
}
