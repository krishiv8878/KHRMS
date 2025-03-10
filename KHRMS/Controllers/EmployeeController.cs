using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Mvc;
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
        [HttpGet]
        [Route("GetEmployees")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _employeeService.GetAllEmployees();
            if (employees == null)
            {
                return NotFound();
            }
            // Use the wrapper class to create a consistent response
            var response = new ApiResponse<List<EmployeeRequestModel>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = employees.Any() ? ApiMessageConstant.EmployeeFound : ApiMessageConstant.NoEmployeeFound,
                Data = employees.ToList()
            };
            return Ok(response);
        }

        /// <summary>
        /// Add a new employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddEmployee")]
        public async Task<IActionResult> AddEmployee(EmployeeRequestModel employeerequestModel)
        {
            if (!ModelState.IsValid)
            {
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
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeAdded,
                    Data = isEmployeeAdded
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeNotAdded,
                    Data = isEmployeeAdded
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Update a existing employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateEmployee")]
        public async Task<IActionResult> UpdateEmployee(EmployeeRequestModel employeeRequestModel)
        {
            if (!ModelState.IsValid)
            {
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
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeUpdated,
                    Data = isEmployeeEdited
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeNotUpdated,
                    Data = isEmployeeEdited
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Delete existing employee
        /// </summary>
        /// <param name="employee"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteEmployee")]
        public async Task<IActionResult> DeleteEmployee(long employeeId)
        {
            var isEmployeeDeleted = await _employeeService.DeleteEmployee(employeeId);
            if (isEmployeeDeleted)
            {
                // Use the wrapper class to create a consistent response
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.EmployeeDeleted,
                    Data = isEmployeeDeleted
                };
                return Ok(response);
            }
            else
            {
                var response = new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ApiMessageConstant.EmployeeNotDeleted,
                    Data = isEmployeeDeleted
                };
                return BadRequest(response);
            }
        }


        [HttpGet("GetManagers")]
        public async Task<IActionResult> GetManagers()
        {
            var managers = await _employeeService.GetAllManagers();
            return Ok(managers);
        }

    }
}
