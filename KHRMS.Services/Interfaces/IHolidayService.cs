using KHRMS.Core;

namespace KHRMS.Services
{
    public interface IHolidayService
    {
        Task<bool> CreateHoliday(Holiday holiday);
        Task<IEnumerable<Holiday>> GetAllHolidays();
        Task<Holiday> GetHolidayById(int holidayId);
        Task<bool> UpdateHoliday(Holiday holiday);
        Task<bool> DeleteHoliday(long holidayId);
        Task<(int copiedCount, int skippedCount)> CopyHolidaysToYear(int sourceYear, int targetYear);
    }
}
