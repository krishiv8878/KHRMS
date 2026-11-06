using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Interfaces;
using KHRMS.Core.Models;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Moq;
using Xunit;

namespace KHRMS.UnitTest.ServiceTests
{
    public class HelpdeskServiceTest
    {
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<IHelpdeskTicketRepository> _ticketRepoMock;
        private readonly Mock<IHelpdeskTicketActivityRepository> _activityRepoMock;
        private readonly Mock<IAuditLogRepository> _auditRepoMock;
        private readonly Mock<IEmployeeRepository> _empRepoMock;
        private readonly Mock<IUserContextService> _userContextMock;
        private readonly HelpdeskService _service;

        private readonly List<HelpdeskTicket> _tickets;
        private readonly List<HelpdeskTicketActivity> _activities;
        private readonly List<AuditLog> _auditLogs;
        private readonly List<Employee> _employees;

        public HelpdeskServiceTest()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _ticketRepoMock = new Mock<IHelpdeskTicketRepository>();
            _activityRepoMock = new Mock<IHelpdeskTicketActivityRepository>();
            _auditRepoMock = new Mock<IAuditLogRepository>();
            _empRepoMock = new Mock<IEmployeeRepository>();
            _userContextMock = new Mock<IUserContextService>();

            _tickets = new List<HelpdeskTicket>();
            _activities = new List<HelpdeskTicketActivity>();
            _auditLogs = new List<AuditLog>();
            _employees = new List<Employee>();

            _ticketRepoMock.Setup(r => r.GetAll()).ReturnsAsync(_tickets);
            _ticketRepoMock.Setup(r => r.GetById(It.IsAny<long>()))
                .ReturnsAsync((long id) => _tickets.FirstOrDefault(t => t.Id == id));
            _ticketRepoMock.Setup(r => r.Add(It.IsAny<HelpdeskTicket>()))
                .Callback<HelpdeskTicket>(t =>
                {
                    if (t.Id == 0) t.Id = _tickets.Count + 1;
                    _tickets.Add(t);
                })
                .Returns(Task.CompletedTask);
            _ticketRepoMock.Setup(r => r.Update(It.IsAny<HelpdeskTicket>()))
                .Callback<HelpdeskTicket>(t =>
                {
                    var idx = _tickets.FindIndex(x => x.Id == t.Id);
                    if (idx >= 0) _tickets[idx] = t;
                });

            _activityRepoMock.Setup(r => r.GetAll()).ReturnsAsync(_activities);
            _activityRepoMock.Setup(r => r.Add(It.IsAny<HelpdeskTicketActivity>()))
                .Callback<HelpdeskTicketActivity>(a =>
                {
                    if (a.Id == 0) a.Id = _activities.Count + 1;
                    _activities.Add(a);
                })
                .Returns(Task.CompletedTask);

            _auditRepoMock.Setup(r => r.GetAll()).ReturnsAsync(_auditLogs);
            _auditRepoMock.Setup(r => r.Add(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(l =>
                {
                    if (l.Id == 0) l.Id = _auditLogs.Count + 1;
                    _auditLogs.Add(l);
                })
                .Returns(Task.CompletedTask);

            _empRepoMock.Setup(r => r.GetAll()).ReturnsAsync(_employees);
            _empRepoMock.Setup(r => r.GetById(It.IsAny<long>()))
                .ReturnsAsync((long id) => _employees.FirstOrDefault(e => e.Id == id));

            _uowMock.Setup(u => u.HelpdeskTickets).Returns(_ticketRepoMock.Object);
            _uowMock.Setup(u => u.HelpdeskTicketActivities).Returns(_activityRepoMock.Object);
            _uowMock.Setup(u => u.AuditLogs).Returns(_auditRepoMock.Object);
            _uowMock.Setup(u => u.Employees).Returns(_empRepoMock.Object);
            _uowMock.Setup(u => u.Save()).Returns(1);

            _service = new HelpdeskService(_uowMock.Object, _userContextMock.Object);

            // Seed sample employees
            _employees.Add(new Employee { Id = 101, FirstName = "Alice", LastName = "Requester", WorkEmail = "alice@khrms.local", ManagerId = 201 });
            _employees.Add(new Employee { Id = 102, FirstName = "Bob", LastName = "Peer", WorkEmail = "bob@khrms.local", ManagerId = 202 });
            _employees.Add(new Employee { Id = 201, FirstName = "Eve", LastName = "Manager", WorkEmail = "eve@khrms.local" });
            _employees.Add(new Employee { Id = 301, FirstName = "Dave", LastName = "ITSupport", WorkEmail = "dave@khrms.local" });
            _employees.Add(new Employee { Id = 999, FirstName = "Admin", LastName = "User", WorkEmail = "admin@khrms.local" });
        }

        [Fact]
        public async Task GetTicketsAsync_WhenStandardEmployee_ReturnsOnlyOwnTickets()
        {
            // Arrange
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Open" });
            _tickets.Add(new HelpdeskTicket { Id = 2, TicketNumber = "HD-2", RequesterEmployeeId = 102, Status = "Open" });

            _userContextMock.Setup(u => u.GetCurrentEmployeeId()).Returns(101);
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);
            _userContextMock.Setup(u => u.IsManager()).Returns(false);

            // Act
            var result = await _service.GetTicketsAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("HD-1", result[0].TicketNumber);
        }

