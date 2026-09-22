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
                    holidayDetails.Type = holiday.Type;
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

        public async Task<(int copiedCount, int skippedCount)> CopyHolidaysToYear(int sourceYear, int targetYear)
        {
            if (sourceYear <= 2000 || targetYear <= 2000 || sourceYear == targetYear)
            {
                return (0, 0);
            }

            var allHolidays = (await _unitOfWork.Holidays.GetAll()).ToList();

            var sourceHolidays = allHolidays
                .Where(h => !h.IsDeleted && h.HolidayDate.Year == sourceYear)
                .ToList();

            if (!sourceHolidays.Any())
            {
                return (0, 0);
            }

            var existingTargetHolidays = allHolidays
                .Where(h => !h.IsDeleted && h.HolidayDate.Year == targetYear)
                .ToList();

            int copiedCount = 0;
            int skippedCount = 0;

            foreach (var sh in sourceHolidays)
            {
                int targetMonth = sh.HolidayDate.Month;
                int targetDay = sh.HolidayDate.Day;

                // Handle Leap Year: Feb 29 on non-leap year falls back to Feb 28
                if (targetMonth == 2 && targetDay == 29 && !DateTime.IsLeapYear(targetYear))
                {
                    targetDay = 28;
                }

                var targetDate = new DateOnly(targetYear, targetMonth, targetDay);

                // Check if already exists with same name and date in target year
                bool alreadyExists = existingTargetHolidays.Any(eh =>
                    eh.HolidayDate == targetDate &&
                    string.Equals(eh.HolidayName?.Trim(), sh.HolidayName?.Trim(), StringComparison.OrdinalIgnoreCase));

                if (alreadyExists)
                {
                    skippedCount++;
                    continue;
                }

                var newHoliday = new Holiday
                {
                    HolidayName = sh.HolidayName,
                    Description = sh.Description,
                    Type = sh.Type,
                    IsOptional = sh.IsOptional,
                    HolidayDate = targetDate,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _unitOfWork.Holidays.Add(newHoliday);
                copiedCount++;
            }

            if (copiedCount > 0)
            {
                _unitOfWork.Save();
            }

            return (copiedCount, skippedCount);
        }
    }
}
