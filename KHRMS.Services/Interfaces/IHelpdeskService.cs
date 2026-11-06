using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<HelpdeskTicketDto> AssignTicketAsync(AssignTicketRequest request, long currentUserId);
        Task<HelpdeskTicketDto> ClaimTicketAsync(long ticketId, long currentUserId);
        Task<HelpdeskTicketActivityDto> AddCommentAsync(AddTicketCommentRequest request, long currentUserId);
        Task<List<HelpdeskTicketActivityDto>> GetTicketActivitiesAsync(long ticketId);
        Task<List<EmployeeLookupDto>> GetSupportAgentsAsync(string? category = null);
        Task<HelpdeskMetricsDto> GetHelpdeskMetricsAsync(string? departmentCategory = null);
    }
}
