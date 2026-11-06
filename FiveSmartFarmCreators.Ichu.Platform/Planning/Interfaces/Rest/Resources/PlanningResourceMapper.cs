using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

internal static class PlanningResourceMapper
{
    internal static PlanningCampaignResource ToResource(PlanningCampaign campaign)
    {
        return new PlanningCampaignResource(
            campaign.Id,
            campaign.RanchId,
            campaign.Name,
            campaign.Kind,
            campaign.StartsOn,
            campaign.EndsOn);
    }

    internal static PlanningActivityResource ToResource(PlannedActivity activity)
    {
        return new PlanningActivityResource(
            activity.Id,
            activity.RanchId,
            activity.CampaignId,
            activity.CattleId,
            activity.Title,
            activity.ScheduledAt);
    }

    internal static ApplicationRecordResource ToResource(ApplicationRecord application)
    {
        return new ApplicationRecordResource(
            application.Id,
            application.ActivityId,
            application.CattleId,
            application.AppliedAt,
            application.InterventionId);
    }

    internal static FollowUpResource ToResource(FollowUp followUp)
    {
        return new FollowUpResource(followUp.Id, followUp.ActivityId, followUp.DueAt);
    }

    internal static WithdrawalPeriodResource ToResource(WithdrawalPeriod withdrawalPeriod)
    {
        return new WithdrawalPeriodResource(
            withdrawalPeriod.Id,
            withdrawalPeriod.RanchId,
            withdrawalPeriod.CattleId,
            withdrawalPeriod.InterventionId,
            withdrawalPeriod.StartsAt,
            withdrawalPeriod.EndsAt);
    }
}
