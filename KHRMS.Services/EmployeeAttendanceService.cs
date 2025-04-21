using KHRMS.Core;
using KHRMS.Infrastructure.Migrations;
using KHRMS.Services.Interfaces;
using System.Reflection.Metadata;

namespace KHRMS.Services
{

    public class EmployeeAttendanceService : IEmployeeAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _sendEmailService;
        private readonly IUserContextService _userContext;

        public EmployeeAttendanceService(
      IUnitOfWork unitOfWork,
      ISendEmailService emailRepository,
      IUserContextService userContextService)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = emailRepository;
            _userContext = userContextService;
        }


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

        //public async Task UpdateExistingAsync(EmployeeAttendance attendance,EmployeeAttendance attendancebyid)
        //{
        //    if (attendancebyid != null)
        //    {
        //        var totalduration = attendance.ClockOut - attendancebyid.ClockIn;
        //        attendancebyid.ClockIn = attendancebyid.ClockIn;
        //        attendancebyid.ClockOut = attendance.ClockOut;
        //        attendancebyid.TotalHours = new TimeSpan(totalduration.Hours, totalduration.Minutes, totalduration.Seconds);
        //        var effectivehrs = attendancebyid.EffectiveHours + attendance.TotalHours;
        //        attendancebyid.EffectiveHours = new TimeSpan(effectivehrs.Hours, effectivehrs.Minutes, effectivehrs.Seconds);
        //        attendancebyid.UpdatedDate = DateTime.Now;
        //        _unitOfWork.EmployeeAttendance.Update(attendancebyid);
        //        var result = _unitOfWork.Save();
        //    }
        //    else
        //    {
        //        throw new Exception("record not found");
        //    }
        //}


        public async Task UpdateExistingAsync(EmployeeAttendance attendance, EmployeeAttendance attendancebyid)
        {
            if (attendancebyid != null)
            {
                // 1. Calculate new TotalHours from ClockIn and ClockOut
                var totalDuration = attendance.ClockOut - attendancebyid.ClockIn;

                // 2. Update ClockIn and ClockOut
                attendancebyid.ClockOut = attendance.ClockOut;

                // 3. Save total hours as a DateTime (based on 0001-01-01 + timespan)
                attendancebyid.TotalHours = new DateTime(1, 1, 1).Add(totalDuration);

                // 4. Add new total to previous effective hours
                TimeSpan previousEffective = attendancebyid.EffectiveHours.TimeOfDay;
                TimeSpan newDuration = attendancebyid.TotalHours.TimeOfDay;
                TimeSpan effectiveSum = previousEffective + newDuration;

                attendancebyid.EffectiveHours = new DateTime(1, 1, 1).Add(effectiveSum);

                // 5. Update metadata
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


        public async Task<bool> SendRegularizationRequestEmail(EmployeeAttendance attendance)
        {
            if (attendance == null)
                return false;
            long employeeId = _userContext.GetCurrentEmployeeId();



            var regularizationRequest = new EmployeeAttendance
            {
                EmployeeId = employeeId,
                ClockIn = attendance.ClockIn,
                ClockOut = attendance.ClockOut,
                TotalHours = attendance.TotalHours,
                EffectiveHours = attendance.EffectiveHours,
                RegularizationReason = attendance.RegularizationReason,
                IsRegularized = false,
                RegularizationRequestedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };
            await _unitOfWork.EmployeeAttendance.Add(regularizationRequest);


            var employee = await _unitOfWork.Employees.GetById(attendance.EmployeeId);
            if (employee == null)
                throw new Exception("Employee not found.");

            var manager = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t => t.Id == employee.ManagerId);
            if (manager == null)
                throw new Exception("Manager not found.");

            string formattedDate = attendance.ClockIn.ToString("dd-MM-yyyy");
            string inTime = attendance.ClockIn.ToString("hh:mm tt");
            string outTime = attendance.ClockOut.ToString("hh:mm tt");

            var managerEmail = manager.EmailAddress;
            var managerName = $"{manager.FirstName} {manager.LastName}";
            var employeeName = $"{employee.FirstName} {employee.LastName}";
            var regularizationType = "RegularizationRequest";

            var dict = new Dictionary<string, string>
    {
        { "ManagerName", managerName },
        { "Date", formattedDate },
        { "InTime", inTime },
        { "OutTime", outTime },
        { "EmployeeName", employeeName },
        { "ManagerEmail", managerEmail },
        { "RegularizationReason", attendance.RegularizationReason ?? "No specific reason provided." }
    };

            var subject = $"Regularization Request from {employeeName} for {formattedDate}";

            await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, regularizationType);

            return true;
        }

    }
}


