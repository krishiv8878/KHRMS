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

        public Task DeleteAttendanceLogAsync(long id)
        {
            throw new NotImplementedException();
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
            var logs = (await _unitOfWork.AttendanceLog.GetAll()).FirstOrDefault(r=>r.AttendanceDate == attendanceLog.AttendanceDate && r.out_time ==  null);
            if (attendLog != null) {
                attendLog.in_time = attendanceLog.in_time;
                attendLog.out_time = attendanceLog.out_time;
                attendLog.duration = attendanceLog.duration;
                _unitOfWork.AttendanceLog.Update(attendLog);
            }
            if (logs != null && attendanceLog.out_time != null)
            {
                logs.out_time = attendanceLog.out_time;
                logs.duration = (decimal)(attendanceLog.out_time - logs.in_time).Value.TotalHours;
                _unitOfWork.AttendanceLog.Update(logs);
            }
            else
            {
                await AddAttendanceLogAsync(attendanceLog);
            }
            _unitOfWork.Save();
        }

        public Task UpdateExistingAttendanceLogAsync(AttendanceLog attendanceLog, AttendanceLog attendanceLogbyId)
        {
            throw new NotImplementedException();
        }
    }
}
