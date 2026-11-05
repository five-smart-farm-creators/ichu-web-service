using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure;

public static class SubscriptionPlansServiceCollectionExtensions
{
    /// <summary>
    /// Registers the module services. The host must also register the shared DbContext and real implementations of
    /// ISubscriptionAccessControl and IPaymentNotificationVerifier; no simulated payment provider is registered here.
    /// </summary>
    public static IServiceCollection AddSubscriptionPlans(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SubscriptionPlansOptions>(configuration.GetSection(SubscriptionPlansOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionPlansUnitOfWork, SubscriptionPlansUnitOfWork>();

        services.AddScoped<SubscriptionAuthorization>();
        services.AddScoped<PlanCatalogService>();
        services.AddScoped<SubscriptionService>();
        services.AddScoped<PaymentConfirmationService>();

        services.AddScoped<ISubscriptionPlansContextFacade, SubscriptionPlansContextFacade>();
        services.AddScoped<SubscriptionPlansExceptionFilter>();
        return services;
    }
}
