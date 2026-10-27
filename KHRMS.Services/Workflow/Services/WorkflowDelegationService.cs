using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Models;

namespace KHRMS.Services.Workflow.Services
{
    public class WorkflowDelegationService : IWorkflowDelegationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkflowDelegationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<WorkflowDelegationDTO>> GetMyDelegationsAsync(long employeeId)
        {
            var delegations = (await _unitOfWork.WorkflowDelegations.GetAll())
                .Where(d => d.FromEmployeeId == employeeId && d.IsActive && !d.IsDeleted)
                .OrderByDescending(d => d.StartDate)
                .ToList();

            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            return delegations.Select(d => new WorkflowDelegationDTO
            {
                Id = d.Id,
                FromEmployeeId = d.FromEmployeeId,
                FromEmployeeName = employees.TryGetValue(d.FromEmployeeId, out var fName) ? fName : "Me",
                ToEmployeeId = d.ToEmployeeId,
                ToEmployeeName = employees.TryGetValue(d.ToEmployeeId, out var tName) ? tName : $"Employee #{d.ToEmployeeId}",
                StartDate = d.StartDate,
                EndDate = d.EndDate,
                RequestType = d.RequestType ?? "All",
                Reason = d.Reason,
                IsActive = d.IsActive && d.EndDate >= DateTime.UtcNow
            }).ToList();
        }

        public async Task<WorkflowDelegationDTO> CreateDelegationAsync(long fromEmployeeId, CreateWorkflowDelegationRequest request)
        {
            if (request.ToEmployeeId <= 0 || request.ToEmployeeId == fromEmployeeId)
            {
                throw new ArgumentException("Invalid delegate employee selected.");
            }
            if (request.EndDate.Date < request.StartDate.Date)
            {
                throw new ArgumentException("End date cannot be earlier than start date.");
            }

            var toEmp = await _unitOfWork.Employees.GetById(request.ToEmployeeId);
            if (toEmp == null || !toEmp.IsActive || toEmp.IsDeleted)
            {
                throw new ArgumentException("Selected delegate employee is inactive or not found.");
            }

            var delegation = new WorkflowDelegation
            {
                FromEmployeeId = fromEmployeeId,
                ToEmployeeId = request.ToEmployeeId,
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date.AddDays(1).AddTicks(-1), // End of day
                RequestType = string.IsNullOrWhiteSpace(request.RequestType) || request.RequestType.Equals("All", StringComparison.OrdinalIgnoreCase) ? null : request.RequestType,
                Reason = request.Reason,
                CreatedBy = (int)fromEmployeeId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = (int)fromEmployeeId,
                UpdatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.WorkflowDelegations.Add(delegation);
            _unitOfWork.Save();

            var fromEmp = await _unitOfWork.Employees.GetById(fromEmployeeId);
            string fromName = fromEmp != null ? $"{fromEmp.FirstName} {fromEmp.LastName}".Trim() : $"Employee #{fromEmployeeId}";
            string toName = $"{toEmp.FirstName} {toEmp.LastName}".Trim();

            // Notify delegate
            try
            {
                await _unitOfWork.Notifications.Add(new Notification
                {
                    EmployeeId = request.ToEmployeeId,
                    Title = "Workflow Approvals Delegated to You",
                    Message = $"{fromName} has delegated their approval authority to you from {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}.",
                    Category = "System",
                    Type = "info",
                    Icon = "swap_horiz",
                    IconBg = "#eff6ff",
                    IconColor = "#2563eb",
                    Route = "/index/request",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                });
                _unitOfWork.Save();
            }
            catch { }

            return new WorkflowDelegationDTO
            {
                Id = delegation.Id,
                FromEmployeeId = fromEmployeeId,
                FromEmployeeName = fromName,
                ToEmployeeId = toEmp.Id,
                ToEmployeeName = toName,
                StartDate = delegation.StartDate,
                EndDate = delegation.EndDate,
                RequestType = delegation.RequestType ?? "All",
                Reason = delegation.Reason,
                IsActive = true
            };
        }

        public async Task<bool> RevokeDelegationAsync(long delegationId, long employeeId)
        {
            var delegation = await _unitOfWork.WorkflowDelegations.GetById(delegationId);
            if (delegation == null || delegation.FromEmployeeId != employeeId)
            {
                return false;
            }

            delegation.IsActive = false;
            delegation.IsDeleted = true;
            delegation.UpdatedBy = (int)employeeId;
            delegation.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.WorkflowDelegations.Update(delegation);
            return _unitOfWork.Save() > 0;
        }
    }
}
