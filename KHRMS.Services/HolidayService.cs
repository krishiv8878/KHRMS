using KHRMS.Core;

namespace KHRMS.Services
{
    public class HolidayService(IUnitOfWork unitOfWork) : IHolidayService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<bool> CreateHoliday(Holiday holiday)
        {
            if(holiday != null)
            {
                holiday.CreatedDate= DateTime.Now;
                await _unitOfWork.Holidays.Add(holiday);

                var result = _unitOfWork.Save();

                if (result > 0)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public async Task<bool> DeleteHoliday(long holidayId)
        {
            if (holidayId > 0)
            {
                var holidayDetails = await _unitOfWork.Holidays.GetById(holidayId);
                if (holidayDetails != null)
                {
                    holidayDetails.IsDeleted = true;
                    holidayDetails.IsActive = false;

                    _unitOfWork.Holidays.Update(holidayDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

        public async Task<IEnumerable<Holiday>> GetAllHolidays()
        {
            var holidays = await _unitOfWork.Holidays.GetAll();
            return holidays;
        }
        public async Task<Holiday> GetHolidayById(int holidayId)
        {
            if (holidayId > 0)
            {
                var holidayDetails = await _unitOfWork.Holidays.GetById(holidayId);
                if (holidayDetails != null)
                {
                    return holidayDetails;
                }
            }
            return null;
        }

        public async Task<bool> UpdateHoliday(Holiday holiday)
        {
            if (holiday != null)
            {
                var holidayDetails = await _unitOfWork.Holidays.GetById(holiday.Id);
                if (holidayDetails != null)
                {
                    holidayDetails.HolidayName = holiday.HolidayName;
                    holidayDetails.Description = holiday.Description;
                    holidayDetails.UpdatedDate = DateTime.Now;
                    holidayDetails.HolidayDate = holiday.HolidayDate;
                    // ✅ Ensure IsActive status is updated
                    holidayDetails.IsActive = holiday.IsActive;
                    _unitOfWork.Holidays.Update(holidayDetails);
                    var result = _unitOfWork.Save();
                    if (result > 0)
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }

    }
        
    
}
