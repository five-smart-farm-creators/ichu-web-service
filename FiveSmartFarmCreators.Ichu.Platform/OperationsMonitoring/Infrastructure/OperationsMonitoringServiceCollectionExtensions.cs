using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.ACL;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure;

/// <summary>
///     Registers the services of the Operations and Monitoring bounded context.
/// </summary>
public static class OperationsMonitoringServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the module services. The host must also register the shared DbContext and real implementations
    ///     of IMonitoringAccessControl, IEdgeGatewayAuthenticator, ICattleLookup and IDeviceAttributionLookup; no
    ///     permissive or empty implementation is registered here.
    /// </summary>
    /// <param name="services">The service collection of the host.</param>
    /// <param name="configuration">The configuration that holds the OperationsMonitoring section.</param>
    /// <returns>The same service collection, to chain calls.</returns>
    public static IServiceCollection AddOperationsMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OperationsMonitoringOptions>(configuration.GetSection(OperationsMonitoringOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ITelemetryReadingRepository, TelemetryReadingRepository>();
        services.AddScoped<IMonitoringAlertRepository, MonitoringAlertRepository>();
        services.AddScoped<IGeofenceRepository, GeofenceRepository>();
        services.AddScoped<IMonitoringThresholdRepository, MonitoringThresholdRepository>();
        services.AddScoped<IMonitoringRuleSetRepository, MonitoringRuleSetRepository>();
        services.AddScoped<ILivestockOperationRepository, LivestockOperationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IMonitoringRuleRevisionService, MonitoringRuleRevisionService>();

        services.AddScoped<ITelemetryReadingCommandService, TelemetryReadingCommandService>();
        services.AddScoped<IMonitoringAlertCommandService, MonitoringAlertCommandService>();
        services.AddScoped<IEdgeSynchronizationCommandService, EdgeSynchronizationCommandService>();
        services.AddScoped<IMonitoringThresholdCommandService, MonitoringThresholdCommandService>();
        services.AddScoped<IGeofenceCommandService, GeofenceCommandService>();
        services.AddScoped<ILivestockOperationCommandService, LivestockOperationCommandService>();

        services.AddScoped<ITelemetryReadingQueryService, TelemetryReadingQueryService>();
        services.AddScoped<IMonitoringAlertQueryService, MonitoringAlertQueryService>();
        services.AddScoped<IMonitoringThresholdQueryService, MonitoringThresholdQueryService>();
        services.AddScoped<IGeofenceQueryService, GeofenceQueryService>();
        services.AddScoped<IMonitoringRuleSetQueryService, MonitoringRuleSetQueryService>();
        services.AddScoped<ILivestockOperationQueryService, LivestockOperationQueryService>();

        services.AddScoped<IOperationsMonitoringContextFacade, OperationsMonitoringContextFacade>();
        return services;
    }
}
