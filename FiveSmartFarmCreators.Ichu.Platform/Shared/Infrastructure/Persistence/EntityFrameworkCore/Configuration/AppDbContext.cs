using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Database context shared by every bounded context of the platform.
/// </summary>
/// <param name="options">The options that configure the context.</param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    } 

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Only IAM is mapped from the assembly; Planning joins in its own phase and migration.
        builder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly,
            type => type.Namespace is not null && type.Namespace.StartsWith("FiveSmartFarmCreators.Ichu.Platform.Iam.", StringComparison.Ordinal));
        builder.ApplyOperationsMonitoringConfiguration();
        builder.ApplySubscriptionPlansConfiguration();
        builder.ApplyCattleInformationConfiguration();
        builder.ApplyIoTAssetsConfiguration();
    }
}
