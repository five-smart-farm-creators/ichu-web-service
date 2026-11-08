using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Configuration.Extensions;

public static class IoTAssetsServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the module services. The host must also register the shared database context, the IAM module, the
    ///     Cattle Information and Subscription Plans facades, and localization.
    /// </summary>
    public static IServiceCollection AddIoTAssets(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DeviceHealthOptions>(configuration.GetSection(DeviceHealthOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IIoTDeviceRepository, IoTDeviceRepository>();
        services.AddScoped<IWaterTroughRepository, WaterTroughRepository>();
        services.AddScoped<IIoTAssetsTransaction, IoTAssetsTransaction>();
        services.AddScoped<DeviceAssignmentDomainService>();
        services.AddScoped<IIoTAssetsAccessService, IamIoTAssetsAccessService>();
        services.AddScoped<ICattleLookupPort, CattleInformationCattleLookup>();
        services.AddScoped<ISubscriptionEntitlementPort, SubscriptionPlansEntitlementAdapter>();
        services.AddScoped<IDeviceCommandService, DeviceCommandService>();
        services.AddScoped<IWaterTroughCommandService, WaterTroughCommandService>();
        services.AddScoped<IDeviceQueryService, DeviceQueryService>();
        services.AddScoped<IWaterTroughQueryService, WaterTroughQueryService>();
        services.AddScoped<IIoTAssetsContextFacade, IoTAssetsContextFacade>();
        return services;
    }
}
