using Microsoft.AspNetCore.Diagnostics;

namespace QuizyfyAPI.Handlers;

internal sealed partial class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }
    
    [LoggerMessage(
        Level = LogLevel.Error, 
        Message = "Unhandled exception occurred: {Message}")]
    private static partial void LogUnhandledException(ILogger logger, Exception ex, string message);

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogUnhandledException(_logger, exception, exception.Message);
        
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred. Please try again later."
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(
            problemDetails, 
            AppJsonSerializerContext.Default.ProblemDetails,
            "application/problem+json",
            cancellationToken);
        
        return true; 
    }
}