using System.Text.Json;
using FluentValidation;
using RepLeague.Application.Common.Exceptions;

namespace RepLeague.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            ValidationException vex => (StatusCodes.Status400BadRequest,
                vex.Errors.Select(e => e.ErrorMessage)),
            ConflictException => (StatusCodes.Status409Conflict,
                new[] { exception.Message }),
            NotFoundException => (StatusCodes.Status404NotFound,
                new[] { exception.Message }),
            UnauthorizedException => (StatusCodes.Status401Unauthorized,
                new[] { exception.Message }),
            AppException => (StatusCodes.Status400BadRequest,
                new[] { exception.Message }),
            _ => (StatusCodes.Status500InternalServerError,
                new[] { "An unexpected error occurred." })
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = JsonSerializer.Serialize(new { errors = message });
        return context.Response.WriteAsync(response);
    }
}
