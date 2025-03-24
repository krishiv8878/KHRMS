using KHRMS.Core;
using KHRMS.Infrastructure.Migrations;
using System.Reflection.Metadata;

namespace KHRMS.Services
{

    public class EmployeeAttendanceService(IUnitOfWork unitOfWork) : IEmployeeAttendanceService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IEnumerable<EmployeeAttendance>> GetAllAsync()
        {
            return await _unitOfWork.EmployeeAttendance.GetAll();
        }

        public async Task<EmployeeAttendance> GetByIdAsync(long id)
        {
            return await _unitOfWork.EmployeeAttendance.GetById(id);
        }

        public async Task<EmployeeAttendance> GetByEmployeeIdAsync(long employeeId)
        {
            return await _unitOfWork.EmployeeAttendance.GetById(employeeId);

        }

        public async Task AddAsync(EmployeeAttendance attendance)
        {
            var attendancebyid = (await _unitOfWork.EmployeeAttendance.GetAll()).FirstOrDefault(r => r.EmployeeId == attendance.EmployeeId && r.ClockIn.Date == attendance.ClockIn.Date);
            attendance.CreatedDate = DateTime.Now;
            if (attendancebyid != null)
            {
                await UpdateExistingAsync(attendance,attendancebyid);
            }else{
                attendance.EffectiveHours = attendance.TotalHours;
                await _unitOfWork.EmployeeAttendance.Add(attendance);
                var result = _unitOfWork.Save();
            }

        }

        public Task UpdateAsync(EmployeeAttendance attendance)
        {
            _unitOfWork.EmployeeAttendance.Update(attendance);
            var result = _unitOfWork.Save();
            return Task.CompletedTask;
        }

        public async Task UpdateExistingAsync(EmployeeAttendance attendance,EmployeeAttendance attendancebyid)
        {
            if (attendancebyid != null)
            {
                var totalduration = attendance.ClockOut - attendancebyid.ClockIn;
                attendancebyid.ClockIn = attendancebyid.ClockIn;
                attendancebyid.ClockOut = attendance.ClockOut;
                attendancebyid.TotalHours = new TimeSpan(totalduration.Hours, totalduration.Minutes, totalduration.Seconds);
                var effectivehrs = attendancebyid.EffectiveHours + attendance.TotalHours;
                attendancebyid.EffectiveHours = new TimeSpan(effectivehrs.Hours, effectivehrs.Minutes, effectivehrs.Seconds);
                attendancebyid.UpdatedDate = DateTime.Now;
                _unitOfWork.EmployeeAttendance.Update(attendancebyid);
                var result = _unitOfWork.Save();
            }
            else
            {
                throw new Exception("record not found");
            }
        }

        public async Task DeleteAsync(long id)
        {
            var attendance = await _unitOfWork.EmployeeAttendance.GetById(id);          
            if (attendance != null)
            {
                _unitOfWork.EmployeeAttendance.Delete(attendance);
                _unitOfWork.Save();
            }

        }
    }
}


