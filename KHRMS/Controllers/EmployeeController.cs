using KHRMS.Services.Interfaces;
using KHRMS.Authorization;
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
    public class EmployeeController(IEmployeeService employeeService, IUserContextService userContextService, IEmployeeAccessService? employeeAccessService = null) : ControllerBase
    {
        public readonly IEmployeeService _employeeService = employeeService;
        private readonly IUserContextService _userContext = userContextService;
        private readonly IEmployeeAccessService? _employeeAccessService = employeeAccessService;


        /// <summary>
        /// Lightweight employee lookup list (Id, FullName, Designation, Email, ProfileImage) for dropdowns, filters, and pickers.
        /// Excludes sensitive PII (addresses, educational percentages, passport numbers, emergency contacts, banking).
        /// </summary>
        [HttpGet("GetEmployeesLookup")]
        [RequirePermission("EMPLOYEE_VIEW_DIRECTORY")]
        public async Task<IActionResult> GetEmployeesLookup()
        {
            Log.Information("EmployeeController - GetEmployeesLookup called.");

            var employees = await _employeeService.GetEmployeesLookupAsync();
            return Ok(new ApiResponse<IEnumerable<EmployeeLookupDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = ApiMessageConstant.EmployeeFound,
                Data = employees
            });
        }


        /// <summary>
        /// Get the list of employees
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetEmployees")]
        [RequirePermission("EMPLOYEE_VIEW_DIRECTORY")]
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

            var canManage = await _userContext.HasPermissionAsync("EMPLOYEE_MANAGE") || _userContext.IsAdmin() || _userContext.IsHR();
            var currentEmpId = _userContext.GetCurrentEmployeeId();

            if (!canManage)
            {
                foreach (var emp in employees)
                {
                    if (emp.Id != currentEmpId)
                    {
                        emp.Responsibilities = null;
                        emp.PassportNumber = null;
                        emp.PassportScanCopy = null;
                        emp.PassportIssueDate = null;
                        emp.PassportExpiryDate = null;
                        emp.PermanentAddress = null;
                        emp.PrimaryContactPhone = null;
                        emp.PrimaryContactEmail = null;
                        emp.PrimaryContactAddress = null;
                        emp.SecondaryContactPhone = null;
                        emp.SecondaryContactEmail = null;
                        emp.SecondaryContactAddress = null;
                    }
                }
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
                else
                {
                    targetId = (int)_userContext.GetCurrentEmployeeId();
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

            var canManage = await _userContext.HasPermissionAsync("EMPLOYEE_MANAGE") || _userContext.IsAdmin() || _userContext.IsHR();
            var currentEmpId = _userContext.GetCurrentEmployeeId();

            if (!canManage && targetId != currentEmpId)
            {
                employee.Responsibilities = null;
                employee.PassportNumber = null;
                employee.PassportScanCopy = null;
                employee.PassportIssueDate = null;
                employee.PassportExpiryDate = null;
                employee.PermanentAddress = null;
                employee.PrimaryContactPhone = null;
                employee.PrimaryContactEmail = null;
                employee.PrimaryContactAddress = null;
                employee.SecondaryContactPhone = null;
                employee.SecondaryContactEmail = null;
                employee.SecondaryContactAddress = null;
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
        [RequirePermission("EMPLOYEE_MANAGE")]
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

            bool isEmployeeAdded = false;
            try
            {
                isEmployeeAdded = await _employeeService.CreateEmployee(employeerequestModel);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                Log.Warning("EmployeeController - Add employee rejected: {Message}", ex.Message);
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
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
            long currentEmpId = 0;
            bool isSelf = !string.IsNullOrEmpty(claimVal) && long.TryParse(claimVal, out currentEmpId) && currentEmpId == employeeRequestModel.Id;

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

            if (!isSelf)
            {
                if (!isAuthorizedStaff)
                {
                    return Forbid();
                }

                {
    
                }

                // SEC-03 (BOLA Fix): If user is a Manager (not Admin/HR), verify target employee is within their reporting hierarchy
                if (_employeeAccessService != null)
                {
                    bool canAccess = await _employeeAccessService.CanAccessEmployeeAsync(employeeRequestModel.Id, "EMPLOYEE_MANAGE");
                    if (!canAccess)
                    {
                        Log.Warning("EmployeeController - Forbidden profile update: Caller {CurrentEmpId} is not authorized to modify Employee {TargetEmpId} outside reporting hierarchy",
                            currentEmpId, employeeRequestModel.Id);
                        return StatusCode((int)HttpStatusCode.Forbidden, new ApiResponse<bool>
                        {
                            StatusCode = (int)HttpStatusCode.Forbidden,
                            Message = "You are only permitted to update profile details for employees within your direct or indirect reporting line.",
                            Data = false
                        });
                    }
                }
            }

            {

            }

            // SEC-01: Safeguard against mass assignment & privilege escalation
            // Only Admin / HR roles are allowed to modify role assignments, responsibilities/salary JSON, employment status, DOJ, manager, or shift
            bool canManageRolesAndCoreFields = User.IsInRole("Admin") || User.IsInRole("System Admin")
                || User.IsInRole("HR") || User.IsInRole("HR Operations")
                || userRoles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r, "System Admin", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r, "HR", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r, "HR Operations", StringComparison.OrdinalIgnoreCase));

            if (!canManageRolesAndCoreFields)
            {
                var existingEmp = await _employeeService.GetEmployeeById((int)employeeRequestModel.Id);
                if (existingEmp != null)
                {
                    employeeRequestModel.IsActive = existingEmp.IsActive;
                }
                employeeRequestModel.RoleIds = null;
                employeeRequestModel.Responsibilities = null;
                employeeRequestModel.DateOfJoining = null;
                employeeRequestModel.ManagerId = null;
                employeeRequestModel.DesignationId = null;
                employeeRequestModel.ShiftId = null;
                employeeRequestModel.EmployeeCode = null;
                employeeRequestModel.ProbationEndDate = null;
                employeeRequestModel.ProbationStatus = null;
                employeeRequestModel.EmploymentStatus = null;
                employeeRequestModel.EmploymentType = null;
            }

            bool isEmployeeEdited = false;
            try
            {
                isEmployeeEdited = await _employeeService.UpdateEmployee(employeeRequestModel);
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                Log.Warning("EmployeeController - Update employee rejected: {Message}", ex.Message);
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
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
        [RequirePermission("EMPLOYEE_MANAGE")]
        public async Task<IActionResult> DeleteEmployee(long employeeId)
        {
            Log.Information("EmployeeController - DeleteEmployee called for ID: {EmployeeId}", employeeId);

            try
            {
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
            catch (InvalidOperationException ex)
            {
                Log.Warning(ex, "Safeguard blocked DeleteEmployee for ID: {EmployeeId}", employeeId);
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
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
        [RequirePermission("EMPLOYEE_MANAGE")]
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
            var currentUserId = _userContext.GetCurrentEmployeeId();
            var isPrivileged = _userContext.IsAdmin() || _userContext.IsHR() || await _userContext.HasPermissionAsync("EMPLOYEE_MANAGE");

            if (!isPrivileged)
            {
                if (request.EmployeeId > 0 && request.EmployeeId != currentUserId)
                {
    
                }
                request.EmployeeId = currentUserId;
            }

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

        [HttpGet("probation-reminders")]
        public async Task<IActionResult> GetProbationReminders()
        {
            Log.Information("GetProbationReminders API called.");
            var reminders = await _employeeService.GetProbationRemindersAsync();
            return Ok(new ApiResponse<IEnumerable<EmployeeRequestModel>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Probation reminder list retrieved successfully",
                Data = reminders
            });
        }

        [HttpPost("{id}/probation-decision")]
        public async Task<IActionResult> ProcessProbationDecision(long id, [FromBody] ProbationDecisionDto dto)
        {
            Log.Information("ProcessProbationDecision API called for employee {EmployeeId}: {Decision}", id, dto.Decision);
            try
            {
                var success = await _employeeService.ProcessProbationDecisionAsync(id, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = success ? $"Probation decision '{dto.Decision}' processed successfully." : "Failed to process probation decision.",
                    Data = success
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                Log.Warning("EmployeeController - ProcessProbationDecision validation rejected: {Message}", ex.Message);
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EmployeeController - Unexpected error processing probation decision for employee {EmployeeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while processing the probation decision. Please try again later.",
                    Data = false
                });
            }
        }

        [HttpGet("matching-hints")]
        public async Task<IActionResult> GetMatchingFormerEmployees([FromQuery] string? email, [FromQuery] string? pan, [FromQuery] string? uan)
        {
            var matches = await _employeeService.GetMatchingFormerEmployeesAsync(email, pan, uan);
            return Ok(new ApiResponse<IEnumerable<MatchingFormerEmployeeDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Matching former employee hints retrieved",
                Data = matches
            });
        }

        [HttpGet("historical-rehire-report")]
        public async Task<IActionResult> GetHistoricalRehireReport()
        {
            var report = await _employeeService.GetHistoricalRehiresReportAsync();
            return Ok(new ApiResponse<IEnumerable<HistoricalRehireReportItemDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Historical rehire report retrieved",
                Data = report
            });
        }

        [HttpGet("{id}/previous-employment")]
        public async Task<IActionResult> GetPreviousEmploymentHistory(long id)
        {
            var history = await _employeeService.GetPreviousEmploymentHistoryAsync(id);
            return Ok(new ApiResponse<IEnumerable<PreviousEmploymentHistoryDto>>
            {
                StatusCode = (int)HttpStatusCode.OK,
                Message = "Previous employment history retrieved",
                Data = history
            });
        }

        [HttpPut("{id}/notice-override")]
        public async Task<IActionResult> UpdateNoticeOverride(long id, [FromBody] NoticePeriodOverrideDto dto)
        {
            try
            {
                var success = await _employeeService.UpdateNoticeOverrideAsync(id, dto);
                return Ok(new ApiResponse<bool>
                {
                    StatusCode = success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.BadRequest,
                    Message = success ? "Notice period override updated successfully." : "Failed to update notice period override.",
                    Data = success
                });
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                Log.Warning("EmployeeController - UpdateNoticeOverride validation rejected: {Message}", ex.Message);
                return BadRequest(new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = ex.Message,
                    Data = false
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "EmployeeController - Unexpected error updating notice override for employee {EmployeeId}", id);
                return StatusCode((int)HttpStatusCode.InternalServerError, new ApiResponse<bool>
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred while updating the notice period override. Please try again later.",
                    Data = false
                });
            }
        }
    }
}
