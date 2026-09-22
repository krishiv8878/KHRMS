using KHRMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace KHRMS.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncActionFilter
    {
        public string Permission { get; }

        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userContext = context.HttpContext.RequestServices.GetRequiredService<IUserContextService>();

            // Super Admin bypasses all checks
            if (userContext.IsAdmin())
            {
                await next();
                return;
            }

            var hasPermission = await userContext.HasPermissionAsync(Permission);
            if (!hasPermission)
            {
                context.Result = new ObjectResult(new
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                    Message = $"Access Denied: Missing required permission '{Permission}'.",
                    Data = (object?)null
                })
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
                return;
            }

            await next();
        }
    }
}
