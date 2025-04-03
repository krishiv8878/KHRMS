
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Serilog;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
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
            Log.Error(ex, "Unhandled exception occurred. ex{0}: {Message}, ex{1}: {StackTrace}, ex{2}: {InnerException}",
                ex.Message, ex.StackTrace, ex.InnerException?.Message ?? "N/A");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "An error occurred while processing your request.",
                Data = new
                {
                    ex0 = ex.Message,
                    ex1 = ex.StackTrace,
                    ex2 = ex.InnerException?.Message ?? "N/A"
                }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
