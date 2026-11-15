using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.EventHandlers;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Seeding;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Configuration.Extensions;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Shared.Infrastructure.Configuration;

/// <summary>
///     Builds the host registrations of Program.cs without a database connection and proves that every module service and
///     controller can be created, so a missing port fails in the test instead of at startup.
/// </summary>
public class DependencyInjectionCompositionTests
{
    [Fact]
    public void EveryCattleAndIoTServiceResolvesInTheHostComposition()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<ICattleRepository>());
        Assert.NotNull(services.GetRequiredService<ILotRepository>());
        Assert.NotNull(services.GetRequiredService<ICattleAccessService>());
        Assert.NotNull(services.GetRequiredService<ICattleProfileIntegrationService>());
        Assert.NotNull(services.GetRequiredService<ICattleCommandService>());
        Assert.NotNull(services.GetRequiredService<ILotCommandService>());
        Assert.NotNull(services.GetRequiredService<ICattleQueryService>());
        Assert.NotNull(services.GetRequiredService<ILotQueryService>());
        Assert.NotNull(services.GetRequiredService<ICattleInformationContextFacade>());

        Assert.NotNull(services.GetRequiredService<IIoTDeviceRepository>());
        Assert.NotNull(services.GetRequiredService<IWaterTroughRepository>());
        Assert.NotNull(services.GetRequiredService<IIoTAssetsAccessService>());
        Assert.NotNull(services.GetRequiredService<ICattleLookupPort>());
        Assert.NotNull(services.GetRequiredService<ISubscriptionEntitlementPort>());
        Assert.NotNull(services.GetRequiredService<IIoTAssetsTransaction>());
        Assert.NotNull(services.GetRequiredService<DeviceAssignmentDomainService>());
        Assert.NotNull(services.GetRequiredService<IDeviceCommandService>());
        Assert.NotNull(services.GetRequiredService<IWaterTroughCommandService>());
        Assert.NotNull(services.GetRequiredService<IDeviceQueryService>());
        Assert.NotNull(services.GetRequiredService<IWaterTroughQueryService>());
        Assert.NotNull(services.GetRequiredService<IIoTAssetsContextFacade>());
        Assert.NotNull(services.GetRequiredService<IEdgeGatewayRepository>());
        Assert.NotNull(services.GetRequiredService<IEdgeGatewayCommandService>());
        Assert.NotNull(services.GetRequiredService<IEdgeGatewayQueryService>());
    }

    [Fact]
    public void EveryDevelopmentSeederResolvesInTheHostComposition()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<DevelopmentIamSeeder>());
        Assert.NotNull(services.GetRequiredService<DevelopmentSubscriptionSeeder>());
        Assert.NotNull(services.GetRequiredService<DevelopmentCattleSeeder>());
        Assert.NotNull(services.GetRequiredService<DevelopmentIoTAssetsSeeder>());
        Assert.NotNull(services.GetRequiredService<DevelopmentMonitoringRulesSeeder>());
    }

    [Fact]
    public void EveryOperationsMonitoringServiceResolvesInTheHostComposition()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<ITelemetryReadingRepository>());
        Assert.NotNull(services.GetRequiredService<IMonitoringAlertRepository>());
        Assert.NotNull(services.GetRequiredService<IGeofenceRepository>());
        Assert.NotNull(services.GetRequiredService<IMonitoringThresholdRepository>());
        Assert.NotNull(services.GetRequiredService<IMonitoringRuleSetRepository>());
        Assert.NotNull(services.GetRequiredService<ILivestockOperationRepository>());
        Assert.NotNull(services.GetRequiredService<IOperationsMonitoringUnitOfWork>());
        Assert.NotNull(services.GetRequiredService<IMonitoringRuleRevisionService>());
        Assert.NotNull(services.GetRequiredService<IMonitoringAccessControl>());
        Assert.NotNull(services.GetRequiredService<IEdgeGatewayAuthenticator>());
        Assert.NotNull(services.GetRequiredService<ICattleLookup>());
        Assert.NotNull(services.GetRequiredService<IDeviceAttributionLookup>());
        Assert.NotNull(services.GetRequiredService<IEdgeDeviceAssignmentLookup>());
        Assert.NotNull(services.GetRequiredService<ITelemetryReadingCommandService>());
        Assert.NotNull(services.GetRequiredService<IMonitoringAlertCommandService>());
        Assert.NotNull(services.GetRequiredService<IEdgeSynchronizationCommandService>());
        Assert.NotNull(services.GetRequiredService<IMonitoringThresholdCommandService>());
        Assert.NotNull(services.GetRequiredService<IGeofenceCommandService>());
        Assert.NotNull(services.GetRequiredService<ILivestockOperationCommandService>());
        Assert.NotNull(services.GetRequiredService<ITelemetryReadingQueryService>());
        Assert.NotNull(services.GetRequiredService<IMonitoringAlertQueryService>());
        Assert.NotNull(services.GetRequiredService<IMonitoringThresholdQueryService>());
        Assert.NotNull(services.GetRequiredService<IGeofenceQueryService>());
        Assert.NotNull(services.GetRequiredService<IMonitoringRuleSetQueryService>());
        Assert.NotNull(services.GetRequiredService<IEdgeDeviceAssignmentQueryService>());
        Assert.NotNull(services.GetRequiredService<ILivestockOperationQueryService>());
        Assert.NotNull(services.GetRequiredService<IReproductiveHistoryQueryService>());
        Assert.NotNull(services.GetRequiredService<ReproductiveHistoryCalculator>());
        Assert.NotNull(services.GetRequiredService<IOperationsMonitoringContextFacade>());
    }

    [Fact]
    public void EveryCattleAndIoTControllerReceivesItsDependencies()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(ActivatorUtilities.CreateInstance<CattleController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<LotsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<DevicesController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<DeviceAssignmentsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<WaterTroughsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<RanchEdgeGatewaysController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<CattleDeactivatedEventHandler>(services));
    }

    [Fact]
    public void EveryOperationsMonitoringControllerReceivesItsDependencies()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(ActivatorUtilities.CreateInstance<AlertsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<EdgeGatewaysController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<GeofencesController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<LivestockOperationsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<MonitoringThresholdsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<TelemetryController>(services));
    }

    [Fact]
    public void EveryPlanningServiceResolvesInTheHostComposition()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<IPlanningCampaignRepository>());
        Assert.NotNull(services.GetRequiredService<IPlannedActivityRepository>());
        Assert.NotNull(services.GetRequiredService<IWithdrawalPeriodRepository>());
        Assert.NotNull(services.GetRequiredService<INutritionPlanRepository>());
        Assert.NotNull(services.GetRequiredService<IPlanningAccessControl>());
        Assert.NotNull(services.GetRequiredService<IPlanningCattleLookup>());
        Assert.NotNull(services.GetRequiredService<IPlanningLotLookup>());
        Assert.NotNull(services.GetRequiredService<IClinicalInterventionLookup>());
        Assert.NotNull(services.GetRequiredService<IPlanningTransaction>());
        Assert.NotNull(services.GetRequiredService<NutritionPlanScheduler>());
        Assert.NotNull(services.GetRequiredService<PlanningGuard>());
        Assert.NotNull(services.GetRequiredService<IPlanningCampaignCommandService>());
        Assert.NotNull(services.GetRequiredService<IPlannedActivityCommandService>());
        Assert.NotNull(services.GetRequiredService<IWithdrawalPeriodCommandService>());
        Assert.NotNull(services.GetRequiredService<INutritionPlanCommandService>());
        Assert.NotNull(services.GetRequiredService<IPlanningCampaignQueryService>());
        Assert.NotNull(services.GetRequiredService<IPlannedActivityQueryService>());
        Assert.NotNull(services.GetRequiredService<IWithdrawalPeriodQueryService>());
        Assert.NotNull(services.GetRequiredService<INutritionPlanQueryService>());
    }

    [Fact]
    public void EveryPlanningControllerReceivesItsDependencies()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(ActivatorUtilities.CreateInstance<PlanningCampaignsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<PlanningActivitiesController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<PlanningCalendarController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<WithdrawalPeriodsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<NutritionPlansController>(services));
    }

    [Fact]
    public void EveryDashboardAnalyticsServiceResolvesInTheHostComposition()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<IAnalyticsAccessControl>());
        Assert.NotNull(services.GetRequiredService<IAnalyticsEntitlements>());
        Assert.NotNull(services.GetRequiredService<IAnalyticsSource>());
        Assert.NotNull(services.GetRequiredService<AnalyticsDomainService>());
        Assert.NotNull(services.GetRequiredService<IAnalyticsQueryService>());
        Assert.NotNull(services.GetRequiredService<IExecutiveReportQueryService>());
    }

    [Fact]
    public void EveryDashboardAnalyticsControllerReceivesItsDependencies()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(ActivatorUtilities.CreateInstance<DashboardController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<EpidemiologicalCurveController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<PortfolioController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<ReportsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<TrendsController>(services));
    }

    [Fact]
    public void EveryIamServiceResolvesInTheHostComposition()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<IUserAccountRepository>());
        Assert.NotNull(services.GetRequiredService<IRanchRegistrationRepository>());
        Assert.NotNull(services.GetRequiredService<IHerdAdvisoryRepository>());
        Assert.NotNull(services.GetRequiredService<IIdentityProvider>());
        Assert.NotNull(services.GetRequiredService<ICurrentAccount>());
        Assert.NotNull(services.GetRequiredService<IAccountCommandService>());
        Assert.NotNull(services.GetRequiredService<IProfessionalProfileCommandService>());
        Assert.NotNull(services.GetRequiredService<IHerdAdvisoryCommandService>());
        Assert.NotNull(services.GetRequiredService<IAccountQueryService>());
        Assert.NotNull(services.GetRequiredService<IRanchQueryService>());
        Assert.NotNull(services.GetRequiredService<IHerdAdvisoryQueryService>());
        Assert.NotNull(services.GetRequiredService<IAuthorizationQueryService>());
        Assert.NotNull(services.GetRequiredService<IRanchAccessCodeGenerator>());
        Assert.NotNull(services.GetRequiredService<IIamContextFacade>());
    }

    [Fact]
    public void EveryIamControllerReceivesItsDependencies()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(ActivatorUtilities.CreateInstance<AccountsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<RanchesController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<HerdAdvisoriesController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<ProfilesController>(services));
    }

    [Fact]
    public void EverySubscriptionPlansServiceAndControllerResolvesInTheHostComposition()
    {
        using var provider = BuildHost();
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;

        Assert.NotNull(services.GetRequiredService<IPaymentGateway>());
        Assert.NotNull(services.GetRequiredService<ISubscriptionCommandService>());
        Assert.NotNull(ActivatorUtilities.CreateInstance<PlansController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<SubscriptionsController>(services));
        Assert.NotNull(ActivatorUtilities.CreateInstance<StripeWebhooksController>(services));
    }

    private static ServiceProvider BuildHost()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SubscriptionPlans:CurrentTermsVersion"] = "2026-11",
                ["Firebase:ProjectId"] = "composition-test",
                ["Firebase:ApiKey"] = "composition-test"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddHttpContextAccessor();
        // The host registers IConfiguration itself, as WebApplicationBuilder does for Program.cs.
        services.AddSingleton<IConfiguration>(configuration);
        services.AddDbContext<AppDbContext>((serviceProvider, options) => options
            .UseNpgsql("Host=localhost;Port=5433;Database=ichu_composition;Username=composition;Password=composition")
            .AddInterceptors(serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>()));
        services.AddScoped<DbContext>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
        services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ExceptionHandlingCommandBehavior<>));
        services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(TransactionCommandBehavior<>));
        services.AddCortexMediator([typeof(AppDbContext)]);
        services.AddIam(configuration);
        services.AddSubscriptionPlans(configuration);
        services.AddOperationsMonitoring(configuration);
        services.AddCattleInformation();
        services.AddIoTAssets(configuration);
        services.AddPlanning();
        services.AddDashboardAnalytics(configuration);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }
}
