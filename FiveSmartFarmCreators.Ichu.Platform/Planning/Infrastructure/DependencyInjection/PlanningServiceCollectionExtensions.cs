using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.DependencyInjection;

/// <summary>
/// Registers Planning application services.
/// </summary>
public static class PlanningServiceCollectionExtensions
{
    /// <summary>
    /// Adds Planning application services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The PostgreSQL connection string for Planning.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="ArgumentException">The connection string is null or whitespace.</exception>
    public static IServiceCollection AddPlanning(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A Planning PostgreSQL connection string is required.", nameof(connectionString));
        }

        services.AddDbContext<PlanningDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IPlanningRepository, PlanningRepository>();
        services.AddScoped<IPlanningApplicationService, PlanningApplicationService>();
        return services;
    }
}
