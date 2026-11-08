using FiveSmartFarmCreators.Ichu.Platform.Resources.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Turns unhandled exceptions into RFC 9457 problem details responses without exposing internal details.
/// </summary>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IStringLocalizer<ErrorMessages> localizer,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    private const string UniqueViolationSqlState = "23505";

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
        else if (IsConcurrentModification(exception))
        {
            // A concurrent request stored the same resource first, so a retry reads the stored state.
            logger.LogWarning(exception, "A concurrent request changed the same resource first.");
            statusCode = StatusCodes.Status409Conflict;
            title = localizer["ConcurrentModification"].Value;
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

    private static bool IsConcurrentModification(Exception exception) =>
        exception is DbUpdateConcurrencyException
        || exception is DbUpdateException { InnerException: PostgresException { SqlState: UniqueViolationSqlState } };
}
