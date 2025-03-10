using KHRMS.Core;
using KHRMS.Services.Request;

namespace KHRMS.Services
{
    public interface IEmployeeService
    {
        Task<bool> CreateEmployee(EmployeeRequestModel employeeRequestModel);
        Task<IEnumerable<EmployeeRequestModel>> GetAllEmployees();
        Task<Employee> GetEmployeeById(int employeeId);
        Task<bool> UpdateEmployee(EmployeeRequestModel employeeRequestModel);
        Task<bool> DeleteEmployee(long employeeId);
        Task<IEnumerable<EmployeeRequestModel>> GetAllManagers();
    }
}