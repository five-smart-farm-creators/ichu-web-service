using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Interfaces.Rest;

public class PlanningResourceTransformTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly DateTimeOffset ScheduledAt = new(2026, 11, 20, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ActivityResourceExposesItsCampaignAsAPlainIdentifier()
    {
        var campaignId = new PlanningCampaignId(Guid.NewGuid());
        var activity = PlannedActivity.Create(RanchId, campaignId, null, "Vaccination", ScheduledAt).Value!;

        var resource = PlannedActivityResourceFromEntityAssembler.ToResourceFromEntity(activity);

        Assert.Equal(activity.Id.Value, resource.Id);
        Assert.Equal(campaignId.Value, resource.CampaignId);
    }

    [Fact]
    public void ScheduleCommandTypesTheCampaignIdentifierOfTheRequest()
    {
        var campaignId = Guid.NewGuid();

        var command = SchedulePlanningActivityCommandFromResourceAssembler.ToCommandFromResource(
            RanchId, new SchedulePlanningActivityResource(campaignId, null, "Vaccination", ScheduledAt));

        Assert.Equal(new PlanningCampaignId(campaignId), command.CampaignId);
    }

    [Fact]
    public void ActivityResourceKeepsItsContractFields()
    {
        var fields = typeof(PlanningActivityResource).GetProperties().Select(property => property.Name);

        Assert.Equal(
            new[] { "Id", "RanchId", "CampaignId", "CattleId", "Title", "ScheduledAt" }.Order(),
            fields.Order());
    }

    [Fact]
    public void WithdrawalPeriodResourceKeepsItsContractFields()
    {
        var fields = typeof(WithdrawalPeriodResource).GetProperties().Select(property => property.Name);

        Assert.Equal(
            new[] { "Id", "RanchId", "CattleId", "InterventionId", "StartsAt", "EndsAt" }.Order(),
            fields.Order());
    }
}
