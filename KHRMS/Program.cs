using KHRMS.Services;
using KHRMS.Infrastructure;
using KHRMS.Services.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// Configure Serilog to log only INFO messages
Log.Logger = new LoggerConfiguration()
    .WriteTo.File($"Logs/ERROR/ERROR_{DateTime.Now:yyyy_MM_dd}.log",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
    .WriteTo.File($"Logs/WARNING/WARNING_{DateTime.Now:yyyy_MM_dd}.log",
       restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
     .WriteTo.File($"Logs/INFO/INFO_{DateTime.Now:yyyy_MM_dd}.log",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
     .WriteTo.File($"Logs/DEBUG/DEBUG_{DateTime.Now:yyyy_MM_dd}.log",
       restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
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

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(p => p.AddPolicy("corspolice", builder =>
{
    builder.AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader();
}));

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
// Use the Global Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging(); // Log all requests

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("corspolice");

app.Run();