        [Fact]
        public async Task GetTicketsAsync_WhenManager_ReturnsOwnTicketsAndDirectReportTickets()
        {
            // Arrange: Alice reports to Eve (Manager 201). Bob reports to 202.
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Open" });
            _tickets.Add(new HelpdeskTicket { Id = 2, TicketNumber = "HD-2", RequesterEmployeeId = 102, Status = "Open" });
            _tickets.Add(new HelpdeskTicket { Id = 3, TicketNumber = "HD-3", RequesterEmployeeId = 201, Status = "Open" });

            _userContextMock.Setup(u => u.GetCurrentEmployeeId()).Returns(201);
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);
            _userContextMock.Setup(u => u.IsManager()).Returns(true);

            // Act
            var result = await _service.GetTicketsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.TicketNumber == "HD-1");
            Assert.Contains(result, t => t.TicketNumber == "HD-3");
            Assert.DoesNotContain(result, t => t.TicketNumber == "HD-2");
        }

        [Fact]
        public async Task ClaimTicketAsync_WhenUnassigned_SuccessfullyClaimsAndSetsInProgress()
        {
            // Arrange
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Open", AssignedToEmployeeId = null });
            _userContextMock.Setup(u => u.GetCurrentEmployeeId()).Returns(301);

            // Act
            var updated = await _service.ClaimTicketAsync(1, 301);

            // Assert
            Assert.Equal(301, updated.AssignedToEmployeeId);
            Assert.Equal("In Progress", updated.Status);
            Assert.Contains(_activities, a => a.TicketId == 1 && a.ActivityType == "Assigned" && a.PerformedByEmployeeId == 301);
        }

        [Fact]
        public async Task ClaimTicketAsync_WhenAlreadyAssignedToOther_ThrowsInvalidOperationException()
        {
            // Arrange
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "In Progress", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.GetCurrentEmployeeId()).Returns(201);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ClaimTicketAsync(1, 201));
        }

        [Fact]
        public async Task AssignTicketAsync_WhenUnauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Open" });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new AssignTicketRequest { TicketId = 1, AssignToEmployeeId = 301 };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.AssignTicketAsync(req, 101));
        }

        [Fact]
        public async Task AssignTicketAsync_WhenAdmin_SuccessfullyAssignsAndLogsActivity()
        {
            // Arrange
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Open" });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(true);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new AssignTicketRequest { TicketId = 1, AssignToEmployeeId = 301, AssignmentNotes = "Urgent IT fix" };

            // Act
            var result = await _service.AssignTicketAsync(req, 999);

            // Assert
            Assert.Equal(301, result.AssignedToEmployeeId);
            Assert.Equal("In Progress", result.Status);
            Assert.Contains(_activities, a => a.TicketId == 1 && a.ActivityType == "Assigned" && a.AssignedToEmployeeId == 301);
        }

        [Fact]
        public async Task AssignTicketAsync_WhenManager_SuccessfullyAssignsTicket()
        {
            // Arrange
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Open" });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);
            _userContextMock.Setup(u => u.IsManager()).Returns(true);

            var req = new AssignTicketRequest { TicketId = 1, AssignToEmployeeId = 301, AssignmentNotes = "Assigned by Manager" };

            // Act
            var result = await _service.AssignTicketAsync(req, 201);

            // Assert
            Assert.Equal(301, result.AssignedToEmployeeId);
            Assert.Equal("In Progress", result.Status);
            Assert.Contains(_activities, a => a.TicketId == 1 && a.ActivityType == "Assigned" && a.AssignedToEmployeeId == 301);
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenUnassignedAndNonAdmin_ThrowsInvalidOperationException()
        {
            // Arrange: Unassigned ticket
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Open", AssignedToEmployeeId = null });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "In Progress" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateTicketStatusAsync(req, 201));
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenNonAssigneeAndNonAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange: Ticket assigned to Dave (301)
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "In Progress", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            // Another manager (201) tries to update status
            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "Resolved" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateTicketStatusAsync(req, 201));
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenAssignee_SuccessfullyUpdatesStatusAndLogsActivity()
        {
            // Arrange: Ticket assigned to Dave (301)
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "In Progress", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "Resolved", ResolutionNotes = "Password reset done" };

            // Act
            var result = await _service.UpdateTicketStatusAsync(req, 301);

            // Assert
            Assert.Equal("Resolved", result.Status);
            Assert.NotNull(result.ResolvedDate);
            Assert.Equal("Password reset done", result.ResolutionNotes);
            Assert.Contains(_activities, a => a.TicketId == 1 && a.ActivityType == "Resolution" && a.NewStatus == "Resolved");
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenRequesterCancels_Succeeds()
        {
            // Arrange
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Open", AssignedToEmployeeId = null });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "Cancelled", ResolutionNotes = "No longer needed" };

            // Act
            var result = await _service.UpdateTicketStatusAsync(req, 101);

            // Assert
            Assert.Equal("Cancelled", result.Status);
            Assert.Contains(_activities, a => a.TicketId == 1 && a.ActivityType == "StatusChange" && a.NewStatus == "Cancelled");
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenRequesterAttemptsInvalidTransition_ThrowsUnauthorizedAccessException()
        {
            // Arrange: Requester attempts to mark their own assigned ticket "Resolved" directly
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "In Progress", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "Resolved", ResolutionNotes = "I solved it myself" };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateTicketStatusAsync(req, 101));
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenRequesterClosesResolvedTicket_Succeeds()
        {
            // Arrange: Ticket was Resolved by support
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Resolved", AssignedToEmployeeId = 301, ResolvedDate = DateTime.UtcNow });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "Closed", ResolutionNotes = "Confirmed working fine" };

            // Act
            var result = await _service.UpdateTicketStatusAsync(req, 101);

            // Assert
            Assert.Equal("Closed", result.Status);
            Assert.Contains(_activities, a => a.TicketId == 1 && a.ActivityType == "StatusChange" && a.NewStatus == "Closed");
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenRequesterReopensResolvedTicket_Succeeds()
        {
            // Arrange: Ticket was Resolved by support but requester reopens it
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Resolved", AssignedToEmployeeId = 301, ResolvedDate = DateTime.UtcNow });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "In Progress", ResolutionNotes = "Issue reoccurred after reboot" };

            // Act
            var result = await _service.UpdateTicketStatusAsync(req, 101);

            // Assert
            Assert.Equal("In Progress", result.Status);
            Assert.Null(result.ResolvedDate);
            Assert.Contains(_activities, a => a.TicketId == 1 && a.ActivityType == "StatusChange" && a.NewStatus == "In Progress");
        }

        [Fact]
        public async Task AddCommentAsync_WhenAuthorized_RecordsCommentActivity()
        {
            // Arrange: Ticket created by Alice (101), assigned to Dave (301)
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "In Progress", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new AddTicketCommentRequest { TicketId = 1, Comment = "Can you check my access again?" };

            // Act
            var activity = await _service.AddCommentAsync(req, 101);

            // Assert
            Assert.Equal("Comment", activity.ActivityType);
            Assert.Equal("Can you check my access again?", activity.Comment);
            Assert.Contains(_activities, a => a.TicketId == 1 && a.ActivityType == "Comment" && a.Comment == "Can you check my access again?");
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenAssigneeAttemptsToModifyClosedTicket_ThrowsInvalidOperationException()
        {
            // Arrange: Ticket is Closed
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Closed", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "In Progress", ResolutionNotes = "Reopening as agent" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateTicketStatusAsync(req, 301));
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenAssigneeAttemptsToModifyResolvedTicket_ThrowsInvalidOperationException()
        {
            // Arrange: Ticket is Resolved and awaiting requester confirmation
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Resolved", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "In Progress", ResolutionNotes = "Changing back to in progress" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateTicketStatusAsync(req, 301));
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenAssigneeAttemptsToCloseTicketDirectly_ThrowsInvalidOperationException()
        {
            // Arrange: Ticket is In Progress
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "In Progress", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "Closed", ResolutionNotes = "Closing ticket myself" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateTicketStatusAsync(req, 301));
        }

        [Fact]
        public async Task UpdateTicketStatusAsync_WhenAssigneeAttemptsToModifyCancelledTicket_ThrowsInvalidOperationException()
        {
            // Arrange: Ticket is Cancelled
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Cancelled", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(false);
            _userContextMock.Setup(u => u.IsHR()).Returns(false);

            var req = new UpdateHelpdeskTicketStatusRequest { TicketId = 1, Status = "In Progress" };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateTicketStatusAsync(req, 301));
        }

        [Fact]
        public async Task AssignTicketAsync_WhenTicketIsClosed_ThrowsInvalidOperationException()
        {
            // Arrange: Ticket is Closed
            _tickets.Add(new HelpdeskTicket { Id = 1, TicketNumber = "HD-1", RequesterEmployeeId = 101, Status = "Closed", AssignedToEmployeeId = 301 });
            _userContextMock.Setup(u => u.IsAdmin()).Returns(true);

            var req = new AssignTicketRequest { TicketId = 1, AssignToEmployeeId = 999 };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignTicketAsync(req, 999));
        }
    }
}
