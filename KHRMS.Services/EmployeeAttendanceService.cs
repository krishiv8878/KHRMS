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
            var all = await _unitOfWork.EmployeeAttendance.GetAll();
            return all
                .Where(r => r.EmployeeId == employeeId && !r.IsDeleted)
                .OrderByDescending(r => r.AttendanceDate)
                .FirstOrDefault()!;
        }

        public async Task AddAsync(EmployeeAttendance attendance)
        {
            if (attendance.AttendanceDate == default)
            {
                attendance.AttendanceDate = attendance.ClockIn != default 
                    ? DateOnly.FromDateTime(attendance.ClockIn) 
                    : DateOnly.FromDateTime(DateTime.Today);
            }

            var attendancebyid = (await _unitOfWork.EmployeeAttendance.GetAll())
                .FirstOrDefault(r => r.EmployeeId == attendance.EmployeeId && r.AttendanceDate == attendance.AttendanceDate && !r.IsDeleted);

            decimal initialDuration = (attendance.ClockOut.HasValue && attendance.ClockIn != default)
                ? (decimal)(attendance.ClockOut.Value - attendance.ClockIn).TotalHours
                : 0;

            var attendanceLog = new AttendanceLog
            {
                EmployeeId = attendance.EmployeeId,
                AttendanceDate = attendance.AttendanceDate,
                InTime = attendance.ClockIn,
                OutTime = attendance.ClockOut,
                Duration = initialDuration
            };

            if (attendancebyid != null)
            {
                await _attendanceLogService.UpdateAttendanceLogAsync(attendanceLog);
                var logs = (await _attendanceLogService.GetAllAttendanceLogAsync())
                    .Where(r => r.EmployeeId == attendance.EmployeeId && r.AttendanceDate == attendance.AttendanceDate)
                    .ToList();

                if (attendance.ClockOut != null)
                {
                    var inTimes = logs.Where(r => r.InTime.HasValue).Select(r => r.InTime.Value).ToList();
                    var outTimes = logs.Where(r => r.OutTime.HasValue).Select(r => r.OutTime.Value).ToList();

                    if (inTimes.Any()) attendancebyid.ClockIn = inTimes.Min();
                    if (outTimes.Any()) attendancebyid.ClockOut = outTimes.Max();

                    var sumDuration = logs.Sum(r => r.Duration);
                    attendancebyid.EffectiveHours = sumDuration;
                    attendancebyid.TotalHours = sumDuration > 0 ? sumDuration : (attendancebyid.ClockOut.HasValue && attendancebyid.ClockIn != default ? (decimal)(attendancebyid.ClockOut.Value - attendancebyid.ClockIn).TotalHours : 0);
                    attendancebyid.UpdatedDate = DateTime.Now;
                    await UpdateAsync(attendancebyid);
                }
                else
                {
                    var inTimes = logs.Where(r => r.InTime.HasValue).Select(r => r.InTime.Value).ToList();
                    if (inTimes.Any()) attendancebyid.ClockIn = inTimes.Min();
                    attendancebyid.ClockOut = null;
                    attendancebyid.UpdatedDate = DateTime.Now;
                    await UpdateAsync(attendancebyid);
                }
            }
            else
            {
                attendance.CreatedDate = DateTime.Now;
                attendance.TotalHours = initialDuration;
                attendance.EffectiveHours = initialDuration;
                attendance.IsActive = true;
                attendance.IsDeleted = false;
                await _unitOfWork.EmployeeAttendance.Add(attendance);
                await _attendanceLogService.AddAttendanceLogAsync(attendanceLog);
                _unitOfWork.Save();
            }
        }

        public async Task UpdateAsync(EmployeeAttendance attendance)
        {
            var attend = await _unitOfWork.EmployeeAttendance.GetById(attendance.Id);
            if (attend != null)
            {
                attend.ClockIn = attendance.ClockIn;
                attend.ClockOut = attendance.ClockOut;
                attend.TotalHours = attendance.TotalHours;
                attend.EffectiveHours = attendance.EffectiveHours;
                attend.UpdatedDate = DateTime.Now;
                attend.UpdatedBy = attendance.UpdatedBy;
                attend.IsActive = attendance.IsActive;
                attend.IsDeleted = attendance.IsDeleted;
                _unitOfWork.EmployeeAttendance.Update(attend);
            }
            else
            {
                _unitOfWork.EmployeeAttendance.Update(attendance);
            }
            _unitOfWork.Save();
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
            if (attendance == null) return false;
            long employeeId = attendance.EmployeeId > 0 ? attendance.EmployeeId : _userContext.GetCurrentEmployeeId();
            if (employeeId <= 0) return false;

            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee == null) return false;

            long managerId = employee.ManagerId.HasValue && employee.ManagerId.Value > 0 ? employee.ManagerId.Value : 0;
            if (managerId == 0)
            {
                var admin = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(e => e.Id != employeeId);
                managerId = admin?.Id ?? employeeId;
            }

            var request = new AttendanceRequest
            {
                EmployeeId = employeeId,
                RequestType = "Regularization",
                RequestedDate = attendance.AttendanceDate != default ? attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue) : (attendance.ClockIn != default ? attendance.ClockIn.Date : DateTime.Today),
                RequestedBy = employeeId,
                Reason = attendance.RegularizationReason ?? "Missed Punch Regularization",
                Status = "Pending",
                LastActionBy = employeeId,
                ClockInTime = attendance.ClockIn,
                ClockOutTime = attendance.ClockOut,
                CreatedBy = (int)employeeId,
                CreatedDate = DateTime.UtcNow,
                ManagerId = managerId,
                IsActive = true,
                IsDeleted = false
            };

            await _unitOfWork.AttendanceRequests.Add(request);
            var result = _unitOfWork.Save();

            // Try sending email notification
            try
            {
                var manager = await _unitOfWork.Employees.GetById(managerId);
                if (manager != null && !string.IsNullOrEmpty(manager.EmailAddress))
                {
                    TimeZoneInfo istZone;
                    try
                    {
                        istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    }
                    catch
                    {
                        istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                    }

                    DateTime inTimeIst = attendance.ClockIn.Kind == DateTimeKind.Utc 
                        ? TimeZoneInfo.ConvertTimeFromUtc(attendance.ClockIn, istZone) 
                        : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(attendance.ClockIn, DateTimeKind.Utc), istZone);

                    string outTimeStr = "--";
                    if (attendance.ClockOut.HasValue)
                    {
                        DateTime outTimeIst = attendance.ClockOut.Value.Kind == DateTimeKind.Utc 
                            ? TimeZoneInfo.ConvertTimeFromUtc(attendance.ClockOut.Value, istZone) 
                            : TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(attendance.ClockOut.Value, DateTimeKind.Utc), istZone);
                        outTimeStr = outTimeIst.ToString("hh:mm tt");
                    }

                    var dict = new Dictionary<string, string>
                    {
                        { "ManagerName", $"{manager.FirstName} {manager.LastName}".Trim() },
                        { "Date", request.RequestedDate.ToString("dd-MM-yyyy") },
                        { "InTime", inTimeIst.ToString("hh:mm tt") },
                        { "OutTime", outTimeStr },
                        { "EmployeeName", $"{employee.FirstName} {employee.LastName}".Trim() },
                        { "ManagerEmail", manager.EmailAddress },
                        { "RegularizationReason", attendance.RegularizationReason ?? "Regularization Request" }
                    };
                    await _sendEmailService.SendTemplateEmailAsync(manager.EmailAddress, $"Regularization Request from {employee.FirstName} {employee.LastName}", dict, "RegularizationRequest");
                }
            }
            catch
            {
                // Non-blocking email sending
            }

            return result > 0;
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


