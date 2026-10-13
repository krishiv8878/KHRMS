using KHRMS.Infrastructure;
using Serilog;
using System.Net;
using System.Text.Json;

namespace GlobalExceptionHandlingDemo.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var contextInfo = GetContextInfo(context);
                await HandleExceptionAsync(context, ex, contextInfo);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, string contextInfo)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;
            var routeData = context.GetRouteData();
            string controller = routeData.Values["controller"]?.ToString() ?? "UnknownController";
            string methodName = routeData.Values["action"]?.ToString() ?? "UnknownMethod";

            var apiResponse = new ApiResponse<object>
            {
                Data = null
            };

            switch (exception)
            {
                case ApplicationException ex:
                    apiResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    apiResponse.Message = !string.IsNullOrWhiteSpace(ex.Message) ? ex.Message : "Application Exception Occurred, please retry after some time.";
                    Log.Error(exception, "Application Exception in {Method} of {Controller}: {Message}", methodName, controller, exception.Message);
                    break;

                case FileNotFoundException ex:
                    apiResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    apiResponse.Message = "The requested file or resource is not found.";
                    Log.Error(exception, "Resource not found in {Method} of {Controller}: {Message}", methodName, controller, exception.Message);
                    break;

                case UnauthorizedAccessException ex:
                    apiResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    apiResponse.Message = "Unauthorized request.";
                    Log.Warning("Unauthorized access attempt in {Method} of {Controller}: {Message}", methodName, controller, exception.Message);
                    break;

                default:
                    apiResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    apiResponse.Message = "Internal Server Error. Please contact support or retry after some time.";
                    Log.Error(exception, "Unhandled Exception in {Method} of {Controller}: {Message}", methodName, controller, exception.Message);
                    break;
            }

            var exResult = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await context.Response.WriteAsync(exResult);
        }

        private string GetContextInfo(HttpContext context)
        {
            try
            {
                var routeData = context.GetRouteData();
                string controller = routeData.Values["controller"]?.ToString() ?? "UnknownController";
                string method = context.Request.Method;

                return $"[Controller: {controller}] [Method: {method}] ";
            }
            catch
            {
                return "[ContextInfo: Unknown]";
            }
        }
    }
}

