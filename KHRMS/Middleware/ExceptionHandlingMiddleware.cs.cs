
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
                Log.Error(ex, "{ContextInfo} - An exception occurred while processing the request.", contextInfo);
                await HandleExceptionAsync(context, ex, contextInfo);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, string contextInfo)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;
            ResponseModel exModel = new ResponseModel();

            switch (exception)
            {
                case ApplicationException ex:
                    exModel.responseCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    exModel.responseMessage = "Application Exception Occurred, please retry after some time.";
                    Log.Error("{ContextInfo} - ApplicationException: {Message}", contextInfo, ex.Message);
                    break;

                case FileNotFoundException ex:
                    exModel.responseCode = (int)HttpStatusCode.NotFound;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    exModel.responseMessage = "The requested resource is not found.";
                    Log.Error("{ContextInfo} - FileNotFoundException: {Message}", contextInfo, ex.Message);
                    break;

                default:
                    exModel.responseCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    exModel.responseMessage = "Internal Server Error, Please retry after some time.";
                    Log.Error("{ContextInfo} - Unhandled Exception: {Message}", contextInfo, exception.Message);
                    break;
            }

            var exResult = JsonSerializer.Serialize(exModel);
            await context.Response.WriteAsync(exResult);
        }

        private string GetContextInfo(HttpContext context)
        {
            try
            {
               var routeData = context.GetRouteData();
                string controller = routeData.Values["controller"]?.ToString() ?? "UnknownController";
               // string action = routeData.Values["action"]?.ToString() ?? "UnknownAction";
               // string requestPath = context.Request.Path;
                string method = context.Request.Method;
                //string user = context.User?.Identity?.Name ?? "Anonymous";

                return $"[Controller: {controller}] [Method: {method}] ";
            }
            catch
            {
                return "[ContextInfo: Unknown]";
            }
        }
    }

    public class ResponseModel
    {
        public int responseCode { get; set; }
        public string responseMessage { get; set; }
    }
}

