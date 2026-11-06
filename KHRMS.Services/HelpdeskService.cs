using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Serilog;

namespace KHRMS.Services
{
    public class HelpdeskService : IHelpdeskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContext;

        public HelpdeskService(IUnitOfWork unitOfWork, IUserContextService userContext)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        public async Task<List<HelpdeskTicketDto>> GetTicketsAsync(string? status = null, string? category = null, string? priority = null, long? requesterId = null, long? assigneeId = null)
        {
            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isAdmin = _userContext.IsAdmin();
            bool isHR = _userContext.IsHR();
            bool isManager = _userContext.IsManager();
            bool canAssign = isAdmin || isHR || isManager;

            var tickets = (await _unitOfWork.HelpdeskTickets.GetAll())
                .Where(t => !t.IsDeleted)
                .ToList();

            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();

            // 1. Role-based baseline visibility scoping
            if (isAdmin || isHR)
            {
                // Admins and HR have full visibility into the queue
                if (requesterId.HasValue && requesterId.Value > 0)
                {
                    tickets = tickets.Where(t => t.RequesterEmployeeId == requesterId.Value).ToList();
                }

                if (assigneeId.HasValue)
                {
                    if (assigneeId.Value == -1) // Unassigned queue filter
                    {
                        tickets = tickets.Where(t => !t.AssignedToEmployeeId.HasValue || t.AssignedToEmployeeId.Value == 0).ToList();
                    }
                    else if (assigneeId.Value > 0)
                    {
                        tickets = tickets.Where(t => t.AssignedToEmployeeId == assigneeId.Value).ToList();
                    }
                }
            }
            else if (isManager)
            {
                // Managers see their own tickets, tickets assigned to them, and tickets from their direct reports
                var subordinateIds = allEmployees.Where(e => e.ManagerId == currentEmpId).Select(e => e.Id).ToHashSet();

                tickets = tickets.Where(t =>
                    t.RequesterEmployeeId == currentEmpId ||
                    (t.AssignedToEmployeeId.HasValue && t.AssignedToEmployeeId.Value == currentEmpId) ||
                    subordinateIds.Contains(t.RequesterEmployeeId)).ToList();

                if (requesterId.HasValue && requesterId.Value > 0)
                {
                    tickets = tickets.Where(t => t.RequesterEmployeeId == requesterId.Value).ToList();
                }

                if (assigneeId.HasValue)
                {
                    if (assigneeId.Value == -1)
                    {
                        tickets = tickets.Where(t => !t.AssignedToEmployeeId.HasValue || t.AssignedToEmployeeId.Value == 0).ToList();
                    }
                    else if (assigneeId.Value > 0)
                    {
                        tickets = tickets.Where(t => t.AssignedToEmployeeId == assigneeId.Value).ToList();
                    }
                }
            }
            else
            {
                // Non-manager employee only sees tickets they requested or tickets directly assigned to them
                tickets = tickets.Where(t =>
                    t.RequesterEmployeeId == currentEmpId ||
                    (t.AssignedToEmployeeId.HasValue && t.AssignedToEmployeeId.Value == currentEmpId)).ToList();

                if (requesterId.HasValue && requesterId.Value == currentEmpId)
                {
                    tickets = tickets.Where(t => t.RequesterEmployeeId == requesterId.Value).ToList();
                }
                else if (assigneeId.HasValue && assigneeId.Value == currentEmpId)
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

            var result = new List<HelpdeskTicketDto>();
            foreach (var t in tickets.OrderByDescending(t => t.CreatedDate))
            {
                var req = allEmployees.FirstOrDefault(e => e.Id == t.RequesterEmployeeId);
                var assignedemp = t.AssignedToEmployeeId.HasValue ? allEmployees.FirstOrDefault(e => e.Id == t.AssignedToEmployeeId.Value) : null;

                string slaStatus = "On Track";
                if (t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) || t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
                {
                    slaStatus = (t.SlaDueTime.HasValue && t.ResolvedDate.HasValue && t.ResolvedDate.Value > t.SlaDueTime.Value) ? "Resolved Past SLA" : "Resolved in SLA";
                }
                else if (t.SlaDueTime.HasValue && DateTime.UtcNow > t.SlaDueTime.Value)
                {
                    slaStatus = "Overdue";
                }

                bool isUnassigned = !t.AssignedToEmployeeId.HasValue || t.AssignedToEmployeeId.Value == 0;
                bool isAssignee = !isUnassigned && t.AssignedToEmployeeId.Value == currentEmpId;
                bool isRequester = t.RequesterEmployeeId == currentEmpId;
                bool canAssignTicket = canAssign &&
                                       !t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) &&
                                       !t.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);

                bool isTicketActive = !t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) &&
                                      !t.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) &&
                                      !t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase);

                bool canUpdateStatus = isAdmin
                    ? !t.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
                    : (isAssignee && !isUnassigned && isTicketActive);

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
                    AssigneeName = assignedemp != null ? $"{assignedemp.FirstName} {assignedemp.LastName}".Trim() : "Unassigned",
                    SlaDueTime = t.SlaDueTime,
                    ResolvedDate = t.ResolvedDate,
                    ResolutionNotes = t.ResolutionNotes,
                    CreatedDate = t.CreatedDate ?? DateTime.UtcNow,
                    SlaStatus = slaStatus,
                    CanCurrentUserUpdateStatus = canUpdateStatus,
                    CanCurrentUserAssign = canAssignTicket,
                    CanCurrentUserClaim = isUnassigned && !t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) && !t.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && canAssign,
                    CanCurrentUserComment = isAdmin || isHR || isManager || isAssignee || isRequester,
                    CanRequesterCloseOrReopen = isRequester && t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase),
                    CanRequesterCancel = isRequester && (t.Status.Equals("Open", StringComparison.OrdinalIgnoreCase) || t.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase))
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
            var assignedemp = t.AssignedToEmployeeId.HasValue ? allEmployees.FirstOrDefault(e => e.Id == t.AssignedToEmployeeId.Value) : null;

            string slaStatus = "On Track";
            if (t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) || t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                slaStatus = (t.SlaDueTime.HasValue && t.ResolvedDate.HasValue && t.ResolvedDate.Value > t.SlaDueTime.Value) ? "Resolved Past SLA" : "Resolved in SLA";
            }
            else if (t.SlaDueTime.HasValue && DateTime.UtcNow > t.SlaDueTime.Value)
            {
                slaStatus = "Overdue";
            }

            long currentEmpId = _userContext.GetCurrentEmployeeId();
            bool isAdmin = _userContext.IsAdmin();
            bool isHR = _userContext.IsHR();
            bool isManager = _userContext.IsManager();
            bool canAssign = (isAdmin || isHR || isManager) &&
                             !t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) &&
                             !t.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);

            bool isUnassigned = !t.AssignedToEmployeeId.HasValue || t.AssignedToEmployeeId.Value == 0;
            bool isAssignee = !isUnassigned && t.AssignedToEmployeeId.Value == currentEmpId;
            bool isRequester = t.RequesterEmployeeId == currentEmpId;

            bool isTicketActive = !t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) &&
                                  !t.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) &&
                                  !t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase);

            bool canUpdateStatus = isAdmin
                ? !t.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
                : (isAssignee && !isUnassigned && isTicketActive);

            var activities = await GetTicketActivitiesAsync(id);

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
                AssigneeName = assignedemp != null ? $"{assignedemp.FirstName} {assignedemp.LastName}".Trim() : "Unassigned",
                SlaDueTime = t.SlaDueTime,
                ResolvedDate = t.ResolvedDate,
                ResolutionNotes = t.ResolutionNotes,
                CreatedDate = t.CreatedDate ?? DateTime.UtcNow,
                SlaStatus = slaStatus,
                Activities = activities,
                CanCurrentUserUpdateStatus = canUpdateStatus,
                CanCurrentUserAssign = canAssign,
                CanCurrentUserClaim = isUnassigned && !t.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) && !t.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && canAssign,
                CanCurrentUserComment = isAdmin || isHR || isManager || isAssignee || isRequester,
                CanRequesterCloseOrReopen = isRequester && t.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase),
                CanRequesterCancel = isRequester && (t.Status.Equals("Open", StringComparison.OrdinalIgnoreCase) || t.Status.Equals("In Progress", StringComparison.OrdinalIgnoreCase))
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

            long? initialAssignee = null;
            if (request.AssignedToEmployeeId.HasValue && request.AssignedToEmployeeId.Value > 0)
            {
                if (_userContext.IsAdmin() || _userContext.IsHR() || _userContext.IsManager())
                {
                    initialAssignee = request.AssignedToEmployeeId.Value;
                }
            }

            var ticket = new HelpdeskTicket
            {
                TicketNumber = ticketNumber,
                RequesterEmployeeId = requesterEmployeeId,
                DepartmentCategory = string.IsNullOrWhiteSpace(request.DepartmentCategory) ? "General" : request.DepartmentCategory.Trim(),
                Subject = request.Subject.Trim(),
                Description = request.Description.Trim(),
                Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Medium" : request.Priority.Trim(),
                Status = initialAssignee.HasValue ? "In Progress" : "Open",
                AssignedToEmployeeId = initialAssignee,
                SlaDueTime = now.AddHours(slaHours),
                CreatedBy = (int)requesterEmployeeId,
                CreatedDate = now,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.HelpdeskTickets.Add(ticket);
            _unitOfWork.Save();

            // Log Creation Activity
            await LogActivityAsync(
                ticket.Id,
                requesterEmployeeId,
                "Created",
                null,
                ticket.Status,
                initialAssignee,
                $"Ticket created with category '{ticket.DepartmentCategory}' and priority '{ticket.Priority}'."
            );

            if (initialAssignee.HasValue)
            {
                var assignee = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(e => e.Id == initialAssignee.Value);
                string assigneeName = assignee != null ? $"{assignee.FirstName} {assignee.LastName}".Trim() : $"EMP-{initialAssignee.Value}";
                await LogActivityAsync(
                    ticket.Id,
                    requesterEmployeeId,
                    "Assigned",
                    "Open",
                    "In Progress",
                    initialAssignee,
                    $"Assigned to {assigneeName} upon creation."
                );
            }

            return (await GetTicketByIdAsync(ticket.Id))!;
        }

        public async Task<HelpdeskTicketDto> AssignTicketAsync(AssignTicketRequest request, long currentUserId)
        {
            bool isAdmin = _userContext.IsAdmin();
            bool isHR = _userContext.IsHR();
            bool isManager = _userContext.IsManager();
            if (!isAdmin && !isHR && !isManager)
            {
                throw new UnauthorizedAccessException("Only Administrators, HR staff, or Managers can assign helpdesk tickets.");
            }

            var ticket = await _unitOfWork.HelpdeskTickets.GetById(request.TicketId);
            if (ticket == null || ticket.IsDeleted) throw new KeyNotFoundException($"Helpdesk ticket {request.TicketId} not found");

            if (ticket.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) ||
                ticket.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot assign or reassign a closed or cancelled ticket.");
            }

            var assignee = await _unitOfWork.Employees.GetById(request.AssignToEmployeeId);
            if (assignee == null || assignee.IsDeleted) throw new ArgumentException($"Assignee employee ID {request.AssignToEmployeeId} not found.");

            string oldStatus = ticket.Status;

            ticket.AssignedToEmployeeId = request.AssignToEmployeeId;
            if (ticket.Status.Equals("Open", StringComparison.OrdinalIgnoreCase))
            {
                ticket.Status = "In Progress";
            }
            ticket.UpdatedBy = (int)currentUserId;
            ticket.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.HelpdeskTickets.Update(ticket);
            _unitOfWork.Save();

            string assigneeName = $"{assignee.FirstName} {assignee.LastName}".Trim();
            string note = string.IsNullOrWhiteSpace(request.AssignmentNotes)
                ? $"Ticket assigned to {assigneeName}."
                : $"Ticket assigned to {assigneeName}. Note: {request.AssignmentNotes.Trim()}";

            await LogActivityAsync(
                ticket.Id,
                currentUserId,
                "Assigned",
                oldStatus,
                ticket.Status,
                request.AssignToEmployeeId,
                note
            );

            return (await GetTicketByIdAsync(ticket.Id))!;
        }

        public async Task<HelpdeskTicketDto> ClaimTicketAsync(long ticketId, long currentUserId)
        {
            var ticket = await _unitOfWork.HelpdeskTickets.GetById(ticketId);
            if (ticket == null || ticket.IsDeleted) throw new KeyNotFoundException($"Helpdesk ticket {ticketId} not found");

            if (ticket.AssignedToEmployeeId.HasValue && ticket.AssignedToEmployeeId.Value > 0)
            {
                if (ticket.AssignedToEmployeeId.Value == currentUserId)
                {
                    return (await GetTicketByIdAsync(ticket.Id))!;
                }
                throw new InvalidOperationException("This ticket has already been claimed or assigned to another agent.");
            }

            if (ticket.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase) || ticket.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Cannot claim a closed or cancelled ticket.");
            }

            string oldStatus = ticket.Status;
            ticket.AssignedToEmployeeId = currentUserId;
            if (ticket.Status.Equals("Open", StringComparison.OrdinalIgnoreCase))
            {
                ticket.Status = "In Progress";
            }
            ticket.UpdatedBy = (int)currentUserId;
            ticket.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.HelpdeskTickets.Update(ticket);
            _unitOfWork.Save();

            await LogActivityAsync(
                ticket.Id,
                currentUserId,
                "Assigned",
                oldStatus,
                ticket.Status,
                currentUserId,
                "Ticket claimed by support agent."
            );

            return (await GetTicketByIdAsync(ticket.Id))!;
        }

        public async Task<HelpdeskTicketDto> UpdateTicketStatusAsync(UpdateHelpdeskTicketStatusRequest request, long currentUserId)
        {
            var ticket = await _unitOfWork.HelpdeskTickets.GetById(request.TicketId);
            if (ticket == null || ticket.IsDeleted) throw new KeyNotFoundException($"Helpdesk ticket {request.TicketId} not found");

            bool isAdmin = _userContext.IsAdmin();
            bool isHR = _userContext.IsHR();
            bool isManager = _userContext.IsManager();
            bool isAssignee = ticket.AssignedToEmployeeId.HasValue && ticket.AssignedToEmployeeId.Value == currentUserId;
            bool isRequester = ticket.RequesterEmployeeId == currentUserId;

            string currentStatus = ticket.Status;
            string requestedStatus = request.Status.Trim();

            // 1. Unassigned Ticket Handling
            if (!ticket.AssignedToEmployeeId.HasValue || ticket.AssignedToEmployeeId.Value <= 0)
            {
                if (isRequester && string.Equals(requestedStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    // Allowed: Requester cancelling unassigned ticket
                }
                else if ((isAdmin || isHR || isManager) &&
                         (request.AssignedToEmployeeId.HasValue && request.AssignedToEmployeeId.Value > 0 || requestedStatus.Equals("In Progress", StringComparison.OrdinalIgnoreCase)))
                {
                    // Admin/HR/Manager moving to In Progress auto-assigns or assigns to requested agent
                    ticket.AssignedToEmployeeId = (request.AssignedToEmployeeId.HasValue && request.AssignedToEmployeeId.Value > 0)
                        ? request.AssignedToEmployeeId.Value
                        : currentUserId;
                    isAssignee = ticket.AssignedToEmployeeId.Value == currentUserId;
                }
                else
                {
                    throw new InvalidOperationException("Ticket must be claimed or assigned to a support agent before updating its progress or resolving it.");
                }
            }

            // 2. Strict Role & Terminal State Guards
            if (isRequester && !isAdmin)
            {
                // Requester transitions:
                if (string.Equals(requestedStatus, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                    (string.Equals(currentStatus, "Open", StringComparison.OrdinalIgnoreCase) || string.Equals(currentStatus, "In Progress", StringComparison.OrdinalIgnoreCase)))
                {
                    // Allowed: Requester cancelling open ticket
                }
                else if (string.Equals(requestedStatus, "Closed", StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(currentStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
                {
                    // Allowed: Requester accepting resolution
                }
                else if (string.Equals(requestedStatus, "In Progress", StringComparison.OrdinalIgnoreCase) &&
                         (string.Equals(currentStatus, "Resolved", StringComparison.OrdinalIgnoreCase) || string.Equals(currentStatus, "Closed", StringComparison.OrdinalIgnoreCase)))
                {
                    // Allowed: Requester reopening ticket
                }
                else if (!isAssignee)
                {
                    throw new UnauthorizedAccessException("As the requester, you can only Cancel your open request or Accept/Reopen a resolved ticket.");
                }
                else
                {
                    throw new InvalidOperationException("Invalid status transition for this ticket.");
                }
            }
            else if (!isAdmin && !isAssignee)
            {
                throw new UnauthorizedAccessException("Only the assigned support agent or an administrator can update the status of this ticket.");
            }
            else if (isAssignee && !isAdmin)
            {
                // 3. Strict Assigned Support Agent Guards
                if (string.Equals(currentStatus, "Closed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("This ticket has been confirmed and closed by the requester. Assigned agents cannot change the status of a closed ticket.");
                }

                if (string.Equals(currentStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("This ticket has been cancelled by the requester and cannot be modified.");
                }

                if (string.Equals(currentStatus, "Resolved", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("This ticket is marked as Resolved and is awaiting confirmation from the requester. The assigned support agent cannot modify the status until the requester reviews it.");
                }

                if (string.Equals(requestedStatus, "Closed", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Support agents cannot close tickets directly. Please transition the ticket to 'Resolved' so the requester can confirm resolution and close it.");
                }

                if (string.Equals(requestedStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Only the requester or an administrator can cancel a ticket.");
                }
            }

            string oldStatus = ticket.Status;
            ticket.Status = request.Status;

            if (request.AssignedToEmployeeId.HasValue && request.AssignedToEmployeeId.Value > 0 && (isAdmin || isHR || isManager))
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
            else if (oldStatus.Equals("Resolved", StringComparison.OrdinalIgnoreCase) && !request.Status.Equals("Closed", StringComparison.OrdinalIgnoreCase))
            {
                ticket.ResolvedDate = null;
            }

            ticket.UpdatedBy = (int)currentUserId;
            ticket.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.HelpdeskTickets.Update(ticket);
            _unitOfWork.Save();

            string activityType = request.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase) ? "Resolution" : "StatusChange";
            string comment = !string.IsNullOrWhiteSpace(request.ResolutionNotes)
                ? request.ResolutionNotes.Trim()
                : $"Status updated from '{oldStatus}' to '{request.Status}'.";

            await LogActivityAsync(
                ticket.Id,
                currentUserId,
                activityType,
                oldStatus,
                request.Status,
                ticket.AssignedToEmployeeId,
                comment
            );

            return (await GetTicketByIdAsync(ticket.Id))!;
        }

        public async Task<HelpdeskTicketActivityDto> AddCommentAsync(AddTicketCommentRequest request, long currentUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Comment))
            {
                throw new ArgumentException("Comment cannot be empty.", nameof(request.Comment));
            }

            var ticket = await _unitOfWork.HelpdeskTickets.GetById(request.TicketId);
            if (ticket == null || ticket.IsDeleted) throw new KeyNotFoundException($"Helpdesk ticket {request.TicketId} not found");

            bool isAdmin = _userContext.IsAdmin();
            bool isHR = _userContext.IsHR();
            bool isManager = _userContext.IsManager();
            bool isAssignee = ticket.AssignedToEmployeeId.HasValue && ticket.AssignedToEmployeeId.Value == currentUserId;
            bool isRequester = ticket.RequesterEmployeeId == currentUserId;

            if (!isAdmin && !isHR && !isManager && !isAssignee && !isRequester)
            {
                throw new UnauthorizedAccessException("You do not have permission to comment on this ticket.");
            }

            await LogActivityAsync(
                ticket.Id,
                currentUserId,
                "Comment",
                ticket.Status,
                ticket.Status,
                ticket.AssignedToEmployeeId,
                request.Comment.Trim()
            );

            var user = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(e => e.Id == currentUserId);

            return new HelpdeskTicketActivityDto
            {
                TicketId = ticket.Id,
                ActivityType = "Comment",
                OldStatus = ticket.Status,
                NewStatus = ticket.Status,
                AssignedToEmployeeId = ticket.AssignedToEmployeeId,
                PerformedByEmployeeId = currentUserId,
                PerformedByName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : $"EMP-{currentUserId}",
                PerformedByEmail = user?.WorkEmail ?? "",
                Comment = request.Comment.Trim(),
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<List<HelpdeskTicketActivityDto>> GetTicketActivitiesAsync(long ticketId)
        {
            var allEmployees = (await _unitOfWork.Employees.GetAll()).ToList();
            var result = new List<HelpdeskTicketActivityDto>();

            // 1. Try fetching from HelpdeskTicketActivities table
            try
            {
                var activities = (await _unitOfWork.HelpdeskTicketActivities.GetAll())
                    .Where(a => a.TicketId == ticketId && !a.IsDeleted)
                    .OrderBy(a => a.CreatedDate)
                    .ToList();

                if (activities.Any())
                {
                    foreach (var act in activities)
                    {
                        var actor = allEmployees.FirstOrDefault(e => e.Id == act.PerformedByEmployeeId);
                        var assignee = act.AssignedToEmployeeId.HasValue ? allEmployees.FirstOrDefault(e => e.Id == act.AssignedToEmployeeId.Value) : null;
                        result.Add(new HelpdeskTicketActivityDto
                        {
                            Id = act.Id,
                            TicketId = act.TicketId,
                            ActivityType = act.ActivityType,
                            OldStatus = act.OldStatus,
                            NewStatus = act.NewStatus,
                            AssignedToEmployeeId = act.AssignedToEmployeeId,
                            AssigneeName = assignee != null ? $"{assignee.FirstName} {assignee.LastName}".Trim() : null,
                            PerformedByEmployeeId = act.PerformedByEmployeeId,
                            PerformedByName = actor != null ? $"{actor.FirstName} {actor.LastName}".Trim() : $"EMP-{act.PerformedByEmployeeId}",
                            PerformedByEmail = actor?.WorkEmail ?? "",
                            Comment = act.Comment,
                            Timestamp = act.CreatedDate ?? DateTime.UtcNow
                        });
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "HelpdeskService - Error reading HelpdeskTicketActivities. Falling back to AuditLogs.");
            }

            // 2. Fallback to AuditLogs
            try
            {
                var auditLogs = (await _unitOfWork.AuditLogs.GetAll())
                    .Where(a => a.ModuleName == "Helpdesk" && a.EntityName == "HelpdeskTicket" && a.EntityId == ticketId.ToString())
                    .OrderBy(a => a.Timestamp)
                    .ToList();

                foreach (var log in auditLogs)
                {
                    var actor = allEmployees.FirstOrDefault(e => e.Id == log.ActorEmployeeId);
                    result.Add(new HelpdeskTicketActivityDto
                    {
                        Id = log.Id,
                        TicketId = ticketId,
                        ActivityType = log.Action,
                        PerformedByEmployeeId = log.ActorEmployeeId,
                        PerformedByName = actor != null ? $"{actor.FirstName} {actor.LastName}".Trim() : (string.IsNullOrEmpty(log.ActorEmail) ? $"User {log.ActorEmployeeId}" : log.ActorEmail),
                        PerformedByEmail = log.ActorEmail,
                        Comment = log.Reason ?? log.Action,
                        Timestamp = log.Timestamp
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskService - Error loading audit logs for ticket {TicketId}", ticketId);
            }

            return result;
        }

        public async Task<List<EmployeeLookupDto>> GetSupportAgentsAsync(string? category = null)
        {
            var employees = (await _unitOfWork.Employees.GetAll())
                .Where(e => !e.IsDeleted && (e.EmploymentStatus == null || e.EmploymentStatus == "Active"))
                .ToList();

            var roleMappings = (await _unitOfWork.EmployeeRoleMappings.GetAll())
                .Where(m => m.IsActive && !m.IsDeleted)
                .ToList();

            var roles = (await _unitOfWork.RoleMaster.GetAll())
                .Where(r => r.IsActive && !r.IsDeleted)
                .ToList();

            var supportRoleIds = roles
                .Where(r => r.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("System Admin", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("HR", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("HR Operations", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Equals("Manager", StringComparison.OrdinalIgnoreCase) ||
                            r.RoleName.Contains("Support", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Id)
                .ToHashSet();

            var privilegedEmployeeIds = roleMappings
                .Where(m => supportRoleIds.Contains(m.RoleId))
                .Select(m => m.EmployeeId)
                .ToHashSet();

            var supportAgents = employees.Where(e =>
            {
                if (privilegedEmployeeIds.Contains(e.Id)) return true;
                var dept = (e.Department ?? "").ToLowerInvariant();
                var desig = (e.Designation ?? "").ToLowerInvariant();
                return dept.Contains("it") || dept.Contains("hr") || dept.Contains("human resource") ||
                       dept.Contains("admin") || dept.Contains("support") || dept.Contains("operation") ||
                       desig.Contains("support") || desig.Contains("admin") || desig.Contains("hr") || 
                       desig.Contains("lead") || desig.Contains("manager");
            }).ToList();

            if (!supportAgents.Any())
            {
                supportAgents = employees;
            }

            return supportAgents.Select(e => new EmployeeLookupDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Designation = e.Designation,
                DesignationId = e.DesignationId,
                ProfileImage = e.ProfileImage,
                EmailAddress = e.WorkEmail,
                DepartmentId = e.DepartmentId,
                Department = e.Department,
                ManagerId = e.ManagerId,
                IsActive = true
            }).OrderBy(e => e.FullName).ToList();
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

        private async Task LogActivityAsync(
            long ticketId,
            long performedByUserId,
            string activityType,
            string? oldStatus,
            string? newStatus,
            long? assignedToEmployeeId,
            string comment)
        {
            var now = DateTime.UtcNow;

            try
            {
                var activity = new HelpdeskTicketActivity
                {
                    TicketId = ticketId,
                    PerformedByEmployeeId = performedByUserId,
                    ActivityType = activityType,
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    AssignedToEmployeeId = assignedToEmployeeId,
                    Comment = comment,
                    CreatedBy = (int)performedByUserId,
                    CreatedDate = now,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.HelpdeskTicketActivities.Add(activity);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "HelpdeskService - HelpdeskTicketActivities table may not exist yet in database or failed to save activity. Falling back to AuditLogs.");
            }

            // Always mirror to AuditLogs for complete enterprise audit trail
            try
            {
                var actor = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(e => e.Id == performedByUserId);
                var audit = new AuditLog
                {
                    ActorEmployeeId = performedByUserId,
                    ActorEmail = actor?.WorkEmail ?? $"user_{performedByUserId}@khrms.local",
                    Action = activityType,
                    ModuleName = "Helpdesk",
                    EntityName = "HelpdeskTicket",
                    EntityId = ticketId.ToString(),
                    BeforeValuesJson = oldStatus != null ? JsonSerializer.Serialize(new { Status = oldStatus }) : null,
                    AfterValuesJson = JsonSerializer.Serialize(new { Status = newStatus, AssignedTo = assignedToEmployeeId, Comment = comment }),
                    Reason = comment,
                    Timestamp = now
                };
                await _unitOfWork.AuditLogs.Add(audit);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "HelpdeskService - Failed to write audit log for ticket {TicketId}", ticketId);
            }
        }
    }
}
