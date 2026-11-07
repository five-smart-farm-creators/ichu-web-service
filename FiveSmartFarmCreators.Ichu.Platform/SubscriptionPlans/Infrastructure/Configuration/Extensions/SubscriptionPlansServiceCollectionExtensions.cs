using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Simulation;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Configuration.Extensions;

public static class SubscriptionPlansServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the module services. The host must also register the shared database context, the IAM module and
    ///     localization. The application fails at startup when the terms version is missing.
    /// </summary>
    public static IServiceCollection AddSubscriptionPlans(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SubscriptionPlansOptions>()
            .Bind(configuration.GetSection(SubscriptionPlansOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.CurrentTermsVersion),
                "The SubscriptionPlans:CurrentTermsVersion setting is required.")
            .ValidateOnStart();
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionAccessService, IamSubscriptionAccessService>();
        services.AddScoped<ISubscriptionCommandService, SubscriptionCommandService>();
        services.AddScoped<IPlanQueryService, PlanQueryService>();
        services.AddScoped<ISubscriptionQueryService, SubscriptionQueryService>();
        services.AddScoped<ISubscriptionPlansContextFacade, SubscriptionPlansContextFacade>();
        services.AddScoped<DevelopmentSubscriptionSeeder>();
        return services;
    }
}
