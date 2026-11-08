using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

/// <summary>
/// Groups the planned activities of one ranch under a sanitary or reproductive purpose and an inclusive date range.
/// </summary>
public sealed class PlanningCampaign : AggregateRoot
{
    // Entity Framework Core materializes campaigns without running the factory.
    private PlanningCampaign()
    {
    }

    public PlanningCampaignId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public CampaignKind Kind { get; private set; }

    public DateOnly StartsOn { get; private set; }

    public DateOnly EndsOn { get; private set; }

    /// <summary>
    /// Both dates are inclusive, so a one-day campaign starts and ends on the same day.
    /// </summary>
    public static Result<PlanningCampaign> Create(Guid ranchId, string name, CampaignKind kind, DateOnly startsOn, DateOnly endsOn)
    {
        if (ranchId == Guid.Empty)
        {
            return Result<PlanningCampaign>.Failure(PlanningErrors.InvalidIdentifier);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<PlanningCampaign>.Failure(PlanningErrors.CampaignNameRequired);
        }

        if (!Enum.IsDefined(kind))
        {
            return Result<PlanningCampaign>.Failure(PlanningErrors.CampaignKindInvalid);
        }

        if (endsOn < startsOn)
        {
            return Result<PlanningCampaign>.Failure(PlanningErrors.CampaignDateRangeInvalid);
        }

        return Result<PlanningCampaign>.Success(new PlanningCampaign
        {
            Id = new PlanningCampaignId(Guid.NewGuid()),
            RanchId = ranchId,
            Name = name.Trim(),
            Kind = kind,
            StartsOn = startsOn,
            EndsOn = endsOn
        });
    }
}
