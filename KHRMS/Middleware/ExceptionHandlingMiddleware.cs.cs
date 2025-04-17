
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
            ResponseModel exModel = new ResponseModel();
            var routeData = context.GetRouteData();
            string controller = routeData.Values["controller"]?.ToString() ?? "UnknownController";
            string methodName = routeData.Values["action"]?.ToString() ?? "UnknownMethod";
            switch (exception)
            {
                case ApplicationException ex:
                    exModel.responseCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    exModel.responseMessage = "Application Exception Occurred, please retry after some time.";
                    Log.Error(string.Format("Application Exception Occurred while processing the request from method {0} of {1}Controller with error {2}", methodName, controller, exception.Message));
                    break;

                case FileNotFoundException ex:
                    exModel.responseCode = (int)HttpStatusCode.NotFound;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    exModel.responseMessage = "The requested resource is not found.";
                    Log.Error(string.Format("The requested resource is not found, while processing the request from method {0} of {1}Controller with error {2}", methodName, controller, exception.Message));
                    break;

                default:
                    exModel.responseCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    exModel.responseMessage = "Internal Server Error, Please retry after some time.";
                    Log.Error(string.Format("An exception occured while processing the request from method {0} of {1}Controller with error {2}", methodName, controller, exception.Message));
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
                string method = context.Request.Method;

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

