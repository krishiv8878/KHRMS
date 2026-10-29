using System.ComponentModel;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using KHRMS.AI;
using KHRMS.Authorization;
using KHRMS.Core;
using KHRMS.Infrastructure;
using KHRMS.Services;
using KHRMS.Services.Interfaces;
using KHRMS.Services.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace KHRMS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(
        IChatClient chatClient,
        ILeaveRequestTypeService leaveService,
        IAttendanceRequestService attendanceService,
        IPayrollService payrollService,
        IHolidayService holidayService,
        IUserContextService userContext,
        IChatPolicyService policyService,
        IUnitOfWork unitOfWork) : ControllerBase
    {
        // ──────────────────────────────────────────────────────────────────────
        // POST api/chat
        // All authenticated employees may chat with the AI.
        // ──────────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequest request,
            CancellationToken ct)
        {
            if (request == null || request.Messages == null)
            {
                return BadRequest("Invalid chat request.");
            }

            // 1. Build conversation history with dynamic context-aware system prompt
            var empId = userContext.GetCurrentEmployeeId();
            var roles = userContext.GetCurrentUserRoles();
            var isPrivileged = userContext.IsAdmin() || userContext.IsHR() || userContext.IsManager();
            var emp = empId > 0 ? await unitOfWork.Employees.GetById(empId) : null;
            var empName = emp != null ? $"{emp.FirstName} {emp.LastName}".Trim() : (User.Identity?.Name ?? "Employee");

            var systemPrompt = HrSystemPrompt.BuildPrompt(DateTime.Today, empId, empName, roles, isPrivileged);

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, systemPrompt)
            };

            // Keep only recent messages (sliding window of 4) and sanitize heavy XML drafts to reduce input tokens
            var recentMessages = request.Messages.Count > 4
                ? request.Messages.Skip(request.Messages.Count - 4)
                : request.Messages;

            foreach (var msg in recentMessages)
            {
                var role = msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase)
                    ? ChatRole.User
                    : ChatRole.Assistant;

                var content = msg.Content ?? string.Empty;
                // Strip bulky <hr-action> blocks from past messages to prevent token inflation
                if (content.Contains("<hr-action>", StringComparison.OrdinalIgnoreCase))
                {
                    content = Regex.Replace(content, @"<hr-action>[\s\S]*?</hr-action>", "[Draft action prepared]").Trim();
                }

                messages.Add(new ChatMessage(role, content));
            }

            // 2. Register tools with execution tracker
            var executedTools = new List<(string Name, int ApproxTokens)>();
            var tools = BuildTools(executedTools);
            var options = new ChatOptions { Tools = tools };

            int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    // 3. Send to Gemini (function-invocation middleware runs tools automatically)
                    Microsoft.Extensions.AI.ChatResponse aiResponse = await chatClient.GetResponseAsync(messages, options, ct);
                    var rawReply = aiResponse.Text ?? string.Empty;

                    // 4. Parse any <hr-action> draft block embedded in the reply
                    var (cleanReply, draft) = ExtractDraft(rawReply);

                    // 5. Token Profiler logging
                    var sysPromptTokens = (systemPrompt.Length + 3) / 4;
                    var histTokens = recentMessages.Sum(m => (m.Content?.Length ?? 0) + 3) / 4;
                    var userQuery = request.Messages.LastOrDefault()?.Content ?? "(empty)";
                    var toolsLog = executedTools.Count > 0
                        ? string.Join(", ", executedTools.Select(t => $"{t.Name} (~{t.ApproxTokens} tok)"))
                        : "None (Direct Answer)";

                    var inputTokens = aiResponse.Usage?.InputTokenCount ?? 0;
                    var outputTokens = aiResponse.Usage?.OutputTokenCount ?? 0;
                    var totalTokens = aiResponse.Usage?.TotalTokenCount ?? 0;

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n======================= AI TOKEN PROFILER =======================");
                    Console.ResetColor();
                    Console.WriteLine($"[QUERY]              : \"{(userQuery.Length > 80 ? userQuery.Substring(0, 77) + "..." : userQuery)}\"");
                    Console.WriteLine($"[PROMPT TOKENS]      : ~{sysPromptTokens} tok (HR System Persona)");
                    Console.WriteLine($"[HISTORY TOKENS]     : ~{histTokens} tok ({recentMessages.Count()} messages retained)");
                    Console.WriteLine($"[TOOLS EXECUTED]     : {toolsLog}");
                    Console.WriteLine($"[ACTION DETECTED]    : {(draft != null ? $"YES ({draft.ActionType} -> {draft.Summary})" : "NO")}");
                    Console.WriteLine($"[API USAGE REPORTED] : Input: {inputTokens} | Output: {outputTokens} | Total: {totalTokens}");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("=================================================================\n");
                    Console.ResetColor();

                    return Ok(new KHRMS.AI.ChatResponse
                    {
                        Reply = cleanReply,
                        Draft = draft
                    });
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    Serilog.Log.Warning("ChatController - Gemini Rate limit hit (429) on attempt {Attempt}/{Max}", attempt, maxAttempts);
                    if (attempt == maxAttempts)
                    {
                        return Ok(new KHRMS.AI.ChatResponse
                        {
                            Reply = "⏳ The AI service is currently receiving a high volume of requests. Please wait a few seconds and try again.",
                            Draft = null
                        });
                    }
                    await Task.Delay(2000 * attempt, ct);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "ChatController - Error during AI chat completion");
                    return Ok(new KHRMS.AI.ChatResponse
                    {
                        Reply = "I am currently having trouble processing this request. Please try again in a moment.",
                        Draft = null
                    });
                }
            }

            return Ok(new KHRMS.AI.ChatResponse
            {
                Reply = "I am currently having trouble processing this request. Please try again in a moment.",
                Draft = null
            });
        }

        // ──────────────────────────────────────────────────────────────────────
        // Phase 4: Prepare & Confirm Endpoints for Leave & Regularization
        // ──────────────────────────────────────────────────────────────────────

        [HttpPost("prepare-leave")]
        public async Task<IActionResult> PrepareLeave([FromBody] PrepareLeaveDto dto)
        {
            if (dto.LeaveTypeId <= 0 || dto.StartDate == default || dto.EndDate == default)
            {
                return BadRequest(new { message = "Invalid leave details. Leave type, start date, and end date are required." });
            }

            var balances = await leaveService.GetEmployeeLeaveBalances();
            var matchedType = balances.FirstOrDefault(b => b.LeaveTypeId == dto.LeaveTypeId);
            var typeName = matchedType?.LeaveTypeName ?? "Leave";

            var summary = $"{typeName} ({dto.LeaveMode}) from {dto.StartDate:yyyy-MM-dd} to {dto.EndDate:yyyy-MM-dd}";

            var draft = new ChatDraft
            {
                ActionType = "leave",
                Summary = summary,
                ConfirmEndpoint = "api/chat/confirm-leave",
                Payload = new
                {
                    leaveTypeId = dto.LeaveTypeId,
                    startDate = dto.StartDate.ToString("yyyy-MM-dd"),
                    endDate = dto.EndDate.ToString("yyyy-MM-dd"),
                    leaveMode = dto.LeaveMode ?? "Full Day",
                    leaveReason = dto.LeaveReason ?? "Applied via AI Assistant",
                    isActive = true,
                    isDeleted = false
                }
            };

            return Ok(draft);
        }

        [HttpPost("confirm-leave")]
        [RequirePermission("LEAVE_APPLY_SELF")]
        public async Task<IActionResult> ConfirmLeave([FromBody] LeaveRequest leaveRequest)
        {
            if (leaveRequest == null)
            {
                return BadRequest("Invalid leave request data.");
            }

            // Ensure employee ID is set from JWT token
            leaveRequest.EmployeeId = userContext.GetCurrentEmployeeId();
            leaveRequest.Status = "Pending";
            leaveRequest.IsActive = true;
            leaveRequest.IsDeleted = false;

            try
            {
                var result = await leaveService.AddLeaveRequestType(leaveRequest);
                if (result)
                {
                    return Ok(new
                    {
                        statusCode = (int)HttpStatusCode.OK,
                        message = "Leave application submitted successfully.",
                        data = true
                    });
                }

                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Failed to submit leave application.",
                    data = false
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = ex.Message,
                    data = false
                });
            }
        }

        [HttpPost("prepare-regularization")]
        public IActionResult PrepareRegularization([FromBody] PrepareRegularizationDto dto)
        {
            if (dto.RequestedDate == default || dto.ClockIn == default)
            {
                return BadRequest(new { message = "Requested date and clock-in time are required." });
            }

            if (dto.RequestedDate.Date >= DateTime.Today)
            {
                return BadRequest(new { message = "Attendance regularization can only be requested for past completed workdays. You cannot regularize the current day or future dates." });
            }

            var summary = $"Attendance Regularization for {dto.RequestedDate:yyyy-MM-dd} (In: {dto.ClockIn:HH:mm}{(dto.ClockOut.HasValue ? $", Out: {dto.ClockOut.Value:HH:mm}" : "")})";

            var draft = new ChatDraft
            {
                ActionType = "regularization",
                Summary = summary,
                ConfirmEndpoint = "api/chat/confirm-regularization",
                Payload = new
                {
                    requestType = string.IsNullOrWhiteSpace(dto.RequestType) ? "Regularization" : dto.RequestType,
                    requestedDate = dto.RequestedDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    clockIn = dto.ClockIn.ToString("yyyy-MM-ddTHH:mm:ss"),
                    clockOut = dto.ClockOut?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    reason = dto.Reason ?? "Regularized via AI Assistant",
                    isActive = true,
                    isDeleted = false
                }
            };

            return Ok(draft);
        }

        [HttpPost("confirm-regularization")]
        [RequirePermission("ATTENDANCE_REGULARIZE_APPLY")]
        public async Task<IActionResult> ConfirmRegularization([FromBody] AttendanceRequestDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid attendance request data.");
            }

            if (dto.RequestedDate.Date >= DateTime.Today)
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = "Attendance regularization can only be requested for past completed workdays. You cannot regularize the current day or future dates.",
                    data = false
                });
            }

            dto.EmployeeId = userContext.GetCurrentEmployeeId();
            dto.Status = "Pending";
            dto.IsActive = true;
            dto.IsDeleted = false;

            try
            {
                await attendanceService.AddAsync(dto, User);
                return Ok(new
                {
                    statusCode = (int)HttpStatusCode.OK,
                    message = "Attendance regularization submitted successfully.",
                    data = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    statusCode = (int)HttpStatusCode.BadRequest,
                    message = ex.Message,
                    data = false
                });
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Tools — thin wrappers over existing services, no DB access,
        // employee ID always comes from the JWT via IUserContextService.
        // ──────────────────────────────────────────────────────────────────────
        private List<AITool> BuildTools(List<(string Name, int ApproxTokens)>? toolTracker = null)
        {
            var jso = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            return
            [
                // ── Validate leave request (MANDATORY BEFORE DRAFTING LEAVE) ───
                AIFunctionFactory.Create(
                    async ([Description("Start date of requested leave in YYYY-MM-DD format.")] string startDate,
                           [Description("End date of requested leave in YYYY-MM-DD format.")] string endDate,
                           [Description("Leave type ID (e.g. 1 for Casual Leave, 2 for Sick Leave).")] long? leaveTypeId,
                           [Description("Leave mode: 'Full Day', 'First Half', or 'Second Half'. Defaults to 'Full Day'.")] string? leaveMode) =>
                    {
                        try
                        {
                            if (!DateTime.TryParse(startDate, out var parsedStart) || !DateTime.TryParse(endDate, out var parsedEnd))
                            {
                                return JsonSerializer.Serialize(new { isValid = false, errorReason = "Invalid date format. Please provide dates in YYYY-MM-DD format." });
                            }

                            if (parsedEnd.Date < parsedStart.Date)
                            {
                                return JsonSerializer.Serialize(new { isValid = false, errorReason = "End date cannot be earlier than start date." });
                            }

                            // 1. Check if requested dates fall entirely on official holidays or weekends
                            var holidayDates = await leaveService.GetActiveHolidayDatesAsync();
                            var allHolidays = await holidayService.GetAllHolidays();
                            var matchedHolidays = allHolidays
                                .Where(h => h.IsActive != false && h.IsDeleted != true &&
                                            h.HolidayDate >= DateOnly.FromDateTime(parsedStart.Date) &&
                                            h.HolidayDate <= DateOnly.FromDateTime(parsedEnd.Date))
                                .Select(h => $"{h.HolidayName} ({h.HolidayDate:yyyy-MM-dd})")
                                .ToList();

                            var workingDays = leaveService.CalculateWorkingLeaveDays(parsedStart, parsedEnd, leaveMode, holidayDates);
                            if (workingDays <= 0)
                            {
                                var reason = matchedHolidays.Count > 0
                                    ? $"The requested date(s) fall on official public holiday(s): {string.Join(", ", matchedHolidays)}. Leave cannot be applied for official holidays."
                                    : "The requested date(s) fall entirely on non-working weekend days (Saturday/Sunday). Leave cannot be applied for non-working days.";
                                return JsonSerializer.Serialize(new { isValid = false, errorReason = reason });
                            }

                            // 2. Check for overlapping active (Approved or Pending) leaves for this employee
                            var allLeaves = await leaveService.GetAllLeaveRequestType();
                            var overlapping = allLeaves.FirstOrDefault(l =>
                                (string.Equals(l.Status, "Approved", StringComparison.OrdinalIgnoreCase) ||
                                 string.Equals(l.Status, "Pending", StringComparison.OrdinalIgnoreCase)) &&
                                !(parsedEnd.Date < l.StartDate.Date || parsedStart.Date > l.EndDate.Date));

                            if (overlapping != null)
                            {
                                var statusText = string.Equals(overlapping.Status, "Approved", StringComparison.OrdinalIgnoreCase) ? "an approved" : "a pending";
                                return JsonSerializer.Serialize(new
                                {
                                    isValid = false,
                                    errorReason = $"You already have {statusText} leave request (#{overlapping.Id}: {overlapping.LeaveTypeName} from {overlapping.StartDate:yyyy-MM-dd} to {overlapping.EndDate:yyyy-MM-dd}, Status: {overlapping.Status}) overlapping with the requested dates."
                                });
                            }

                            // 3. Check leave balance quota if leaveTypeId provided
                            if (leaveTypeId.HasValue && leaveTypeId.Value > 0)
                            {
                                var balances = await leaveService.GetEmployeeLeaveBalances();
                                var matchedBalance = balances.FirstOrDefault(b => b.LeaveTypeId == leaveTypeId.Value);
                                if (matchedBalance != null && matchedBalance.RemainingDays < workingDays)
                                {
                                    return JsonSerializer.Serialize(new
                                    {
                                        isValid = false,
                                        errorReason = $"Insufficient leave balance for {matchedBalance.LeaveTypeName}. You have {matchedBalance.RemainingDays} day(s) remaining, but this request requires {workingDays} day(s)."
                                    });
                                }
                            }

                            return JsonSerializer.Serialize(new
                            {
                                isValid = true,
                                workingDays = workingDays,
                                holidayNote = matchedHolidays.Count > 0 ? $"Excludes official holiday: {string.Join(", ", matchedHolidays)}" : null
                            });
                        }
                        catch (Exception ex)
                        {
                            return JsonSerializer.Serialize(new { isValid = false, errorReason = ex.Message });
                        }
                    },
                    name: "validate_leave_request",
                    description: "MANDATORY validation before drafting any leave. Validates if requested dates overlap with existing approved/pending leaves, fall on holidays or weekends, or exceed leave quota. Returns { isValid: bool, errorReason: string, workingDays: number }."
                ),

                // ── Validate regularization request (MANDATORY BEFORE DRAFTING REGULARIZATION) ───
                AIFunctionFactory.Create(
                    ([Description("The workday date to regularize in YYYY-MM-DD format.")] string requestedDate) =>
                    {
                        if (!DateTime.TryParse(requestedDate, out var date))
                        {
                            return JsonSerializer.Serialize(new { isValid = false, errorReason = "Invalid date format. Use YYYY-MM-DD." });
                        }

                        var today = DateTime.Today;
                        if (date.Date >= today)
                        {
                            string dateDesc = date.Date == today ? "today (current day)" : $"a future date ({date:yyyy-MM-dd})";
                            return JsonSerializer.Serialize(new
                            {
                                isValid = false,
                                errorReason = $"Attendance regularization cannot be requested for {dateDesc}. Regularization is strictly for past completed workdays prior to today ({today:yyyy-MM-dd})."
                            });
                        }

                        return JsonSerializer.Serialize(new { isValid = true });
                    },
                    name: "validate_regularization_request",
                    description: "MANDATORY validation before drafting attendance regularization. Checks that the requested date is strictly a past completed workday before today. Returns { isValid: bool, errorReason: string }."
                ),

                // ── Leave balance ──────────────────────────────────────────────
                AIFunctionFactory.Create(
                    async () =>
                    {
                        try
                        {
                            var balances = await leaveService.GetEmployeeLeaveBalances();
                            var result = balances.Select(b => new
                            {
                                leaveTypeId = b.LeaveTypeId,
                                leaveTypeName = b.LeaveTypeName,
                                totalQuota = b.TotalQuota,
                                usedDays = b.UsedDays,
                                pendingDays = b.PendingDays,
                                remainingDays = b.RemainingDays
                            });
                            var json = JsonSerializer.Serialize(result, jso);
                            toolTracker?.Add(("get_leave_balance", (json.Length + 3) / 4));
                            return json;
                        }
                        catch (Exception ex)
                        {
                            return $"{{\"error\":\"{ex.Message}\"}}";
                        }
                    },
                    name: "get_leave_balance",
                    description: "Returns the leave balance (total, used, pending, remaining) for every leave type for the currently logged-in employee. Also returns the leaveTypeId needed when drafting a leave application."),

                // ── My leave history ──────────────────────────────────────────
                AIFunctionFactory.Create(
                    async ([Description("Optional status filter: 'Pending', 'Approved', 'Rejected', or 'All'. Defaults to 'All'.")] string? status) =>
                    {
                        try
                        {
                            var leaves = await leaveService.GetAllLeaveRequestType();
                            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
                            {
                                leaves = leaves.Where(l => string.Equals(l.Status, status, StringComparison.OrdinalIgnoreCase));
                            }
                            var result = leaves
                                .OrderByDescending(l => l.StartDate)
                                .Take(20)
                                .Select(l => new
                                {
                                    id = l.Id,
                                    leaveTypeName = l.LeaveTypeName,
                                    startDate = l.StartDate.ToString("yyyy-MM-dd"),
                                    endDate = l.EndDate.ToString("yyyy-MM-dd"),
                                    leaveMode = l.LeaveMode,
                                    status = l.Status,
                                    totalDays = l.TotalDays
                                });
                            var json = JsonSerializer.Serialize(result, jso);
                            toolTracker?.Add(("get_my_leave_history", (json.Length + 3) / 4));
                            return json;
                        }
                        catch (Exception ex)
                        {
                            return $"{{\"error\":\"{ex.Message}\"}}";
                        }
                    },
                    name: "get_my_leave_history",
                    description: "Returns the personal leave application history for the currently logged-in employee. Can filter by status ('Pending', 'Approved', 'Rejected')."),

                // ── Pending Team Approvals (for Managers, HR, and Admin) ───────
                AIFunctionFactory.Create(
                    async ([Description("Category of requests to check: 'Leave', 'Attendance', or 'All'. Defaults to 'All'.")] string? category,
                           [Description("Filter by status: 'Pending', 'Approved', 'Rejected', or 'All'. Defaults to 'Pending'.")] string? status) =>
                    {
                        try
                        {
                            bool isPrivileged = userContext.IsAdmin() || userContext.IsHR() || userContext.IsManager();
                            if (!isPrivileged)
                            {
                                return "{\"info\":\"You do not have a Manager, HR, or Admin role to view team approvals.\"}";
                            }

                            // Strict filter: default to Pending, strictly return requests matching the target status
                            var targetStatus = string.IsNullOrWhiteSpace(status) ? "Pending" : status.Trim();
                            var targetCategory = string.IsNullOrWhiteSpace(category) ? "All" : category.Trim();
                            var pendingList = new List<object>();

                            // 1. Team Leaves
                            if (string.Equals(targetCategory, "All", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(targetCategory, "Leave", StringComparison.OrdinalIgnoreCase))
                            {
                                var teamLeaves = await leaveService.GetAllEmployeesLeaveRequest();
                                if (!string.Equals(targetStatus, "All", StringComparison.OrdinalIgnoreCase))
                                {
                                    teamLeaves = teamLeaves.Where(l => string.Equals(l.Status, targetStatus, StringComparison.OrdinalIgnoreCase));
                                }

                                foreach (var l in teamLeaves.Take(15))
                                {
                                    pendingList.Add(new
                                    {
                                        category = "Leave",
                                        id = l.Id,
                                        employeeName = $"{l.Employee?.FirstName} {l.Employee?.LastName}".Trim(),
                                        leaveType = l.LeaveType?.Type ?? "Leave",
                                        startDate = l.StartDate.ToString("yyyy-MM-dd"),
                                        endDate = l.EndDate.ToString("yyyy-MM-dd"),
                                        totalDays = l.TotalDays,
                                        status = l.Status
                                    });
                                }
                            }

                            // 2. Attendance Regularization
                            if (string.Equals(targetCategory, "All", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(targetCategory, "Attendance", StringComparison.OrdinalIgnoreCase))
                            {
                                var teamAttendance = await attendanceService.GetAllAsync();
                                if (!string.Equals(targetStatus, "All", StringComparison.OrdinalIgnoreCase))
                                {
                                    teamAttendance = teamAttendance.Where(a => string.Equals(a.Status, targetStatus, StringComparison.OrdinalIgnoreCase));
                                }

                                foreach (var a in teamAttendance.Take(15))
                                {
                                    pendingList.Add(new
                                    {
                                        category = "Attendance",
                                        id = a.Id,
                                        employeeName = a.EmployeeName ?? "Employee",
                                        requestType = a.RequestType,
                                        date = a.RequestedDate.ToString("yyyy-MM-dd"),
                                        status = a.Status
                                    });
                                }
                            }

                            var json = JsonSerializer.Serialize(pendingList, jso);
                            toolTracker?.Add(("get_team_approval_requests", (json.Length + 3) / 4));
                            return json;
                        }
                        catch (Exception ex)
                        {
                            return $"{{\"error\":\"{ex.Message}\"}}";
                        }
                    },
                    name: "get_team_approval_requests",
                    description: "For Managers, HR, and Admin users only: returns team requests (leaves, attendance regularizations) pending approval or filtered by status ('Pending', 'Approved', 'Rejected', 'All')."),

                // ── Upcoming holidays ─────────────────────────────────────────
                AIFunctionFactory.Create(
                    async () =>
                    {
                        try
                        {
                            var holidays = (await holidayService.GetAllHolidays())
                                .Where(h => h.IsActive != false && h.IsDeleted != true)
                                .OrderBy(h => h.HolidayDate)
                                .Select(h => new
                                {
                                    name = h.HolidayName,
                                    date = h.HolidayDate.ToString("yyyy-MM-dd"),
                                    day = h.HolidayDate.DayOfWeek.ToString()
                                });
                            var json = JsonSerializer.Serialize(holidays, jso);
                            toolTracker?.Add(("get_upcoming_holidays", (json.Length + 3) / 4));
                            return json;
                        }
                        catch (Exception ex)
                        {
                            return $"{{\"error\":\"{ex.Message}\"}}";
                        }
                    },
                    name: "get_upcoming_holidays",
                    description: "Returns the complete list of official public holidays for the company calendar."),

                // ── Latest payslip ────────────────────────────────────────────
                AIFunctionFactory.Create(
                    async () =>
                    {
                        try
                        {
                            var empId = userContext.GetCurrentEmployeeId();
                            var slips = await payrollService.GetEmployeePayslipsAsync(empId);
                            var latest = slips.OrderByDescending(p => p.Year)
                                              .ThenByDescending(p => p.Month)
                                              .FirstOrDefault();
                            if (latest is null) return "{\"info\":\"No payslip found.\"}";

                            var result = new
                            {
                                month = latest.Month,
                                year = latest.Year,
                                basicSalary = latest.BasicSalary,
                                hra = latest.Hra,
                                grossSalary = latest.GrossSalary,
                                epfDeduction = latest.EpfDeduction,
                                esicDeduction = latest.EsicDeduction,
                                tdsDeduction = latest.TdsDeduction,
                                netSalary = latest.NetSalary
                            };
                            var json = JsonSerializer.Serialize(result, jso);
                            toolTracker?.Add(("get_latest_payslip", (json.Length + 3) / 4));
                            return json;
                        }
                        catch (Exception ex)
                        {
                            return $"{{\"error\":\"{ex.Message}\"}}";
                        }
                    },
                    name: "get_latest_payslip",
                    description: "Returns the most recent payslip details for the currently logged-in employee, including gross, deductions, and net salary."),

                // ── CTC / salary breakdown ─────────────────────────────────────
                AIFunctionFactory.Create(
                    async () =>
                    {
                        try
                        {
                            var empId = userContext.GetCurrentEmployeeId();
                            var assignment = await payrollService.GetEmployeeSalaryAssignmentAsync(empId);
                            if (assignment is null) return "{\"info\":\"No salary assignment found.\"}";

                            var result = new
                            {
                                annualCtc = assignment.AnnualCtc,
                                monthlyGross = assignment.MonthlyGross,
                                basicSalary = assignment.BasicSalary,
                                hra = assignment.Hra,
                                epfEmployee = assignment.EpfEmployee,
                                esicEmployee = assignment.EsicEmployee,
                                professionalTax = assignment.ProfessionalTax,
                                monthlyTds = assignment.MonthlyTds,
                                monthlyNetTakeHome = assignment.MonthlyNetTakeHome
                            };
                            var json = JsonSerializer.Serialize(result, jso);
                            toolTracker?.Add(("get_salary_breakdown", (json.Length + 3) / 4));
                            return json;
                        }
                        catch (Exception ex)
                        {
                            return $"{{\"error\":\"{ex.Message}\"}}";
                        }
                    },
                    name: "get_salary_breakdown",
                    description: "Returns the CTC and monthly salary breakdown for the currently logged-in employee, including components like Basic, HRA, EPF, ESIC, TDS, and net take-home."),

                // ── Policy search ─────────────────────────────────────────────
                AIFunctionFactory.Create(
                    async ([Description("Keywords to search HR policies for (e.g. 'leave encashment', 'notice period', 'overtime')")]
                           string query) =>
                    {
                        try
                        {
                            var results = await policyService.SearchAsync(query);
                            var list = results.Select(p => new
                            {
                                title = p.Title,
                                category = p.Category,
                                content = p.Content
                            }).ToList();
                            var json = list.Count == 0
                                ? "{\"info\":\"No matching policy found. The policy may not be documented yet.\"}"
                                : JsonSerializer.Serialize(list, jso);
                            toolTracker?.Add(("search_company_policy", (json.Length + 3) / 4));
                            return json;
                        }
                        catch (Exception ex)
                        {
                            return $"{{\"error\":\"{ex.Message}\"}}";
                        }
                    },
                    name: "search_company_policy",
                    description: "Searches company HR policies by keyword. Use this for any question about leave entitlements, notice period, payroll rules, attendance policy, WFH, etc."),
            ];
        }

        // ──────────────────────────────────────────────────────────────────────
        // Parse <hr-action>...</hr-action> from the AI's raw response.
        // Returns (cleanReply, draft?).
        // ──────────────────────────────────────────────────────────────────────
        private static (string cleanReply, ChatDraft? draft) ExtractDraft(string rawReply)
        {
            var match = Regex.Match(rawReply,
                @"<hr-action>\s*([\s\S]*?)\s*</hr-action>",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return (rawReply.Trim(), null);

            var jsonBlock = match.Groups[1].Value.Trim();
            var cleanReply = rawReply
                .Replace(match.Value, string.Empty)
                .Trim();

            try
            {
                using var doc = JsonDocument.Parse(jsonBlock);
                var root = doc.RootElement;

                var draft = new ChatDraft
                {
                    ActionType = root.TryGetProperty("actionType", out var at) ? at.GetString() ?? "" : "",
                    Summary = root.TryGetProperty("summary", out var s) ? s.GetString() ?? "" : "",
                    ConfirmEndpoint = root.TryGetProperty("confirmEndpoint", out var ep) ? ep.GetString() ?? "" : "",
                    Payload = root.TryGetProperty("payload", out var p)
                                      ? JsonSerializer.Deserialize<Dictionary<string, object>>(p.GetRawText())!
                                      : new Dictionary<string, object>()
                };

                return (cleanReply, draft);
            }
            catch
            {
                // Malformed JSON — return raw so the user sees the full reply
                return (rawReply.Trim(), null);
            }
        }
    }
}
