using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Filters;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Infrastructure.Configuration.Extensions;

/// <summary>
///     Registers analytics query services and the integration adapters supplied by the host.
/// </summary>
public static class DashboardAnalyticsServiceCollectionExtensions
{
    /// <summary>
    ///     Registers analytics with explicit source, subscription, and access adapters.
    /// </summary>
    /// <typeparam name="TSource">The adapter supplying analytics observations.</typeparam>
    /// <typeparam name="TEntitlements">The adapter checking subscription export rights.</typeparam>
    /// <typeparam name="TAccess">The adapter enforcing current-account ranch access.</typeparam>
    /// <param name="services">The dependency-injection collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddDashboardAnalytics<TSource, TEntitlements, TAccess>(this IServiceCollection services)
        where TSource : class, IAnalyticsSource
        where TEntitlements : class, IAnalyticsEntitlements
        where TAccess : class, IAnalyticsAccess
    {
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<IAnalyticsSource, TSource>();
        services.AddScoped<IAnalyticsEntitlements, TEntitlements>();
        services.AddScoped<IAnalyticsAccess, TAccess>();
        services.AddScoped<AnalyticsDomainService>();
        services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
        services.AddScoped<IExecutiveReportQueryService, ExecutiveReportQueryService>();
        services.AddScoped<AnalyticsExceptionFilter>();
        return services;
    }
}
