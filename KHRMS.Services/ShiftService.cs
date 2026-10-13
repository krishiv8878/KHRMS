using KHRMS.Core;
using Microsoft.EntityFrameworkCore;

namespace KHRMS.Services
{
    public class ShiftService(IUnitOfWork unitOfWork) : IShiftService

    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IEnumerable<ShiftMaster>> GetAllShiftsAsync()
        {
            return await _unitOfWork.ShiftRepository.GetAll();
        }

        public async Task<ShiftMaster> GetShiftByIdAsync(long id)
        {
            return await _unitOfWork.ShiftRepository.GetById(id);
        }

        public async Task AddShiftAsync(ShiftMaster shift)
        {
            await _unitOfWork.ShiftRepository.Add(shift);
            var result = _unitOfWork.Save();

        }




        public Task UpdateShiftAsync(ShiftMaster shift)
        {
            _unitOfWork.ShiftRepository.Update(shift);
            var result = _unitOfWork.Save();
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


        
    }
}





