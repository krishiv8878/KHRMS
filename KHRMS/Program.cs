
using KHRMS.Services;
using KHRMS.Services.Workflow.Interfaces;
using KHRMS.Services.Workflow.Services;
using KHRMS.Services.Workflow.Rules;
using KHRMS.Services.Workflow.Conditions;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using KHRMS.AI;
using Serilog;
using GlobalExceptionHandlingDemo.Middleware;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.AI;
using OpenAI;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

//Configure Serilog for multiple levels
Log.Logger = new LoggerConfiguration()
    .WriteTo.File($"Logs/ERROR/ERROR_{DateTime.Now:yyyy_MM_dd}.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
    .WriteTo.File($"Logs/WARNING/WARNING_{DateTime.Now:yyyy_MM_dd}.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
    .WriteTo.File($"Logs/INFO/INFO_{DateTime.Now:yyyy_MM_dd}.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .WriteTo.File($"Logs/DEBUG/DEBUG_{DateTime.Now:yyyy_MM_dd}.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
    .CreateLogger();
builder.Host.UseSerilog();

// Register services
builder.Services.AddDIServices(builder.Configuration);

builder.Services.AddScoped<ICandidateService, CandidateService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IDesignationService, DesignationService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAssetsMasterService, AssetsMasterService>();
builder.Services.AddScoped<IAssetRequestService, AssetRequestService>();
builder.Services.AddScoped<IHolidayService, HolidayService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILeaveTypeService, LeaveTypeService>();
builder.Services.AddScoped<IUserLoginService, UserLoginService>();
builder.Services.AddScoped<IProjectMasterService, ProjectMasterService>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<IRoleMasterService, RoleMasterService>();
builder.Services.AddScoped<IEmployeeRoleMappingService, EmployeeRoleMappingService>();
builder.Services.AddScoped<IAttendanceRequestService, AttendanceRequestService>();
builder.Services.AddScoped<IEmployeeAttendanceService, EmployeeAttendanceService>();
builder.Services.AddScoped<IEmployeePaymentInfoService, EmployeePaymentInfoService>();
builder.Services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IEmailTemplateTypeMasterService, EmailTemplateTypeService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISendEmailService, SendEmailService>();
builder.Services.AddScoped<ILeaveRequestTypeService, LeaveRequestTypeService>();
builder.Services.AddScoped<IResignationService, ResignationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAttendanceLogService, AttendanceLogService>();
builder.Services.AddScoped<ITimesheetService, TimesheetService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailTriggerEventService, EmailTriggerEventService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IStatutoryDeductionEngine, StatutoryDeductionEngine>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IPmsService, PmsService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IRecruitmentService, RecruitmentService>();
builder.Services.AddScoped<IEmployeeDailyWorkModeService, EmployeeDailyWorkModeService>();
builder.Services.AddScoped<IChecklistService, ChecklistService>();
builder.Services.AddScoped<IAttendanceLockService, AttendanceLockService>();
builder.Services.AddScoped<IDailyAttendanceService, DailyAttendanceService>();
builder.Services.AddScoped<ILeaveLedgerService, LeaveLedgerService>();
builder.Services.AddScoped<ILopConversionService, LopConversionService>();
builder.Services.AddScoped<ILeaveEncashmentService, LeaveEncashmentService>();
builder.Services.AddScoped<ICompOffService, CompOffService>();
// Phase 7 Services: Helpdesk, Company Policies, and Reports
builder.Services.AddScoped<IHelpdeskService, HelpdeskService>();
builder.Services.AddScoped<ICompanyPolicyService, CompanyPolicyService>();
builder.Services.AddScoped<IReportsService, ReportsService>();

// Configurable Approval Workflow System
builder.Services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
builder.Services.AddScoped<IApproverRule, ReportingManagerRule>();
builder.Services.AddScoped<IApproverRule, SkipLevelManagerRule>();
builder.Services.AddScoped<IApproverRule, DepartmentHeadRule>();
builder.Services.AddScoped<IApproverRule, RoleInLocationRule>();
builder.Services.AddScoped<IApproverRule, ProjectOwnerRule>();
builder.Services.AddScoped<IApproverRule, SpecificPersonRule>();
builder.Services.AddScoped<IApproverRule, GroupQueueRule>();
builder.Services.AddScoped<IApproverResolver, ApproverResolver>();
builder.Services.AddScoped<IWorkflowEngineService, WorkflowEngineService>();
builder.Services.AddScoped<IWorkflowDelegationService, WorkflowDelegationService>();
builder.Services.AddScoped<IWorkflowAdminService, WorkflowAdminService>();
builder.Services.AddHostedService<WorkflowEscalationBackgroundService>();
builder.Services.AddHostedService<LwdReminderBackgroundService>();
builder.Services.AddHostedService<DailyAttendanceBackgroundService>();
builder.Services.AddHostedService<LeaveAccrualBackgroundService>();
builder.Services.AddHostedService<LeaveRolloverBackgroundService>();
builder.Services.AddHostedService<CompOffExpiryBackgroundService>();
builder.Services.AddMemoryCache();

// SEC-04: Fixed-window Rate Limiting policies for Auth and Chatbot endpoints
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Policy for login / registration endpoints (10 requests per minute per IP)
    options.AddPolicy("auth-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Policy for AI Chatbot endpoint (20 requests per minute per user/IP)
    options.AddPolicy("chat-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// Add context-aware services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IEmployeeAccessService, EmployeeAccessService>();

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

//  Add CORS
builder.Services.AddCors(p => p.AddPolicy("corspolice", corsBuilder =>
{
    corsBuilder.WithOrigins("http://localhost:4200")
    .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));
// Fail-fast configuration validation: ensure JWT secret is cryptographically secure (at least 32 bytes / 256 bits)
var configuredJwtSecret = builder.Configuration["Jwt:PasswordResetSecret"];
if (string.IsNullOrWhiteSpace(configuredJwtSecret) || Encoding.UTF8.GetByteCount(configuredJwtSecret) < 32)
{
    throw new InvalidOperationException(
        "CRITICAL SECURITY CONFIGURATION ERROR: 'Jwt:PasswordResetSecret' must be configured with a key of at least 256 bits (32 bytes). " +
        "In local development, configure this via .NET User Secrets (dotnet user-secrets set 'Jwt:PasswordResetSecret' '<secret>'); " +
        "in production, supply it via environment variables (Jwt__PasswordResetSecret) or Key Vault. The application cannot start with a missing or blank secret.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option => option.TokenValidationParameters = new TokenValidationParameters { 
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
        ValidAudience = builder.Configuration["Jwt:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuredJwtSecret)),
    });
builder.Services.AddAuthorization(options =>
{
    // Default-Deny standard: Fallback policy mandates authentication unless explicitly decorated with [AllowAnonymous]
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, 
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your JWT Access Token",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }

    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});


// API key lives in user-secrets ("Gemini:ApiKey") never in appsettings.json.
// Model name and endpoint are read from appsettings.json ("Gemini" section).
var geminiApiKey = builder.Configuration["Gemini:ApiKey"]
    ?? "AI_KEY_NOT_CONFIGURED";

var geminiEndpoint = new Uri(
    builder.Configuration["Gemini:Endpoint"]
    ?? "https://generativelanguage.googleapis.com/v1beta/openai/");

var geminiModel = builder.Configuration["Gemini:Model"]
    ?? builder.Configuration["Gemini:ModelName"]
    ?? "gemini-3.1-flash-lite";

var openAiOptions = new OpenAIClientOptions { Endpoint = geminiEndpoint };
openAiOptions.AddPolicy(new GeminiThoughtSignaturePolicy(), System.ClientModel.Primitives.PipelinePosition.PerCall);

var openAiClient = new OpenAIClient(
    new System.ClientModel.ApiKeyCredential(geminiApiKey),
    openAiOptions);

// Register with UseFunctionInvocation so tools are called automatically.
builder.Services.AddChatClient(
    openAiClient.GetChatClient(geminiModel).AsIChatClient())
    .UseFunctionInvocation();

// INT-04: Database-backed SQL company policy service for Gemini AI Chatbot
builder.Services.AddScoped<IChatPolicyService, SqlChatPolicyService>();

var app = builder.Build();

//Middleware configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

//  Global exception middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Serilog request logging
app.UseSerilogRequestLogging();

// SEC-26: Standard HTTP Security Headers Middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    if (!context.Response.Headers.ContainsKey("Strict-Transport-Security") && context.Request.IsHttps)
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    }
    await next();
});

// Middleware ordering
app.UseRouting();
app.UseCors("corspolice");
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

// Reconcile default permissions and system roles on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        await permissionService.SeedDefaultPermissionsAsync();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to run startup permission reconciliation.");
    }
}

app.Run();
