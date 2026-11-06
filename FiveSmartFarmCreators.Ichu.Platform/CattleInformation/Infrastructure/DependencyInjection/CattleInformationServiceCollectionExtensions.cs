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

/// <summary>
/// Registers module services alongside coordinator-supplied authorization and external integration adapters.
/// </summary>
public static class CattleInformationServiceCollectionExtensions
{
    /// <summary>
    /// Registers services that share the host's scoped context and require explicit integration adapters.
    /// </summary>
    /// <typeparam name="TContext">The host context registered in the same scope as repositories and the unit of work.</typeparam>
    /// <typeparam name="TAccessService">The adapter that checks current Iam permissions from trusted caller context.</typeparam>
    /// <typeparam name="TProfileIntegrationService">The adapter that reads IoT assignment and monitoring data.</typeparam>
    /// <returns>The service collection for further host composition.</returns>
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
