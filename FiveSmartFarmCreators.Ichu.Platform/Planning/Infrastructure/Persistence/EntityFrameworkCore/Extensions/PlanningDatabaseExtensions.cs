namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Extensions;

/// <summary>
/// Initializes the Planning database schema for a newly created database.
/// </summary>
public static class PlanningDatabaseExtensions
{
    /// <summary>
    /// Creates the Planning schema when the configured database is empty.
    /// </summary>
    /// <param name="services">The application service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task InitializePlanningDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlanningDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }
}
