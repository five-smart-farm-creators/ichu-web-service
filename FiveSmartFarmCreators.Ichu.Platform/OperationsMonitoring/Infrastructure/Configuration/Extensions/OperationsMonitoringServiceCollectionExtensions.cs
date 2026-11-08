using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Configuration.Extensions;

/// <summary>
///     Registers the services of the Operations and Monitoring bounded context.
/// </summary>
public static class OperationsMonitoringServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the module services. The host must also register the shared application context, the IAM module and real implementations of IEdgeGatewayAuthenticator, ICattleLookup and IDeviceAttributionLookup; no permissive or empty implementation is registered here.
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

        services.AddScoped<IMonitoringRuleRevisionService, MonitoringRuleRevisionService>();
        services.AddScoped<IMonitoringAccessControl, IamOperationsMonitoringAccessControl>();

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

    /// <summary>
    ///     Registers the development-only simulation of Cattle Information and IoT Assets, read from the "OperationsMonitoring:Simulation" section. The host calls it only in Development.
    /// </summary>
    public static IServiceCollection AddOperationsMonitoringSimulation(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MonitoringSimulationOptions>(configuration.GetSection(MonitoringSimulationOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddScoped<ICattleLookup, SimulatedCattleLookup>();
        services.AddScoped<IDeviceAttributionLookup, SimulatedDeviceAttributionLookup>();
        services.AddScoped<IEdgeGatewayAuthenticator, SimulatedEdgeGatewayAuthenticator>();
        return services;
    }
}
