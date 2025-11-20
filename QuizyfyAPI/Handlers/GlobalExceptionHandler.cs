using Microsoft.AspNetCore.Diagnostics;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        string message = _hostEnvironment.IsDevelopment() ? $"Internal Server Error: {exception.Message} \n {exception.StackTrace}" : "An internal error occurred. Please try again later.";

        ErrorResponse response = new()
        {
            StatusCode = httpContext.Response.StatusCode,
            Message = message
        };
        
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        
        return true; 
    }
}