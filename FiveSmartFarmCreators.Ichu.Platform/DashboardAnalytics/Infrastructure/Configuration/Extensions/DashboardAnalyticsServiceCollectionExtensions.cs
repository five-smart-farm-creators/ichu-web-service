using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Infrastructure.Configuration.Extensions;

/// <summary>
///     Registers the DashboardAnalytics context. The host must also register the IAM, Subscription Plans, Cattle Information and Operations and Monitoring modules, because the adapters read through their facades.
/// </summary>
public static class DashboardAnalyticsServiceCollectionExtensions
{
    public static IServiceCollection AddDashboardAnalytics(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DashboardAnalyticsOptions>(configuration.GetSection(DashboardAnalyticsOptions.SectionName));
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IAnalyticsAccessControl, IamAnalyticsAccessControl>();
        services.AddScoped<IAnalyticsEntitlements, SubscriptionPlansAnalyticsEntitlements>();
        services.AddScoped<IAnalyticsSource, PlatformAnalyticsSource>();
        services.AddScoped<AnalyticsDomainService>();
        services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
        services.AddScoped<IExecutiveReportQueryService, ExecutiveReportQueryService>();
        return services;
    }
}
