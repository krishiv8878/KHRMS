using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KHRMS.Services
{
    public class DepartmentService(IUnitOfWork unitOfWork) : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = (await _unitOfWork.Departments.GetAll())
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .ToList();

            var employees = (await _unitOfWork.Employees.GetAll())
                .Where(e => !e.IsDeleted)
                .ToList();

            var empCountMap = employees
                .Where(e => e.DepartmentId.HasValue)
                .GroupBy(e => e.DepartmentId!.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            var managerMap = employees
                .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());

            return departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                CostCenter = d.CostCenter,
                ManagerId = d.ManagerId,
                ManagerName = d.ManagerId.HasValue && managerMap.TryGetValue(d.ManagerId.Value, out var mName) ? mName : null,
                Description = d.Description,
                EmployeeCount = empCountMap.TryGetValue(d.Id, out var count) ? count : 0,
                IsActive = d.IsActive,
                CreatedDate = d.CreatedDate
            });
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(long id)
        {
            var d = await _unitOfWork.Departments.GetById(id);
            if (d == null || d.IsDeleted) return null;

            string? managerName = null;
            if (d.ManagerId.HasValue)
            {
                var mgr = await _unitOfWork.Employees.GetById(d.ManagerId.Value);
                if (mgr != null && !mgr.IsDeleted)
                {
                    managerName = $"{mgr.FirstName} {mgr.LastName}".Trim();
                }
            }

            var employees = await _unitOfWork.Employees.GetAll();
            int empCount = employees.Count(e => !e.IsDeleted && e.DepartmentId == d.Id);

            return new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                CostCenter = d.CostCenter,
                ManagerId = d.ManagerId,
                ManagerName = managerName,
                Description = d.Description,
                EmployeeCount = empCount,
                IsActive = d.IsActive,
                CreatedDate = d.CreatedDate
            };
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request, long currentUserId = 0)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Department Name is required.");
            }

            var trimmedName = request.Name.Trim();

            // Check name uniqueness among non-deleted departments
            var existing = (await _unitOfWork.Departments.GetAll())
                .FirstOrDefault(d => !d.IsDeleted && string.Equals(d.Name, trimmedName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                throw new InvalidOperationException($"A department with the name '{trimmedName}' already exists.");
            }

            if (request.ManagerId.HasValue && request.ManagerId.Value > 0)
            {
                var mgr = await _unitOfWork.Employees.GetById(request.ManagerId.Value);
                if (mgr == null || mgr.IsDeleted)
                {
                    throw new ArgumentException($"Selected manager (ID: {request.ManagerId.Value}) does not exist.");
                }
            }

            var department = new Department
            {
                Name = trimmedName,
                Code = request.Code?.Trim().ToUpperInvariant(),
                CostCenter = request.CostCenter?.Trim(),
                ManagerId = request.ManagerId > 0 ? request.ManagerId : null,
                Description = request.Description?.Trim(),
                IsActive = true,
                IsDeleted = false,
                CreatedBy = (int)currentUserId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Departments.Add(department);
            _unitOfWork.Save();

            Log.Information("DepartmentService - Created new Department '{Name}' (ID: {Id}) by User #{UserId}",
                department.Name, department.Id, currentUserId);

            return (await GetDepartmentByIdAsync(department.Id))!;
        }

        public async Task<DepartmentDto?> UpdateDepartmentAsync(long id, UpdateDepartmentRequest request, long currentUserId = 0)
        {
            var department = await _unitOfWork.Departments.GetById(id);
            if (department == null || department.IsDeleted)
            {
                return null;
            }

            var trimmedName = request.Name.Trim();

            // Check duplicate name
            var duplicate = (await _unitOfWork.Departments.GetAll())
                .FirstOrDefault(d => !d.IsDeleted && d.Id != id && string.Equals(d.Name, trimmedName, StringComparison.OrdinalIgnoreCase));

            if (duplicate != null)
            {
                throw new InvalidOperationException($"Another department with the name '{trimmedName}' already exists.");
            }

            if (request.ManagerId.HasValue && request.ManagerId.Value > 0)
            {
                var mgr = await _unitOfWork.Employees.GetById(request.ManagerId.Value);
                if (mgr == null || mgr.IsDeleted)
                {
                    throw new ArgumentException($"Selected manager (ID: {request.ManagerId.Value}) does not exist.");
                }
            }

            department.Name = trimmedName;
            department.Code = request.Code?.Trim().ToUpperInvariant();
            department.CostCenter = request.CostCenter?.Trim();
            department.ManagerId = request.ManagerId > 0 ? request.ManagerId : null;
            department.Description = request.Description?.Trim();
            department.IsActive = request.IsActive;
            department.UpdatedBy = (int)currentUserId;
            department.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Departments.Update(department);
            _unitOfWork.Save();

            Log.Information("DepartmentService - Updated Department '{Name}' (ID: {Id}) by User #{UserId}",
                department.Name, department.Id, currentUserId);

            return await GetDepartmentByIdAsync(department.Id);
        }

        public async Task<bool> DeleteDepartmentAsync(long id, long currentUserId = 0)
        {
            var department = await _unitOfWork.Departments.GetById(id);
            if (department == null || department.IsDeleted)
            {
                return false;
            }

            // Check if active employees belong to this department
            var employees = await _unitOfWork.Employees.GetAll();
            var assignedCount = employees.Count(e => !e.IsDeleted && e.DepartmentId == id);
            if (assignedCount > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete department '{department.Name}' because {assignedCount} active employee(s) are assigned to it. Please reassign them first.");
            }

            department.IsDeleted = true;
            department.IsActive = false;
            department.UpdatedBy = (int)currentUserId;
            department.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Departments.Update(department);
            var result = _unitOfWork.Save();

            Log.Information("DepartmentService - Soft-deleted Department '{Name}' (ID: {Id}) by User #{UserId}",
                department.Name, id, currentUserId);

            return result > 0;
        }

        public async Task<int> BackfillDepartmentsFromExistingEmployeesAsync()
        {
            var allEmployees = (await _unitOfWork.Employees.GetAll())
                .Where(e => !e.IsDeleted)
                .ToList();

            var existingDepts = (await _unitOfWork.Departments.GetAll())
                .Where(d => !d.IsDeleted)
                .ToList();

            var deptNameMap = existingDepts
                .ToDictionary(d => d.Name.Trim().ToLowerInvariant(), d => d);

            // Standard seed departments if table is empty
            var defaultDepartmentNames = new[] { "Engineering", "Human Resources", "Finance", "Sales", "Marketing", "Operations", "Product", "Quality Assurance" };
            foreach (var defaultName in defaultDepartmentNames)
            {
                var lower = defaultName.ToLowerInvariant();
                if (!deptNameMap.ContainsKey(lower))
                {
                    var newDept = new Department
                    {
                        Name = defaultName,
                        Code = defaultName.Substring(0, Math.Min(4, defaultName.Length)).ToUpperInvariant(),
                        CostCenter = $"CC-{defaultName.Substring(0, Math.Min(3, defaultName.Length)).ToUpperInvariant()}",
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.Departments.Add(newDept);
                    _unitOfWork.Save();
                    deptNameMap[lower] = newDept;
                }
            }

            // Also check distinct existing branch / department values on employees
            var distinctEmployeeBranches = allEmployees
                .Select(e => e.Branch?.Trim())
                .Where(b => !string.IsNullOrWhiteSpace(b))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var branchName in distinctEmployeeBranches)
            {
                if (branchName == null) continue;
                var lower = branchName.ToLowerInvariant();
                if (!deptNameMap.ContainsKey(lower))
                {
                    var newDept = new Department
                    {
                        Name = branchName,
                        Code = branchName.Substring(0, Math.Min(4, branchName.Length)).ToUpperInvariant(),
                        CostCenter = "CC-GEN",
                        IsActive = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _unitOfWork.Departments.Add(newDept);
                    _unitOfWork.Save();
                    deptNameMap[lower] = newDept;
                }
            }

            // Now backfill employee.DepartmentId
            int updatedCount = 0;
            foreach (var emp in allEmployees)
            {
                if (!emp.DepartmentId.HasValue)
                {
                    var branchLower = emp.Branch?.Trim().ToLowerInvariant() ?? "";
                    if (!string.IsNullOrEmpty(branchLower) && deptNameMap.TryGetValue(branchLower, out var matchedDept))
                    {
                        emp.DepartmentId = matchedDept.Id;
                        _unitOfWork.Employees.Update(emp);
                        updatedCount++;
                    }
                    else if (deptNameMap.TryGetValue("engineering", out var fallbackDept))
                    {
                        emp.DepartmentId = fallbackDept.Id;
                        _unitOfWork.Employees.Update(emp);
                        updatedCount++;
                    }
                }
            }

            if (updatedCount > 0)
            {
                _unitOfWork.Save();
                Log.Information("DepartmentService - Successfully backfilled DepartmentId for {Count} employees.", updatedCount);
            }

            return updatedCount;
        }
    }
}
