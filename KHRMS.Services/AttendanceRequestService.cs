using KHRMS.Core;
using System.Security.Claims;


namespace KHRMS.Services
{
    public class AttendanceRequestService(IUnitOfWork unitOfWork) : IAttendanceRequestService
    {
        public IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IEnumerable<AttendanceRequest>> GetAllAsync()
        {
            var attendanceRequests = await _unitOfWork.AttendanceRequests.GetAll();
            return attendanceRequests;
        }

        public async Task<AttendanceRequest> GetByIdAsync(long id)
        {
            var attendanceRequests = await _unitOfWork.AttendanceRequests.GetById(id);
            return attendanceRequests;
        }

      

        public async Task AddAsync(AttendanceRequest attendanceRequest, ClaimsPrincipal user)
        {
            if (attendanceRequest == null)
                throw new ArgumentNullException(nameof(attendanceRequest));

            // Get the Employee ID from the logged-in user
            var employeeIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (employeeIdClaim == null)
                throw new Exception("User identity not found.");

            long employeeId = long.Parse(employeeIdClaim.Value); // Convert to long if necessary

            // Fetch the employee details to get ManagerId
            var employee = await _unitOfWork.Employees.GetById(employeeId);
            if (employee == null)
                throw new Exception("Employee not found.");

            // Assign the Employee ID and Manager ID automatically
            attendanceRequest.EmployeeId = employee.Id;
            attendanceRequest.ManagerId = employee.ManagerId;

            await _unitOfWork.AttendanceRequests.Add(attendanceRequest);
            var result = _unitOfWork.Save();
        }


        public Task UpdateAsync(AttendanceRequest attendanceRequest)
        {
            _unitOfWork.AttendanceRequests.Update(attendanceRequest);
            var result = _unitOfWork.Save();
            return Task.CompletedTask;
        }

        public Task DeleteAsync(long id)
        {
            _unitOfWork.AttendanceRequests.DeleteAsync(id);
            var result = _unitOfWork.Save();
            return Task.CompletedTask;
        }
    }
}





