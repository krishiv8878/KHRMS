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
            var attendLog = await _unitOfWork.AttendanceLog.GetById(attendanceLog.Id);
            var logs = (await _unitOfWork.AttendanceLog.GetAll()).FirstOrDefault(r=>r.AttendanceDate == attendanceLog.AttendanceDate && r.OutTime ==  null);
            if (attendLog != null) {
                attendLog.InTime = attendanceLog.InTime;
                attendLog.OutTime = attendanceLog.OutTime;
                attendLog.Duration = attendanceLog.Duration;
                _unitOfWork.AttendanceLog.Update(attendLog);
            }
            if (logs != null && attendanceLog.OutTime != null)
            {
                logs.OutTime = attendanceLog.OutTime;
                logs.Duration = (decimal)(attendanceLog.OutTime - logs.InTime).Value.TotalHours;
                _unitOfWork.AttendanceLog.Update(logs);
            }
            else
            {
                await AddAttendanceLogAsync(attendanceLog);
            }
            _unitOfWork.Save();
        }
    }
}
