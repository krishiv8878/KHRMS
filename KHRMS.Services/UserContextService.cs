using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User;
    public long GetCurrentEmployeeId()
    {
        var id = User?.FindFirst("UserId")?.Value;
        return id != null ? Convert.ToInt64(id) : 0;
    }

}
