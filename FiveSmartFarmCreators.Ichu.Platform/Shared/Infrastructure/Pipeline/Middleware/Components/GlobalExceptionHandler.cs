using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Turns unhandled exceptions into RFC 9457 problem details responses without exposing internal details.
/// </summary>
/// <param name="logger">Logger for diagnostics and error reporting.</param>
/// <param name="problemDetailsService">The service that writes problem details responses.</param>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    /// <summary>
    ///     Logs the exception and writes the matching problem details response.
    /// </summary>
    /// <param name="httpContext">The context of the failed request.</param>
    /// <param name="exception">The unhandled exception.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>True when the response was written.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int statusCode;
        string title;
        if (exception is OperationCanceledException)
        {
            logger.LogWarning(exception, "The request was cancelled.");
            statusCode = StatusCodes.Status409Conflict;
            title = "The operation was cancelled.";
        }
        else
        {
            logger.LogError(exception, "An unhandled exception occurred.");
            statusCode = StatusCodes.Status500InternalServerError;
            title = "An unexpected error occurred.";
        }

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Instance = httpContext.Request.Path
            }
        });
    }
}
