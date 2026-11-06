using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.DependencyInjection;

public static class CattleInformationServiceCollectionExtensions
{
    /// <summary>Registers module services against the host context and explicit integration adapters.</summary>
    public static IServiceCollection AddCattleInformation<TContext, TAccessService, TProfileIntegrationService>(
        this IServiceCollection services)
        where TContext : DbContext
        where TAccessService : class, ICattleAccessService
        where TProfileIntegrationService : class, ICattleProfileIntegrationService
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<ICattleAccessService, TAccessService>();
        services.AddScoped<ICattleProfileIntegrationService, TProfileIntegrationService>();
        services.AddScoped<ICattleRepository, CattleRepository<TContext>>();
        services.AddScoped<ILotRepository, LotRepository<TContext>>();
        services.AddScoped<ICattleUnitOfWork, CattleUnitOfWork<TContext>>();
        services.AddScoped<ICattleCommandService, CattleCommandService>();
        services.AddScoped<ILotCommandService, LotCommandService>();
        services.AddScoped<ICattleQueryService, CattleQueryService>();
        services.AddScoped<ILotQueryService, LotQueryService>();
        services.AddScoped<ICattleInformationContextFacade, CattleInformationContextFacade>();
        return services;
    }
}
