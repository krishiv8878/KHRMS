using KHRMS.Core.Models;
using KHRMS.Services.Request;

namespace KHRMS.Services.Interfaces
{
    public interface IHelpdeskService
    {
        Task<List<HelpdeskTicketDto>> GetTicketsAsync(string? status = null, string? category = null, string? priority = null, long? requesterId = null, long? assigneeId = null);
        Task<HelpdeskTicketDto?> GetTicketByIdAsync(long id);
        Task<HelpdeskTicketDto> CreateTicketAsync(CreateHelpdeskTicketRequest request, long requesterEmployeeId);
        Task<HelpdeskTicketDto> UpdateTicketStatusAsync(UpdateHelpdeskTicketStatusRequest request, long currentUserId);
        Task<HelpdeskMetricsDto> GetHelpdeskMetricsAsync(string? departmentCategory = null);
    }
}
