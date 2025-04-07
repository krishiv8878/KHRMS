using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Net;


namespace KHRMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController(IEmployeeService employeeService) : ControllerBase
    {
        public readonly IEmployeeService _employeeService = employeeService;

        /// <summary>
        /// Get the list of employees
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        //[Route("GetEmployees")]
        //public async Task<IActionResult> GetEmployees()
        //{
        //    var employees = await _employeeService.GetAllEmployees();
        //    if (employees == null)
        //    {
        //        return NotFound();
        //    }
        //    // Use the wrapper class to create a consistent response
        //    var response = new ApiResponse<List<EmployeeRequestModel>>
        //    {
        //        StatusCode = (int)HttpStatusCode.OK,
        //        Message = employees.Any() ? ApiMessageConstant.EmployeeFound : ApiMessageConstant.NoEmployeeFound,
        //        Data = employees.ToList()
        //    };
        //    return Ok(response);
        //}
        [HttpGet("GetEmployees")]
        public async Task<IActionResult> GetEmployees()
        {
            Log.Information("EmployeeController - GetEmployees called.");

            var employees = await _employeeService.GetAllEmployees();
            if (employees == null || !employees.Any())
            {
                Log.Warning("EmployeeController - No employees found.");
                return NotFound(new ApiResponse<List<EmployeeRequestModel>>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
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
        /// Add a new employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("AddEmployee")]
        //public async Task<IActionResult> AddEmployee(EmployeeRequestModel employeerequestModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidData,
        //            Data = false
        //        });
        //    }
        //    var isEmployeeAdded = await _employeeService.CreateEmployee(employeerequestModel);
        //    if (isEmployeeAdded)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.EmployeeAdded,
        //            Data = isEmployeeAdded
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.EmployeeNotAdded,
        //            Data = isEmployeeAdded
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpPost("AddEmployee")]
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
        //[HttpPut]
        //[Route("UpdateEmployee")]
        //public async Task<IActionResult> UpdateEmployee(EmployeeRequestModel employeeRequestModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.InvalidData,
        //            Data = false
        //        });
        //    }
        //    var isEmployeeEdited = await _employeeService.UpdateEmployee(employeeRequestModel);
        //    if (isEmployeeEdited)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.EmployeeUpdated,
        //            Data = isEmployeeEdited
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.EmployeeNotUpdated,
        //            Data = isEmployeeEdited
        //        };
        //        return BadRequest(response);
        //    }
        //}
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
        //[HttpDelete]
        //[Route("DeleteEmployee")]
        //public async Task<IActionResult> DeleteEmployee(long employeeId)
        //{
        //    var isEmployeeDeleted = await _employeeService.DeleteEmployee(employeeId);
        //    if (isEmployeeDeleted)
        //    {
        //        // Use the wrapper class to create a consistent response
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.OK,
        //            Message = ApiMessageConstant.EmployeeDeleted,
        //            Data = isEmployeeDeleted
        //        };
        //        return Ok(response);
        //    }
        //    else
        //    {
        //        var response = new ApiResponse<bool>
        //        {
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = ApiMessageConstant.EmployeeNotDeleted,
        //            Data = isEmployeeDeleted
        //        };
        //        return BadRequest(response);
        //    }
        //}
        [HttpDelete("DeleteEmployee")]
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
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeNotDeleted,
                    Data = false
                });
            }
        }


        //[HttpGet("GetManagers")]
        //public async Task<IActionResult> GetManagers()
        //{
        //    var managers = await _employeeService.GetAllManagers();
        //    return Ok(managers);
        //}
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
    }
}
