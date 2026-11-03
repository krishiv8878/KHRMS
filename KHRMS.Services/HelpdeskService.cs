using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;

namespace KHRMS.Services
{
    public class HelpdeskService : IHelpdeskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContext;

        public HelpdeskService(IUnitOfWork unitOfWork, IUserContextService userContext)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
        }

        public async Task<List<HelpdeskTicketDto>> GetTicketsAsync(string? status = null, string? category = null, string? priority = null, long? requesterId = null, long? assigneeId = null)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isAdminOrHr = _userContext.IsAdmin() || _userContext.IsHR();

            var tickets = (await _unitOfWork.HelpdeskTickets.GetAll())
                .Where(t => !t.IsDeleted)
                .ToList();

            // Scoping: Non-admin employees by default see their own tickets unless specified
            if (!isAdminOrHr)
            {
                if (requesterId.HasValue && requesterId.Value > 0)
                {
                    tickets = tickets.Where(t => t.RequesterEmployeeId == requesterId.Value).ToList();
                }
                else if (assigneeId.HasValue && assigneeId.Value > 0)
                {
                    tickets = tickets.Where(t => t.AssignedToEmployeeId == assigneeId.Value).ToList();
                }
                else
                {
                    tickets = tickets.Where(t => t.RequesterEmployeeId == currentEmpId || t.AssignedToEmployeeId == currentEmpId).ToList();
                }
            }
            else
            {
                if (requesterId.HasValue && requesterId.Value > 0)
                {
                    tickets = tickets.Where(t => t.RequesterEmployeeId == requesterId.Value).ToList();
                }
                if (assigneeId.HasValue && assigneeId.Value > 0)
                {
                    tickets = tickets.Where(t => t.AssignedToEmployeeId == assigneeId.Value).ToList();
                }
            }

            if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                tickets = tickets.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                tickets = tickets.Where(t => t.DepartmentCategory.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(priority) && !priority.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                tickets = tickets.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();

            var result = new List<HelpdeskTicketDto>();
            foreach (var t in tickets.OrderByDescending(t => t.CreatedDate))
            {
                var req = allEmployees.FirstOrDefault(e => e.Id == t.RequesterEmployeeId);
                var ass = t.AssignedToEmployeeId.HasValue ? allEmployees.FirstOrDefault(e => e.Id == t.AssignedToEmployeeId.Value) : null;

                string slaStatus = "On Track";
                if (t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) || t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
                {
                    if (t.SlaDueTime.HasValue && t.ResolvedDate.HasValue && t.ResolvedDate.Value > t.SlaDueTime.Value)
                    {
                        slaStatus = "Resolved Past SLA";
                    }
                    else
                    {
                        slaStatus = "Resolved in SLA";
                    }
                }
                else if (t.SlaDueTime.HasValue && DateTime.UtcNow > t.SlaDueTime.Value)
                {
                    slaStatus = "Overdue";
                }

                result.Add(new HelpdeskTicketDto
                {
                    Id = t.Id,
                    TicketNumber = t.TicketNumber,
                    RequesterEmployeeId = t.RequesterEmployeeId,
                    RequesterName = req != null ? $"{req.FirstName} {req.LastName}".Trim() : $"EMP-{t.RequesterEmployeeId}",
                    RequesterCode = req?.EmployeeCode?.ToString() ?? "",
                    RequesterEmail = req?.WorkEmail ?? "",
                    RequesterDepartment = req?.Department ?? req?.Branch ?? "General",
                    DepartmentCategory = t.DepartmentCategory,
                    Subject = t.Subject,
                    Description = t.Description,
                    Priority = t.Priority,
                    Status = t.Status,
                    AssignedToEmployeeId = t.AssignedToEmployeeId,
                    AssigneeName = ass != null ? $"{ass.FirstName} {ass.LastName}".Trim() : "Unassigned",
                    SlaDueTime = t.SlaDueTime,
                    ResolvedDate = t.ResolvedDate,
                    ResolutionNotes = t.ResolutionNotes,
                    CreatedDate = t.CreatedDate ?? DateTime.UtcNow,
                    SlaStatus = slaStatus
                });
            }

            return result;
        }

        public async Task<HelpdeskTicketDto?> GetTicketByIdAsync(long id)
        {
            var t = await _unitOfWork.HelpdeskTickets.GetById(id);
            if (t == null || t.IsDeleted) return null;

            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();
            var req = allEmployees.FirstOrDefault(e => e.Id == t.RequesterEmployeeId);
            var ass = t.AssignedToEmployeeId.HasValue ? allEmployees.FirstOrDefault(e => e.Id == t.AssignedToEmployeeId.Value) : null;

            string slaStatus = "On Track";
            if (t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) || t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                slaStatus = (t.SlaDueTime.HasValue && t.ResolvedDate.HasValue && t.ResolvedDate.Value > t.SlaDueTime.Value) ? "Resolved Past SLA" : "Resolved in SLA";
            }
            else if (t.SlaDueTime.HasValue && DateTime.UtcNow > t.SlaDueTime.Value)
            {
                slaStatus = "Overdue";
            }

            return new HelpdeskTicketDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                RequesterEmployeeId = t.RequesterEmployeeId,
                RequesterName = req != null ? $"{req.FirstName} {req.LastName}".Trim() : $"EMP-{t.RequesterEmployeeId}",
                RequesterCode = req?.EmployeeCode?.ToString() ?? "",
                RequesterEmail = req?.WorkEmail ?? "",
                RequesterDepartment = req?.Department ?? req?.Branch ?? "General",
                DepartmentCategory = t.DepartmentCategory,
                Subject = t.Subject,
                Description = t.Description,
                Priority = t.Priority,
                Status = t.Status,
                AssignedToEmployeeId = t.AssignedToEmployeeId,
                AssigneeName = ass != null ? $"{ass.FirstName} {ass.LastName}".Trim() : "Unassigned",
                SlaDueTime = t.SlaDueTime,
                ResolvedDate = t.ResolvedDate,
                ResolutionNotes = t.ResolutionNotes,
                CreatedDate = t.CreatedDate ?? DateTime.UtcNow,
                SlaStatus = slaStatus
            };
        }

        public async Task<HelpdeskTicketDto> CreateTicketAsync(CreateHelpdeskTicketRequest request, long requesterEmployeeId)
        {
            var now = DateTime.UtcNow;
            var ticketNumber = $"HD-{now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            int slaHours = request.Priority switch
            {
                "Critical" => 4,
                "High" => 24,
                "Medium" => 48,
                "Low" => 72,
                _ => 48
            };

            var ticket = new HelpdeskTicket
            {
                TicketNumber = ticketNumber,
                RequesterEmployeeId = requesterEmployeeId,
                DepartmentCategory = string.IsNullOrWhiteSpace(request.DepartmentCategory) ? "General" : request.DepartmentCategory.Trim(),
                Subject = request.Subject.Trim(),
                Description = request.Description.Trim(),
                Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority.Trim(),
                Status = "Open",
                AssignedToEmployeeId = null,
                SlaDueTime = now.AddHours(slaHours),
                CreatedBy = (int)requesterEmployeeId,
                CreatedDate = now,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.HelpdeskTickets.Add(ticket);
            _unitOfWork.Save();

            return (await GetTicketByIdAsync(ticket.Id))!;
        }

        public async Task<HelpdeskTicketDto> UpdateTicketStatusAsync(UpdateHelpdeskTicketStatusRequest request, long currentUserId)
        {
            var ticket = await _unitOfWork.HelpdeskTickets.GetById(request.TicketId);
            if (ticket == null || ticket.IsDeleted) throw new KeyNotFoundException($"Helpdesk Ticket {request.TicketId} not found");

            ticket.Status = request.Status;
            if (request.AssignedToEmployeeId.HasValue && request.AssignedToEmployeeId.Value > 0)
            {
                ticket.AssignedToEmployeeId = request.AssignedToEmployeeId.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.ResolutionNotes))
            {
                ticket.ResolutionNotes = request.ResolutionNotes.Trim();
            }

            if (request.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) || request.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                if (!ticket.ResolvedDate.HasValue)
                {
                    ticket.ResolvedDate = DateTime.UtcNow;
                }
            }

            ticket.UpdatedBy = (int)currentUserId;
            ticket.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.HelpdeskTickets.Update(ticket);
            _unitOfWork.Save();

            return (await GetTicketByIdAsync(ticket.Id))!;
        }

        public async Task<HelpdeskMetricsDto> GetHelpdeskMetricsAsync(string? departmentCategory = null)
        {
            var tickets = (await _unitOfWork.HelpdeskTickets.GetAll())
                .Where(t => !t.IsDeleted)
                .ToList();

            if (!string.IsNullOrEmpty(departmentCategory) && !departmentCategory.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                tickets = tickets.Where(t => t.DepartmentCategory.Equals(departmentCategory, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            int total = tickets.Count;
            int open = tickets.Count(t => t.Status.Equals("Open", StringComparison.OrdinalIgnoreCase));
            int inProgress = tickets.Count(t => t.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase));
            int resolved = tickets.Count(t => t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase));
            int closed = tickets.Count(t => t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase));
            int critical = tickets.Count(t => t.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase) && !t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase));

            var resolvedTickets = tickets.Where(t => t.ResolvedDate.HasValue).ToList();
            int withinSla = 0;
            decimal totalHours = 0;

            foreach (var t in resolvedTickets)
            {
                if (t.SlaDueTime.HasValue && t.ResolvedDate.Value <= t.SlaDueTime.Value)
                {
                    withinSla++;
                }
                var created = t.CreatedDate ?? t.ResolvedDate.Value;
                var hours = (decimal)(t.ResolvedDate.Value - created).TotalHours;
                if (hours > 0) totalHours += hours;
            }

            decimal slaRate = resolvedTickets.Any() ? Math.Round((decimal)withinSla / resolvedTickets.Count * 100m, 1) : 100m;
            decimal avgHours = resolvedTickets.Any() ? Math.Round(totalHours / resolvedTickets.Count, 1) : 0m;

            return new HelpdeskMetricsDto
            {
                TotalTickets = total,
                OpenTickets = open,
                InProgressTickets = inProgress,
                ResolvedTickets = resolved,
                ClosedTickets = closed,
                CriticalTicketsCount = critical,
                SlaCompliancePercent = slaRate,
                AverageResolutionHours = avgHours
            };
        }
    }
}
