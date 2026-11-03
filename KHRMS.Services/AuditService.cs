using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace KHRMS.Services
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserContextService _userContextService;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public AuditService(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _userContextService = userContextService;
        }

        public async Task LogAsync(
            string moduleName,
            string entityName,
            string entityId,
            string action,
            object? beforeValues = null,
            object? afterValues = null,
            string? reason = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                long actorId = _userContextService.GetCurrentEmployeeId();

                string actorEmail = httpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                                 ?? httpContext?.User?.FindFirst("email")?.Value
                                 ?? httpContext?.User?.Identity?.Name
                                 ?? "System";

                string? ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
                if (httpContext?.Request?.Headers.TryGetValue("X-Forwarded-For", out var forwarded) == true)
                {
                    ipAddress = forwarded.ToString().Split(',')[0].Trim();
                }

                string? userAgent = httpContext?.Request?.Headers.UserAgent.ToString();
                if (!string.IsNullOrEmpty(userAgent) && userAgent.Length > 290)
                {
                    userAgent = userAgent.Substring(0, 290);
                }

                string? requestId = httpContext?.TraceIdentifier;

                string? beforeJson = null;
                if (beforeValues != null)
                {
                    beforeJson = beforeValues is string strBefore ? strBefore : JsonSerializer.Serialize(beforeValues, _jsonOptions);
                }

                string? afterJson = null;
                if (afterValues != null)
                {
                    afterJson = afterValues is string strAfter ? strAfter : JsonSerializer.Serialize(afterValues, _jsonOptions);
                }

                var log = new AuditLog
                {
                    ActorEmployeeId = actorId,
                    ActorEmail = actorEmail,
                    Action = action,
                    ModuleName = moduleName,
                    EntityName = entityName,
                    EntityId = entityId,
                    BeforeValuesJson = beforeJson,
                    AfterValuesJson = afterJson,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    RequestId = requestId,
                    Timestamp = DateTime.UtcNow,
                    Reason = reason
                };

                await _unitOfWork.AuditLogs.Add(log);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to record audit trail log for {Module}.{Entity} ({Action})", moduleName, entityName, action);
            }
        }

        public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(
            string? moduleName = null,
            string? entityName = null,
            string? action = null,
            long? actorEmployeeId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var allLogs = await _unitOfWork.AuditLogs.GetAll();
            var query = allLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(moduleName))
            {
                query = query.Where(l => string.Equals(l.ModuleName, moduleName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                query = query.Where(l => string.Equals(l.EntityName, entityName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(l => string.Equals(l.Action, action, StringComparison.OrdinalIgnoreCase));
            }

            if (actorEmployeeId.HasValue && actorEmployeeId.Value > 0)
            {
                query = query.Where(l => l.ActorEmployeeId == actorEmployeeId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.Timestamp <= toDate.Value);
            }

            return query.OrderByDescending(l => l.Timestamp).Take(500).ToList();
        }
    }
}
