using System.Security.Claims;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Serilog;

namespace KHRMS.Services
{
    public class EmployeeDailyWorkModeService : IEmployeeDailyWorkModeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public EmployeeDailyWorkModeService(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task<WorkModeResponseDTO?> GetWorkModeForDateAsync(long employeeId, DateOnly date)
        {
            var record = await _unitOfWork.EmployeeDailyWorkModes.GetByEmployeeAndDateAsync(employeeId, date);
            if (record == null) return null;

            var employee = await _unitOfWork.Employees.GetById(employeeId);
            return MapToResponse(record, employee);
        }

        public async Task<IEnumerable<WorkModeResponseDTO>> GetWorkModesForRangeAsync(long employeeId, DateOnly startDate, DateOnly endDate)
        {
            var records = await _unitOfWork.EmployeeDailyWorkModes.GetByEmployeeAndDateRangeAsync(employeeId, startDate, endDate);
            var employee = await _unitOfWork.Employees.GetById(employeeId);
            return records.Select(r => MapToResponse(r, employee)).ToList();
        }

        public async Task<IEnumerable<WorkModeResponseDTO>> GetAllWorkModesForDateAsync(DateOnly date)
        {
            var records = await _unitOfWork.EmployeeDailyWorkModes.GetAllForDateAsync(date);
            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => e);

            return records.Select(r =>
            {
                employees.TryGetValue(r.EmployeeId, out var emp);
                return MapToResponse(r, emp);
            }).ToList();
        }

        public async Task<IEnumerable<WorkModeResponseDTO>> GetAllWorkModesForRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            var records = await _unitOfWork.EmployeeDailyWorkModes.GetAllForDateRangeAsync(startDate, endDate);
            var employees = (await _unitOfWork.Employees.GetAll()).ToDictionary(e => e.Id, e => e);

            return records.Select(r =>
            {
                employees.TryGetValue(r.EmployeeId, out var emp);
                return MapToResponse(r, emp);
            }).ToList();
        }

        public async Task<WorkModeResponseDTO> SetWorkModeAsync(SetWorkModeDTO dto, ClaimsPrincipal user)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            long currentEmpId = _userContextService.GetCurrentEmployeeId();
            if (currentEmpId == 0 && user != null)
            {
                var idStr = user.FindFirst("UserId")?.Value 
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? user.FindFirst("id")?.Value;
                if (long.TryParse(idStr, out var parsedId))
                {
                    currentEmpId = parsedId;
                }
            }

            long targetEmpId = dto.EmployeeId > 0 ? dto.EmployeeId : currentEmpId;
            if (targetEmpId <= 0)
                throw new InvalidOperationException("Invalid employee ID specified for work mode.");

            // Permission check: if modifying someone else's work mode, user must be Admin, HR, or Manager
            if (targetEmpId != currentEmpId)
            {
                bool isPrivileged = _userContextService.IsAdmin() || _userContextService.IsHR() || _userContextService.IsManager();
                if (!isPrivileged && user != null)
                {
                    isPrivileged = user.IsInRole("Admin") || user.IsInRole("System Admin") || 
                                   user.IsInRole("HR") || user.IsInRole("HR Operations") || 
                                   user.IsInRole("Manager") || user.IsInRole("Management");
                }

                if (!isPrivileged)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update work mode for another employee.");
                }
            }

            var employee = await _unitOfWork.Employees.GetById(targetEmpId);
            if (employee == null)
                throw new InvalidOperationException($"Employee with ID {targetEmpId} not found.");

            // Determine date
            DateOnly workDate;
            if (!string.IsNullOrWhiteSpace(dto.WorkDate) && DateOnly.TryParse(dto.WorkDate, out var parsedDate))
            {
                workDate = parsedDate;
            }
            else if (dto.Date.HasValue)
            {
                workDate = DateOnly.FromDateTime(dto.Date.Value);
            }
            else
            {
                workDate = DateOnly.FromDateTime(DateTime.Today);
            }

            // Normalize work mode
            string rawMode = (dto.WorkMode ?? string.Empty).Trim();
            string normalizedMode;
            if (rawMode.Equals("WFH", StringComparison.OrdinalIgnoreCase) || rawMode.IndexOf("Home", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                normalizedMode = "WFH";
            }
            else if (rawMode.Equals("OD", StringComparison.OrdinalIgnoreCase) || rawMode.IndexOf("Duty", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                normalizedMode = "OD";
            }
            else
            {
                normalizedMode = "WFO";
            }

            // Determine selectedBy
            string selectedBy = dto.SelectedBy ?? string.Empty;
            if (string.IsNullOrWhiteSpace(selectedBy))
            {
                if (targetEmpId == currentEmpId)
                {
                    selectedBy = "Employee";
                }
                else if (_userContextService.IsAdmin() || _userContextService.IsHR())
                {
                    selectedBy = "Admin";
                }
                else if (_userContextService.IsManager())
                {
                    selectedBy = "Manager";
                }
                else
                {
                    selectedBy = "Policy";
                }
            }

            var existing = await _unitOfWork.EmployeeDailyWorkModes.GetByEmployeeAndDateAsync(targetEmpId, workDate);
            if (existing != null)
            {
                existing.WorkMode = normalizedMode;
                existing.SelectedBy = selectedBy;
                if (!string.IsNullOrWhiteSpace(dto.Notes))
                {
                    existing.Notes = dto.Notes;
                }
                existing.UpdatedDate = DateTime.Now;
                existing.UpdatedBy = (int)currentEmpId;
                _unitOfWork.EmployeeDailyWorkModes.Update(existing);
                _unitOfWork.Save();
                Log.Information("Updated daily work mode for Employee {EmpId} on {Date} to {Mode} by {SelectedBy}", targetEmpId, workDate, normalizedMode, selectedBy);
                return MapToResponse(existing, employee);
            }
            else
            {
                var newRecord = new EmployeeDailyWorkMode
                {
                    EmployeeId = targetEmpId,
                    WorkDate = workDate,
                    WorkMode = normalizedMode,
                    SelectedBy = selectedBy,
                    Notes = dto.Notes,
                    CreatedDate = DateTime.Now,
                    CreatedBy = (int)currentEmpId,
                    IsActive = true,
                    IsDeleted = false
                };
                await _unitOfWork.EmployeeDailyWorkModes.Add(newRecord);
                _unitOfWork.Save();
                Log.Information("Created daily work mode for Employee {EmpId} on {Date} as {Mode} by {SelectedBy}", targetEmpId, workDate, normalizedMode, selectedBy);
                return MapToResponse(newRecord, employee);
            }
        }

        private static WorkModeResponseDTO MapToResponse(EmployeeDailyWorkMode record, Employee? employee)
        {
            return new WorkModeResponseDTO
            {
                Id = record.Id,
                EmployeeId = record.EmployeeId,
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}".Trim() : string.Empty,
                WorkDate = record.WorkDate.ToString("yyyy-MM-dd"),
                WorkMode = record.WorkMode,
                SelectedBy = record.SelectedBy,
                Notes = record.Notes,
                CreatedDate = record.CreatedDate,
                UpdatedDate = record.UpdatedDate
            };
        }
    }
}
