using System.ComponentModel;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using KHRMS.AI;
using KHRMS.Authorization;
using KHRMS.Core;
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
        IChatPolicyService policyService) : ControllerBase
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

            // 1. Build conversation history with system prompt prepended
            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, HrSystemPrompt.Text)
            };

            // Keep only recent messages (sliding window of 8) to reduce token usage and prevent 429 TPM exhaustion
            var recentMessages = request.Messages.Count > 8
                ? request.Messages.Skip(request.Messages.Count - 8)
                : request.Messages;

            foreach (var msg in recentMessages)
            {
                var role = msg.Role.Equals("user", StringComparison.OrdinalIgnoreCase)
                    ? ChatRole.User
                    : ChatRole.Assistant;
                messages.Add(new ChatMessage(role, msg.Content ?? string.Empty));
            }

            // 2. Register tools (closures capture scoped services from DI, employee ID from JWT)
            var tools = BuildTools();
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

                    return Ok(new KHRMS.AI.ChatResponse
                    {
                        Reply = cleanReply,
                        Draft = draft
                    });
                }
                catch (System.ClientModel.ClientResultException cre) when (cre.Status == 429)
                {
                    Serilog.Log.Warning("Gemini rate limit (429) encountered on attempt {Attempt}/{Max}", attempt, maxAttempts);
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
        private List<AITool> BuildTools()
        {
            var jso = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            return
            [
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
                            return JsonSerializer.Serialize(result, jso);
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
                            var result = leaves.Select(l => new
                            {
                                id = l.Id,
                                leaveTypeName = l.LeaveTypeName,
                                startDate = l.StartDate.ToString("yyyy-MM-dd"),
                                endDate = l.EndDate.ToString("yyyy-MM-dd"),
                                leaveMode = l.LeaveMode,
                                status = l.Status,
                                leaveReason = l.LeaveReason,
                                totalDays = l.TotalDays
                            });
                            return JsonSerializer.Serialize(result, jso);
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

                                foreach (var l in teamLeaves)
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
                                        leaveMode = l.LeaveMode,
                                        status = l.Status,
                                        reason = l.LeaveReason
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

                                foreach (var a in teamAttendance)
                                {
                                    pendingList.Add(new
                                    {
                                        category = "Attendance",
                                        id = a.Id,
                                        employeeName = a.EmployeeName ?? "Employee",
                                        requestType = a.RequestType,
                                        date = a.RequestedDate.ToString("yyyy-MM-dd"),
                                        status = a.Status,
                                        reason = a.Reason
                                    });
                                }
                            }

                            return JsonSerializer.Serialize(pendingList, jso);
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
                            var today = DateOnly.FromDateTime(DateTime.Today);
                            var holidays = await holidayService.GetAllHolidays();
                            var upcoming = holidays
                                .Where(h => h.HolidayDate >= today)
                                .OrderBy(h => h.HolidayDate)
                                .Take(20)
                                .Select(h => new
                                {
                                    name = h.HolidayName,
                                    date = h.HolidayDate.ToString("yyyy-MM-dd"),
                                    day = h.HolidayDate.DayOfWeek.ToString()
                                });
                            return JsonSerializer.Serialize(upcoming, jso);
                        }
                        catch (Exception ex)
                        {
                            return $"{{\"error\":\"{ex.Message}\"}}";
                        }
                    },
                    name: "get_upcoming_holidays",
                    description: "Returns the list of upcoming public holidays for the current year."),

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
                            return JsonSerializer.Serialize(result, jso);
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
                            return JsonSerializer.Serialize(result, jso);
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
                            return list.Count == 0
                                ? "{\"info\":\"No matching policy found. The policy may not be documented yet.\"}"
                                : JsonSerializer.Serialize(list, jso);
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
