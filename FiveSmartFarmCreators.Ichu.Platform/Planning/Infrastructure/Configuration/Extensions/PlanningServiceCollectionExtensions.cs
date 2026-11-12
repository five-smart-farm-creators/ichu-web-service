using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Configuration.Extensions;

/// <summary>
///     Registers the Planning context. The host must also register the shared database context and unit of work, the IAM module, and the Cattle Information and Operations and Monitoring facades.
/// </summary>
public static class PlanningServiceCollectionExtensions
{
    public static IServiceCollection AddPlanning(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IPlanningCampaignRepository, PlanningCampaignRepository>();
        services.AddScoped<IPlannedActivityRepository, PlannedActivityRepository>();
        services.AddScoped<IWithdrawalPeriodRepository, WithdrawalPeriodRepository>();
        services.AddScoped<INutritionPlanRepository, NutritionPlanRepository>();
        services.AddScoped<IPlanningAccessControl, IamPlanningAccessControl>();
        services.AddScoped<IPlanningCattleLookup, CattleInformationPlanningCattleLookup>();
        services.AddScoped<IPlanningLotLookup, CattleInformationPlanningLotLookup>();
        services.AddScoped<IClinicalInterventionLookup, OperationsMonitoringClinicalInterventionLookup>();
        services.AddScoped<IPlanningTransaction, PlanningTransaction>();
        services.AddScoped<NutritionPlanScheduler>();
        services.AddScoped<PlanningGuard>();
        services.AddScoped<IPlanningCampaignCommandService, PlanningCampaignCommandService>();
        services.AddScoped<IPlannedActivityCommandService, PlannedActivityCommandService>();
        services.AddScoped<IWithdrawalPeriodCommandService, WithdrawalPeriodCommandService>();
        services.AddScoped<INutritionPlanCommandService, NutritionPlanCommandService>();
        services.AddScoped<IPlanningCampaignQueryService, PlanningCampaignQueryService>();
        services.AddScoped<IPlannedActivityQueryService, PlannedActivityQueryService>();
        services.AddScoped<IWithdrawalPeriodQueryService, WithdrawalPeriodQueryService>();
        services.AddScoped<INutritionPlanQueryService, NutritionPlanQueryService>();
        return services;
    }
}
