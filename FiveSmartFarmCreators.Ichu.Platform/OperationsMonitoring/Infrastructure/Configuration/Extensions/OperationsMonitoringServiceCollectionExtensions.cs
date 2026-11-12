using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Integration;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Configuration.Extensions;

/// <summary>
///     Registers the services of the Operations and Monitoring bounded context.
/// </summary>
public static class OperationsMonitoringServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the module services and its adapters for the Cattle Information and IoT Assets facades. The host must also register the shared application context, the IAM module and those two modules.
    /// </summary>
    public static IServiceCollection AddOperationsMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OperationsMonitoringOptions>(configuration.GetSection(OperationsMonitoringOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();

        services.AddScoped<ITelemetryReadingRepository, TelemetryReadingRepository>();
        services.AddScoped<IMonitoringAlertRepository, MonitoringAlertRepository>();
        services.AddScoped<IGeofenceRepository, GeofenceRepository>();
        services.AddScoped<IMonitoringThresholdRepository, MonitoringThresholdRepository>();
        services.AddScoped<IMonitoringRuleSetRepository, MonitoringRuleSetRepository>();
        services.AddScoped<ILivestockOperationRepository, LivestockOperationRepository>();
        services.AddScoped<IOperationsMonitoringUnitOfWork, OperationsMonitoringUnitOfWork>();

        services.AddScoped<IMonitoringRuleRevisionService, MonitoringRuleRevisionService>();
        services.AddScoped<ReproductiveHistoryCalculator>();
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
        services.AddScoped<IReproductiveHistoryQueryService, ReproductiveHistoryQueryService>();

        services.AddScoped<ICattleLookup, CattleInformationCattleLookup>();
        services.AddScoped<IDeviceAttributionLookup, IoTAssetsDeviceAttributionLookup>();
        services.AddScoped<IEdgeGatewayAuthenticator, IoTAssetsEdgeGatewayAuthenticator>();

        services.AddScoped<IOperationsMonitoringContextFacade, OperationsMonitoringContextFacade>();
        services.AddScoped<DevelopmentMonitoringRulesSeeder>();
        return services;
    }
}
