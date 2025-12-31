using KHRMS.Core;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using MimeKit.Cryptography;
using System.Linq;
using System.Security.Claims;


namespace KHRMS.Services
{
    public class AttendanceRequestService : IAttendanceRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly IUserContextService _userContextService;
        private readonly ISendEmailService _emailService;

        public AttendanceRequestService(IUnitOfWork unitOfWork, IEmployeeAttendanceService employeeAttendanceService, IUserContextService userContextService, ISendEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _userContextService = userContextService;
            _employeeAttendanceService = employeeAttendanceService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<AttendanceRequestUpdateDTO>> GetAllAsync()
        {
            long manager = _userContextService.GetCurrentEmployeeId();
            var attendance = await _unitOfWork.AttendanceRequests.GetAll();
            var employees = await _unitOfWork.Employees.GetAll();

            var attendanceDtos = attendance
                .Where(x => x.ManagerId == manager)
                .Join(employees,
                      a => a.EmployeeId,
                      e => e.Id,
                      (a, e) => new AttendanceRequestUpdateDTO
                      {
                          Id = a.Id,
                          EmployeeId = a.EmployeeId,
                          EmployeeName = e.FirstName +" "+ e.LastName,
                          RequestType = a.RequestType,
                          RequestedDate = a.RequestedDate,
                          RequestedBy = a.RequestedBy,
                          Reason = a.Reason,
                          Status = a.Status,
                          LastActionBy = (long)a.LastActionBy,
                          clockIn = a.ClockInTime,
                          clockOut = (DateTime)a.ClockOutTime,
                          ManagerId = a.ManagerId,
                      })
                .ToList();

            return attendanceDtos;
        }

        public async Task<AttendanceRequest> GetByIdAsync(long id)
        {
            var attendanceRequests = await _unitOfWork.AttendanceRequests.GetById(id);
            return attendanceRequests;
        }

        //public async Task AddAsync(AttendanceRequest attendanceRequest)
        //{
        //    await _unitOfWork.AttendanceRequests.Add(attendanceRequest);
        //    var result = _unitOfWork.Save();

        //}
        //public async Task AddAsync(AttendanceRequest attendanceRequest)
        //{
        //    if (attendanceRequest == null)
        //        throw new ArgumentNullException(nameof(attendanceRequest));

        //    // Fetch the employee details to get ManagerId
        //    var employee = await _unitOfWork.Employees.GetById(attendanceRequest.EmployeeId);
        //    if (employee == null)
        //        throw new Exception("Employee not found.");

        //    // Assign the manager's ID automatically
        //    attendanceRequest.ManagerId = employee.ManagerId;

        //    await _unitOfWork.AttendanceRequests.Add(attendanceRequest);
        //    var result = _unitOfWork.Save();
        //}


        public async Task AddAsync(AttendanceRequestDTO attendanceRequest, ClaimsPrincipal user)
        {
            if (attendanceRequest == null)
                throw new ArgumentNullException(nameof(attendanceRequest));

            // Get the Employee ID from the logged-in user
            var employeeIdClaim = user.FindFirst("UserId")?.Value;
            if (employeeIdClaim == null)
                throw new Exception("User identity not found.");

            long employeeId = long.Parse(employeeIdClaim); // Convert to long if necessary

            // Fetch the employee details to get ManagerId
            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee == null)
                throw new Exception("Employee not found.");
            var manager = (await _unitOfWork.Employees.GetAll()).FirstOrDefault(t => t.Id == employee.ManagerId);
            if (manager == null)
                throw new Exception("Manager not found.");

            try
            {
                //string formattedDate = attendanceRequest.RequestedDate.ToString("dd-MM-yyyy");
                //string inTime = attendanceRequest.clockIn.ToString("hh:mm tt");
                //string outTime = attendanceRequest.clockOut?.ToString("hh:mm tt");

                //var managerEmail = manager.EmailAddress;
                //var managerName = $"{manager.FirstName} {manager.LastName}";
                //var employeeName = $"{employee.FirstName} {employee.LastName}";
                //var regularizationType = "RegularizationRequest";

                //var dict = new Dictionary<string, string>
                //{
                //    { "ManagerName", managerName },
                //    { "Date", formattedDate },
                //    { "InTime", inTime },
                //    { "OutTime", outTime },
                //    { "EmployeeName", employeeName },
                //    { "ManagerEmail", managerEmail },
                //    { "RegularizationReason", attendanceRequest.Reason ?? "No specific reason provided." }
                //};

                //var subject = $"Regularization Request from {employeeName} for {formattedDate}";

                //await _emailService.SendTemplateEmailAsync(managerEmail, subject, dict, regularizationType);
                var request = new AttendanceRequest
                {
                    EmployeeId = employee.Id,
                    RequestType = attendanceRequest.RequestType,
                    RequestedDate = attendanceRequest.RequestedDate,
                    RequestedBy = employee.Id,
                    Reason = attendanceRequest.Reason,
                    Status = "pending",
                    LastActionBy = employee.Id,
                    ClockInTime = attendanceRequest.clockIn,
                    ClockOutTime = attendanceRequest.clockOut,
                    CreatedDate = DateTime.Now,
                    ManagerId = employee.ManagerId,
                };
                await _unitOfWork.AttendanceRequests.Add(request);
                var result = _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        public async Task UpdateAsync(AttendanceRequestUpdateDTO attendanceRequest)
        {
            var request = await _unitOfWork.AttendanceRequests.GetById(attendanceRequest.Id);
            request.Reason = attendanceRequest.Reason;
            request.Status = attendanceRequest.Status;
            request.LastActionBy = attendanceRequest.LastActionBy;
            request.UpdatedBy = attendanceRequest.UpdatedBy;
            request.UpdatedDate = attendanceRequest.UpdatedDate;
            request.IsActive = attendanceRequest.IsActive;
            request.IsDeleted = attendanceRequest.IsDeleted;
            _unitOfWork.AttendanceRequests.Update(request);
           
            if(attendanceRequest.Status == "Approved")
            {
                TimeSpan totalhrs = (TimeSpan)(attendanceRequest.clockOut - attendanceRequest.clockIn);
                var th = totalhrs.TotalHours;
                var emp = new EmployeeAttendance
                {
                    EmployeeId = attendanceRequest.EmployeeId,
                    ClockIn = attendanceRequest.clockIn,
                    ClockOut = (DateTime)attendanceRequest.clockOut,
                    TotalHours = (decimal)th,
                    EffectiveHours = (decimal)th,
                    AttendanceDate = DateOnly.FromDateTime(attendanceRequest.RequestedDate)
                };
                await _employeeAttendanceService.AddAsync(emp);
            }
            _unitOfWork.Save();
            return;
        }

        public Task DeleteAsync(long id)
        {
            _unitOfWork.AttendanceRequests.DeleteAsync(id);
            var result = _unitOfWork.Save();
            return Task.CompletedTask;
        }
    }
}





