using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Services.Interfaces;
using Org.BouncyCastle.Asn1.Esf;

namespace KHRMS.Services
{
    public class AttendanceLogService : IAttendanceLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContextService;

        public AttendanceLogService(IUnitOfWork unitOfWork, IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
        }

        public async Task AddAttendanceLogAsync(AttendanceLog attendanceLog)
        {
            attendanceLog.employee = null;
            await _unitOfWork.AttendanceLog.Add(attendanceLog);
            _unitOfWork.Save();
            return;
        }

        public async Task DeleteAttendanceLogAsync(long id)
        {
            await _unitOfWork.AttendanceLog.DeleteAsync(id);
            return;
        }
        public async Task<AttendanceLog> GetAttendanceLogByIdAsync(long id)
        {
            return await _unitOfWork.AttendanceLog.GetById(id);
        }

        public async Task<IEnumerable<AttendanceLog>> GetAllAttendanceLogAsync()
        {
            return await _unitOfWork.AttendanceLog.GetAll();
        }

        public async Task UpdateAttendanceLogAsync(AttendanceLog attendanceLog)
        {
            if (attendanceLog.Id > 0)
            {
                var attendLog = await _unitOfWork.AttendanceLog.GetById(attendanceLog.Id);
                if (attendLog != null)
                {
                    attendLog.InTime = attendanceLog.InTime;
                    attendLog.OutTime = attendanceLog.OutTime;
                    if (attendLog.InTime.HasValue && attendLog.OutTime.HasValue)
                    {
                        attendLog.Duration = (decimal)(attendLog.OutTime.Value - attendLog.InTime.Value).TotalHours;
                    }
                    else
                    {
                        attendLog.Duration = 0;
                    }
                    _unitOfWork.AttendanceLog.Update(attendLog);
                    _unitOfWork.Save();
                    return;
                }
            }

            if (attendanceLog.OutTime != null)
            {
                var activeLog = (await _unitOfWork.AttendanceLog.GetAll())
                    .FirstOrDefault(r => r.EmployeeId == attendanceLog.EmployeeId &&
                                         r.AttendanceDate == attendanceLog.AttendanceDate &&
                                         r.OutTime == null);

                if (activeLog != null)
                {
                    activeLog.OutTime = attendanceLog.OutTime;
                    if (activeLog.InTime.HasValue)
                    {
                        activeLog.Duration = (decimal)(activeLog.OutTime.Value - activeLog.InTime.Value).TotalHours;
                    }
                    _unitOfWork.AttendanceLog.Update(activeLog);
                    _unitOfWork.Save();
                    return;
                }
            }

            await AddAttendanceLogAsync(attendanceLog);
        }
    }
}
