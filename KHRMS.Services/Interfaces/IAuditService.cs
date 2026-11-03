using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KHRMS.Core.Models;

namespace KHRMS.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string moduleName, string entityName, string entityId, string action, object? beforeValues = null, object? afterValues = null, string? reason = null);
        Task<IEnumerable<AuditLog>> GetAuditLogsAsync(string? moduleName = null, string? entityName = null, string? action = null, long? actorEmployeeId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
