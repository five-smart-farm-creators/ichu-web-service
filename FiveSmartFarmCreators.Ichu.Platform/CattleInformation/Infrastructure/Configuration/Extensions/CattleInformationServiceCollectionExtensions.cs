using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Configuration.Extensions;

public static class CattleInformationServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the module services. The host must also register the shared database context, the IAM module, the
    ///     IoT Assets and Operations and Monitoring facades, and localization.
    /// </summary>
    public static IServiceCollection AddCattleInformation(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<ICattleRepository, CattleRepository>();
        services.AddScoped<ILotRepository, LotRepository>();
        services.AddScoped<ICattleAccessService, IamCattleAccessService>();
        services.AddScoped<ICattleProfileIntegrationService, IoTAssetsCattleProfileIntegrationService>();
        services.AddScoped<ICattleCommandService, CattleCommandService>();
        services.AddScoped<ILotCommandService, LotCommandService>();
        services.AddScoped<ICattleQueryService, CattleQueryService>();
        services.AddScoped<ILotQueryService, LotQueryService>();
        services.AddScoped<ICattleInformationContextFacade, CattleInformationContextFacade>();
        return services;
    }
}
