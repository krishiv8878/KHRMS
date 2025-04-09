using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long GetCurrentEmployeeId()
    {
        var employeeIdString = _httpContextAccessor.HttpContext?.Session.GetString("EmployeeId");

        if (long.TryParse(employeeIdString, out var employeeId))
            return employeeId;

        throw new Exception("User is not logged in.");
    }

}
