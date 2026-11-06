using System.Net;
using KHRMS.Authorization;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;
        private readonly IUserContextService _userContext;

        public DepartmentController(IDepartmentService departmentService, IUserContextService userContext)
        {
            _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }

        /// <summary>
        /// Retrieves all active departments.
        /// </summary>
        [HttpGet]
        [HttpGet("GetDepartments")]
        public async Task<IActionResult> GetDepartments()
        {
            Log.Information("GetDepartments API called.");
            var departments = await _departmentService.GetAllDepartmentsAsync();

            return Ok(new ApiResponse<IEnumerable<DepartmentDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.DepartmentsFound,
                Data = departments
            });
        }

        /// <summary>
        /// Retrieves a specific department by its primary ID.
        /// </summary>
        [HttpGet("{id}")]
        [HttpGet("GetDepartmentById/{id}")]
        public async Task<IActionResult> GetDepartmentById(long id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid department identifier.",
                    Data = null
                });
            }

            var dept = await _departmentService.GetDepartmentByIdAsync(id);
            if (dept == null)
            {
                return NotFound(new ApiResponse<DepartmentDto>
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = ApiMessageConstant.DepartmentNotFound,
                    Data = null
                });
            }

            return Ok(new ApiResponse<DepartmentDto>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.DepartmentFound,
                Data = dept
            });
        }

        /// <summary>
        /// Creates a new department. Requires Admin or HR role with DEPARTMENT_MANAGE permission.
        /// </summary>
        [HttpPost]
        [HttpPost("AddDepartment")]
        [Authorize(Roles = "Admin,HR")]
        [RequirePermission("DEPARTMENT_MANAGE")]
        public async Task<IActionResult> AddDepartment([FromBody] CreateDepartmentRequest request)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Department name is required.",
                    Data = null
                });
            }

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var created = await _departmentService.CreateDepartmentAsync(request, currentUserId);
                return CreatedAtAction(nameof(GetDepartmentById), new { id = created.Id }, new ApiResponse<DepartmentDto>
                {
                    StatusCode = (int)HttpStatusCode.Created,
                    Message = ApiMessageConstant.DepartmentAdded,
                    Data = created
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<DepartmentDto>
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Updates an existing department. Requires Admin or HR role with DEPARTMENT_MANAGE permission.
        /// </summary>
        [HttpPut("{id}")]
        [HttpPut("UpdateDepartment/{id}")]
        [Authorize(Roles = "Admin,HR")]
        [RequirePermission("DEPARTMENT_MANAGE")]
        public async Task<IActionResult> UpdateDepartment(long id, [FromBody] UpdateDepartmentRequest request)
        {
            if (id <= 0 || !ModelState.IsValid || string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Valid department ID and name are required.",
                    Data = null
                });
            }

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var updated = await _departmentService.UpdateDepartmentAsync(id, request, currentUserId);
                if (updated == null)
                {
                    return NotFound(new ApiResponse<DepartmentDto>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = ApiMessageConstant.DepartmentNotFound,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<DepartmentDto>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.DepartmentUpdated,
                    Data = updated
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new ApiResponse<DepartmentDto>
                {
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// Soft-deletes a department. Requires Admin or HR role with DEPARTMENT_MANAGE permission.
        /// </summary>
        [HttpDelete("{id}")]
        [HttpDelete("DeleteDepartment/{id}")]
        [Authorize(Roles = "Admin,HR")]
        [RequirePermission("DEPARTMENT_MANAGE")]
        public async Task<IActionResult> DeleteDepartment(long id)
        {
            if (id <= 0)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid department identifier.",
                    Data = false
                });
            }

            try
            {
                long currentUserId = _userContext.GetCurrentEmployeeId();
                var deleted = await _departmentService.DeleteDepartmentAsync(id, currentUserId);
                if (!deleted)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = ApiMessageConstant.DepartmentNotFound,
                        Data = false
                    });
                }

                return Ok(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Message = ApiMessageConstant.DepartmentDeleted,
                    Data = true
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
        }

        /// <summary>
        /// Backfills relational Department entities and updates existing Employee records based on historical data.
        /// Safe and idempotent to execute. Requires Admin role.
        /// </summary>
        [HttpPost("Backfill")]
        [Authorize(Roles = "Admin")]
        [RequirePermission("DEPARTMENT_MANAGE")]
        public async Task<IActionResult> Backfill()
        {
            Log.Information("Backfill departments endpoint triggered.");
            int count = await _departmentService.BackfillDepartmentsFromExistingEmployeesAsync();
            return Ok(new ApiResponse<int>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = $"{ApiMessageConstant.DepartmentBackfilled} (Updated {count} employee records).",
                Data = count
            });
        }
    }
}
