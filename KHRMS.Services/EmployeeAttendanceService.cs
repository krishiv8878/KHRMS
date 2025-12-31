using KHRMS.Core;
using KHRMS.Core.Models;
using KHRMS.Infrastructure.Migrations;
using KHRMS.Services.Interfaces;
using MimeKit.Cryptography;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace KHRMS.Services
{

    public class EmployeeAttendanceService : IEmployeeAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _sendEmailService;
        private readonly IUserContextService _userContext;
        private readonly IAttendanceLogService _attendanceLogService;

        public EmployeeAttendanceService(
      IUnitOfWork unitOfWork,
      ISendEmailService emailRepository,
      IUserContextService userContextService,
      IAttendanceLogService attendanceLogService)
        {
            _unitOfWork = unitOfWork;
            _sendEmailService = emailRepository;
            _userContext = userContextService;
            _attendanceLogService = attendanceLogService;
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
            //var attendancebyid = (await _unitOfWork.EmployeeAttendance.GetAll()).FirstOrDefault(r => r.EmployeeId == attendance.EmployeeId && r.CreatedDate.Value.Date == attendance.CreatedDate.Value.Date);
            //if (attendancebyid != null)
            //{
            //    if(attendancebyid.ClockOut == null)
            //    {
            //        attendancebyid.ClockOut = attendance.ClockOut;
            //        attendancebyid.TotalHours = (decimal)(attendance.ClockOut - attendancebyid.ClockIn).Value.TotalHours;
            //        attendancebyid.EffectiveHours = (decimal)(attendance.ClockOut - attendancebyid.ClockIn).Value.TotalHours;
            //        await UpdateAsync(attendancebyid);
            //    }
            //    else
            //    {
            //        await UpdateExistingAsync(attendance, attendancebyid);
            //    }
            //}else{
            //    attendance.ClockIn = attendance.ClockIn;
            //    attendance.ClockOut = attendance.ClockOut;
            //    attendance.CreatedDate = DateTime.Now;
            //    attendance.EffectiveHours = attendance.TotalHours;
            //    await _unitOfWork.EmployeeAttendance.Add(attendance);
            //    var result = _unitOfWork.Save();
            //}
            var attendancebyid = (await _unitOfWork.EmployeeAttendance.GetAll()).FirstOrDefault(r => r.EmployeeId == attendance.EmployeeId && r.AttendanceDate == attendance.AttendanceDate);
            var attendanceLog = new AttendanceLog
            {
                EmployeeId = attendance.EmployeeId,
                AttendanceDate = attendance.AttendanceDate,
                InTime = attendance.ClockIn,
                OutTime = attendance.ClockOut,
                Duration = 0,
            };
            if (attendancebyid != null)
            {
                await _attendanceLogService.UpdateAttendanceLogAsync(attendanceLog);
                if (attendance.ClockOut != null)
                {
                    attendancebyid.ClockOut = attendance.ClockOut;
                    attendancebyid.TotalHours = (decimal)(attendance.ClockOut - attendancebyid.ClockIn).Value.TotalHours;
                    var efh = (await _attendanceLogService.GetAllAttendanceLogAsync()).Where(r => r.AttendanceDate == attendance.AttendanceDate).ToList();
                    attendancebyid.EffectiveHours = efh.Sum(r => r.Duration);
                    await UpdateAsync(attendancebyid);
                }
            }
            else
            {
                attendance.AttendanceDate = attendance.AttendanceDate;
                attendance.ClockIn = attendance.ClockIn;
                attendance.ClockOut = attendance.ClockOut;
                attendance.CreatedDate = DateTime.Now;
                attendance.EffectiveHours = attendance.TotalHours;
                await _unitOfWork.EmployeeAttendance.Add(attendance);
                await _attendanceLogService.AddAttendanceLogAsync(attendanceLog);
                var result = _unitOfWork.Save();
            }

        }

        public async Task UpdateAsync(EmployeeAttendance attendance)
        {
            var attend = await _unitOfWork.EmployeeAttendance.GetByIdAsync(attendance.Id);
            _unitOfWork.EmployeeAttendance.Update(attend);
            var result = _unitOfWork.Save();
            return;
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
                decimal totalDuration = (decimal)((DateTime)attendance.ClockOut - attendancebyid.ClockIn).TotalHours;
                
                var effective = (decimal)((DateTime)attendance.ClockOut - attendance.ClockIn).TotalHours;
                attendancebyid.ClockOut = attendance.ClockOut;
                attendancebyid.TotalHours = totalDuration;

                // 4. Add new total to previous effective hours
                //TimeSpan previousEffective = attendancebyid.EffectiveHours.TimeOfDay;
                //TimeSpan newDuration = attendancebyid.TotalHours.TimeOfDay;
                //TimeSpan effectiveSum = previousEffective + newDuration;
                var reffectivesum = effective + attendancebyid.EffectiveHours;

                attendancebyid.EffectiveHours = reffectivesum;

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
    //        if (attendance == null)
    //            return false;
    //        long employeeId = _userContext.GetCurrentEmployeeId();



    //        var regularizationRequest = new EmployeeAttendance
    //        {
    //            EmployeeId = employeeId,
    //            ClockIn = attendance.ClockIn,
    //            ClockOut = attendance.ClockOut,
    //            TotalHours = attendance.TotalHours,
    //            EffectiveHours = attendance.EffectiveHours,
    //            RegularizationReason = attendance.RegularizationReason,
    //            IsRegularized = false,
    //            RegularizationRequestedDate = DateTime.Now,
    //            CreatedDate = DateTime.Now,
    //            IsActive = true,
    //            IsDeleted = false
    //        };
    //        await _unitOfWork.EmployeeAttendance.Add(regularizationRequest);

    //        _unitOfWork.Save();
    //        var employee = await _unitOfWork.Employees.GetById(employeeId);
    //        if (employee == null)
    //            throw new Exception("Employee not found.");

    //        var manager = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t => t.Id == employee.ManagerId);
    //        if (manager == null)
    //            throw new Exception("Manager not found.");

    //        string formattedDate = attendance.ClockIn.ToString("dd-MM-yyyy");
    //        string inTime = attendance.ClockIn.ToString("hh:mm tt");
    //        string outTime = attendance.ClockOut.ToString("hh:mm tt");

    //        var managerEmail = manager.EmailAddress;
    //        var managerName = $"{manager.FirstName} {manager.LastName}";
    //        var employeeName = $"{employee.FirstName} {employee.LastName}";
    //        var regularizationType = "RegularizationRequest";

    //        var dict = new Dictionary<string, string>
    //{
    //    { "ManagerName", managerName },
    //    { "Date", formattedDate },
    //    { "InTime", inTime },
    //    { "OutTime", outTime },
    //    { "EmployeeName", employeeName },
    //    { "ManagerEmail", managerEmail },
    //    { "RegularizationReason", attendance.RegularizationReason ?? "No specific reason provided." }
    //};

    //        var subject = $"Regularization Request from {employeeName} for {formattedDate}";
        
    //        await _sendEmailService.SendTemplateEmailAsync(managerEmail, subject, dict, regularizationType);

            return true;
        }


        public async Task<bool> ApproveRegularizationRequestAsync(EmployeeAttendance attendance)
        {
            var attendanceRecord = await _unitOfWork.EmployeeAttendance.GetById(attendance.Id);
            //if (attendanceRecord == null || attendanceRecord.IsRegularized) return false;

            var managerId = _userContext.GetCurrentEmployeeId();

            var employee = await _unitOfWork.Employees.GetById(attendanceRecord.EmployeeId);
            var manager = await _unitOfWork.Employees.GetById(managerId);

            if (employee == null || manager == null) return false;

            attendanceRecord.UpdatedBy = 1;
            attendanceRecord.UpdatedDate = DateTime.Now;
            attendanceRecord.UpdatedDate = DateTime.Now;


            _unitOfWork.EmployeeAttendance.Update(attendanceRecord);
            var result = _unitOfWork.Save();

            if (result <= 0) return false;

            var dict = new Dictionary<string, string>
    {
        { "ManagerName", $"{manager.FirstName} {manager.LastName}" },
        { "Date", attendanceRecord.ClockIn.ToString("dd-MM-yyyy") },
        //{ "RegularizationReason", attendanceRecord.RegularizationReason ?? "No specific reason provided." },
        { "LeaveReason", "RegularizationRequest Approval Request" },
        { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
        { "ManagerEmail", manager.EmailAddress }
    };

            string subject = $"Your Regularization Request for {attendanceRecord.ClockIn:dd-MM-yyyy} Has Been Approved";

            try
            {
                await _sendEmailService.SendTemplateEmailAsync(
                    employee.EmailAddress,
                    subject,
                    dict,
                    "RegularizationRequestApproved"
                );
            }
            catch (Exception ex)
            {
                // Optional: log the exception if needed
                // Log.Error(ex, "Email sending failed for regularization approval.");
            }

            return true;
        }


    }
}


