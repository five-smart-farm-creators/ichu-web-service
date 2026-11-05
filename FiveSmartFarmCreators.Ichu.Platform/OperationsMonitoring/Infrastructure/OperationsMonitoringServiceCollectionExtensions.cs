using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure;

public static class OperationsMonitoringServiceCollectionExtensions
{
    /// <summary>
    /// Registers the module services. The host must also register the shared DbContext and real implementations of
    /// IMonitoringAccessControl, IEdgeGatewayAuthenticator, ICattleLookup and IDeviceAttributionLookup; no
    /// permissive or empty implementation is registered here.
    /// </summary>
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
        services.AddScoped<IOperationsMonitoringUnitOfWork, OperationsMonitoringUnitOfWork>();

        services.AddScoped<MonitoringAuthorization>();
        services.AddScoped<SyncEntryProcessor>();
        services.AddScoped<EdgeSynchronizationService>();
        services.AddScoped<TelemetryQueryService>();
        services.AddScoped<AlertService>();
        services.AddScoped<MonitoringRulesService>();
        services.AddScoped<LivestockOperationService>();

        services.AddScoped<IOperationsMonitoringContextFacade, OperationsMonitoringContextFacade>();
        services.AddScoped<OperationsMonitoringExceptionFilter>();
        return services;
    }
}
