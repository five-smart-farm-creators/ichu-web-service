using System.Globalization;
using FiveSmartFarmCreators.Ichu.Platform.Resources.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Shared.Infrastructure.Pipeline;

public class GlobalExceptionHandlerTests
{
    private const string UniqueViolation = "23505";
    private const string ForeignKeyViolation = "23503";

    private static IStringLocalizer<ErrorMessages> Localizer { get; } = new StringLocalizer<ErrorMessages>(
        new ResourceManagerStringLocalizerFactory(Options.Create(new LocalizationOptions()), NullLoggerFactory.Instance));

    [Fact]
    public async Task TryHandleAsync_MapsUniqueViolationToConflictWithTheLocalizedMessage()
    {
        var (httpContext, written) = await HandleInEnglishAsync(DatabaseError(UniqueViolation));

        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, written.ProblemDetails.Status);
        Assert.Equal("The resource changed concurrently; retry the request.", written.ProblemDetails.Title);
    }

    [Fact]
    public async Task TryHandleAsync_MapsConcurrencyConflictsToConflict()
    {
        var (httpContext, written) = await HandleInEnglishAsync(new DbUpdateConcurrencyException("The row changed."));

        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
        Assert.Equal("The resource changed concurrently; retry the request.", written.ProblemDetails.Title);
    }

    [Fact]
    public async Task TryHandleAsync_KeepsOtherDatabaseErrorsAsInternalErrors()
    {
        var (httpContext, _) = await HandleInEnglishAsync(DatabaseError(ForeignKeyViolation));

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_KeepsNonDatabaseExceptionsAsInternalErrors()
    {
        var (httpContext, _) = await HandleInEnglishAsync(new InvalidOperationException("The state is invalid."));

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    private static DbUpdateException DatabaseError(string sqlState) =>
        new("The changes could not be saved.", new PostgresException("A database error occurred.", "ERROR", "ERROR", sqlState));

    private static async Task<(DefaultHttpContext HttpContext, ProblemDetailsContext Written)> HandleInEnglishAsync(Exception exception)
    {
        var previousCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        try
        {
            var problemDetails = new CapturingProblemDetailsService();
            var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, Localizer, problemDetails);
            var httpContext = new DefaultHttpContext();

            await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
            return (httpContext, problemDetails.Written!);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? Written { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Written = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Written = context;
            return ValueTask.FromResult(true);
        }
    }
}
