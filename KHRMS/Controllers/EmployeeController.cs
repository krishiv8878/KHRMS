using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;


namespace KHRMS
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {
        public readonly IEmployeeService _employeeService = employeeService;


        /// <summary>
        /// Get the list of employees
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetEmployees")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations,Manager,Management")]
        public async Task<IActionResult> GetEmployees()
        {
            Log.Information("EmployeeController - GetEmployees called.");

            var employees = await _employeeService.GetAllEmployees();
            if (employees == null || !employees.Any())
            {
                Log.Warning("EmployeeController - No employees found.");
                return Ok(new ApiResponse<List<EmployeeRequestModel>>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.NoEmployeeFound,
                    Data = null
                });
            }

            Log.Information("EmployeeController - {Count} employees found.", employees.Count());
            return Ok(new ApiResponse<List<EmployeeRequestModel>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeFound,
                Data = employees.ToList()
            });
        }

        /// <summary>
        /// Get employee by ID (accessible by self, or Admin/HR/Manager)
        /// </summary>
        [HttpGet("GetEmployeeById/{id?}")]
        public async Task<IActionResult> GetEmployeeById(int? id)
        {
            Log.Information("EmployeeController - GetEmployeeById called for ID: {Id}", id);

            int targetId = id ?? 0;
            if (targetId <= 0)
            {
                var claimVal = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(claimVal) && int.TryParse(claimVal, out int parsedClaimId))
                {
                    targetId = parsedClaimId;
                }
            }

            if (targetId <= 0)
            {
                return BadRequest(new ApiResponse<Employee>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid Employee ID is required",
                    Data = null
                });
            }

            var employee = await _employeeService.GetEmployeeById(targetId);
            if (employee == null)
            {
                return Ok(new ApiResponse<Employee>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.NoEmployeeFound,
                    Data = null
                });
            }

            return Ok(new ApiResponse<Employee>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeFound,
                Data = employee
            });
        }

        /// <summary>
        /// Add a new employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost("AddEmployee")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        public async Task<IActionResult> AddEmployee(EmployeeRequestModel employeerequestModel)
        {
            Log.Information("EmployeeController - AddEmployee called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("EmployeeController - Invalid model state.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            var isEmployeeAdded = await _employeeService.CreateEmployee(employeerequestModel);
            if (isEmployeeAdded)
            {
                Log.Information("EmployeeController - Employee added successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeAdded,
                    Data = true
                });
            }
            else
            {
                Log.Error("EmployeeController - Failed to add employee.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeNotAdded,
                    Data = false
                });
            }
        }

        /// <summary>
        /// Update a existing employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPut("UpdateEmployee")]
        public async Task<IActionResult> UpdateEmployee(EmployeeRequestModel employeeRequestModel)
        {
            Log.Information("EmployeeController - UpdateEmployee called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("EmployeeController - Invalid model state during update.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            var claimVal = User.FindFirst("UserId")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("id")?.Value;
            bool isSelf = !string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out long currentEmpId) && currentEmpId == employeeRequestModel.Id;

            var userRoles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value)
                .Union(User.FindAll("role").Select(c => c.Value))
                .ToList();

            bool isAuthorizedStaff = User.IsInRole("Admin") || User.IsInRole("System Admin")
                || User.IsInRole("HR") || User.IsInRole("HR Operations")
                || User.IsInRole("Manager") || User.IsInRole("Management")
                || userRoles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r, "System Admin", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r, "HR", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r, "HR Operations", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r, "Manager", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r, "Management", StringComparison.OrdinalIgnoreCase));

            if (!isSelf && !isAuthorizedStaff)
            {
                return Forbid();
            }

            var isEmployeeEdited = await _employeeService.UpdateEmployee(employeeRequestModel);
            if (isEmployeeEdited)
            {
                Log.Information("EmployeeController - Employee updated successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeUpdated,
                    Data = true
                });
            }
            else
            {
                Log.Error("EmployeeController - Failed to update employee.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeNotUpdated,
                    Data = false
                });
            }
        }

        /// <summary>
        /// Delete existing employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpDelete("DeleteEmployee")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations")]
        public async Task<IActionResult> DeleteEmployee(long employeeId)
        {
            Log.Information("EmployeeController - DeleteEmployee called for ID: {EmployeeId}", employeeId);

            var isEmployeeDeleted = await _employeeService.DeleteEmployee(employeeId);
            if (isEmployeeDeleted)
            {
                Log.Information("EmployeeController - Employee with ID {EmployeeId} deleted.", employeeId);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeDeleted,
                    Data = true
                });
            }
            else
            {
                Log.Error("EmployeeController - Failed to delete employee with ID: {EmployeeId}", employeeId);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.NoEmployeeFound,
                    Data = false
                });
            }
        }

        // <summary>
        /// Update a existing employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPut("UpdateExistingEmployee")]
        [Authorize(Roles = "Admin,System Admin,HR,HR Operations,Manager,Management")]
        public async Task<IActionResult> UpdateExistingEmployee(EmployeeRequestModel employeeRequestModel)
        {
            Log.Information("EmployeeController - UpdateEmployee called.");

            if (!ModelState.IsValid)
            {
                Log.Warning("EmployeeController - Invalid model state during update.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.InvalidData,
                    Data = false
                });
            }

            var isEmployeeEdited = await _employeeService.ExistingEmployeeUpdate(employeeRequestModel);
            if (isEmployeeEdited)
            {
                Log.Information("EmployeeController - Employee updated successfully.");
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeUpdated,
                    Data = true
                });
            }
            else
            {
                Log.Error("EmployeeController - Failed to update employee.");
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeNotUpdated,
                    Data = false
                });
            }
        }

        [HttpGet("GetManagers")]
        public async Task<IActionResult> GetManagers()
        {
            Log.Information("EmployeeController - GetManagers called.");
            var managers = await _employeeService.GetAllManagers();

            if (managers == null || !managers.Any())
            {
                Log.Warning("EmployeeController - No managers found.");
            }
            else
            {
                Log.Information("EmployeeController - {Count} managers found.", managers.Count());
            }

            return Ok(managers);
        }

        [HttpPost("UploadProfileImage")]
        public async Task<IActionResult> UploadProfileImage([FromForm] ProfileImageRequest request)
        {
            var result = await _employeeService.UploadProfileImage(request);

            if (result == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeProfileUploadFailed,
                    Data = null
                });
            }

            return Ok(new ApiResponse<string>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeProfileUploadSuccess,
                Data = result
            });
        }
    }
}
