using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Domain;

public class PlanningCampaignTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly DateOnly StartsOn = new(2026, 11, 1);

    [Fact]
    public void Create_WithAnEndBeforeTheStart_IsRejected()
    {
        var result = PlanningCampaign.Create(RanchId, "Vaccination", CampaignKind.Sanitary, StartsOn, StartsOn.AddDays(-1));

        Assert.Equal(PlanningErrors.CampaignDateRangeInvalid.Code, result.DomainError.Code);
    }

    [Fact]
    public void Create_WithABlankName_IsRejected()
    {
        var result = PlanningCampaign.Create(RanchId, "  ", CampaignKind.Sanitary, StartsOn, StartsOn);

        Assert.Equal(PlanningErrors.CampaignNameRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void Create_KeepsASingleDayCampaignInclusiveAndTrimsItsName()
    {
        var result = PlanningCampaign.Create(RanchId, " Vaccination ", CampaignKind.Sanitary, StartsOn, StartsOn);

        Assert.True(result.IsSuccess);
        Assert.Equal("Vaccination", result.Value!.Name);
        Assert.Equal(result.Value.StartsOn, result.Value.EndsOn);
    }
}
