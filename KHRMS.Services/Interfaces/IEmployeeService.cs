using KHRMS.Core;
using KHRMS.Services.Request;
using System.Linq.Expressions;

namespace KHRMS.Services
{
    public interface IEmployeeService
    {
        Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel);
        Task<IEnumerable<EmployeeRequestModel>> GetAllEmployees();
        Task<Employee> GetEmployeeById(int employeeId);
        Task<bool> UpdateEmployee(EmployeeRequestModel employeeRequestModel);
        Task<bool> ExistingEmployeeUpdate(EmployeeRequestModel employeeRequestModel);
        Task<bool> DeleteEmployee(long employeeId);
        Task<IEnumerable<EmployeeRequestModel>> GetAllManagers();
        Task<string?> UploadProfileImage(ProfileImageRequest request);

    }
}