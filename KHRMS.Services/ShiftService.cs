using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Services
{
    public class ShiftService(IUnitOfWork unitOfWork) : IShiftService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IEnumerable<ShiftMaster>> GetAllShiftsAsync()
        {
            return (await _unitOfWork.ShiftRepository.GetAll())
                .Where(s => !s.IsDeleted);
        }

        public async Task<ShiftMaster> GetShiftByIdAsync(long id)
        {
            return await _unitOfWork.ShiftRepository.GetById(id);
        }

        public async Task AddShiftAsync(ShiftMaster shift)
        {
            shift.CreatedDate = DateTime.UtcNow;
            shift.IsActive = true;
            shift.IsDeleted = false;
            await _unitOfWork.ShiftRepository.Add(shift);
            _unitOfWork.Save();
        }

        public Task UpdateShiftAsync(ShiftMaster shift)
        {
            shift.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.ShiftRepository.Update(shift);
            _unitOfWork.Save();
            return Task.CompletedTask;
        }

        public async Task DeleteShiftAsync(long id)
        {
            var shift = await _unitOfWork.ShiftRepository.GetByIdAsync(id);
            if (shift != null)
            {
                shift.IsDeleted = true;
                shift.IsActive = false;
                shift.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.ShiftRepository.Update(shift);
                _unitOfWork.Save();
            }
        }

        public async Task<IEnumerable<EmployeeShiftAssignment>> GetEmployeeShiftAssignmentsAsync(long employeeId)
        {
            return (await _unitOfWork.EmployeeShiftAssignments.GetAll())
                .Where(a => !a.IsDeleted && a.EmployeeId == employeeId)
                .OrderByDescending(a => a.EffectiveFromDate);
        }

        public async Task<EmployeeShiftAssignment> AssignShiftToEmployeeAsync(EmployeeShiftAssignment assignment)
        {
            if (assignment.EmployeeId <= 0)
                throw new ArgumentException("Valid Employee ID is required.");

            if (assignment.ShiftId <= 0)
                throw new ArgumentException("Valid Shift ID is required.");

            // Expire any existing open assignments starting on or after the new assignment's date
            var existingAssignments = (await _unitOfWork.EmployeeShiftAssignments.GetAll())
                .Where(a => !a.IsDeleted && a.EmployeeId == assignment.EmployeeId &&
                            (!a.EffectiveToDate.HasValue || a.EffectiveToDate.Value >= assignment.EffectiveFromDate))
                .ToList();

            foreach (var prev in existingAssignments)
            {
                prev.EffectiveToDate = assignment.EffectiveFromDate.AddDays(-1);
                prev.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.EmployeeShiftAssignments.Update(prev);
            }

            assignment.CreatedDate = DateTime.UtcNow;
            assignment.IsActive = true;
            assignment.IsDeleted = false;

            await _unitOfWork.EmployeeShiftAssignments.Add(assignment);
            _unitOfWork.Save();

            return assignment;
        }
    }
}
