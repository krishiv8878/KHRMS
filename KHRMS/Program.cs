
using KHRMS.Services;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Serilog;
using GlobalExceptionHandlingDemo.Middleware;

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
builder.Services.AddScoped<IAssetsMasterService, AssetsMasterService>();
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

// Add context-aware services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IUserContextService, UserContextService>();

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; 
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

//  Add CORS
builder.Services.AddCors(p => p.AddPolicy("corspolice", corsBuilder =>
{
    corsBuilder.WithOrigins("http://localhost:4200")
    .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));

//Add controllers, Swagger, etc.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Middleware ordering
app.UseRouting();
app.UseCors("corspolice");
app.UseSession();
app.UseAuthorization();

app.MapControllers();

app.Run();
