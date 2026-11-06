using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Enums;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

/// <summary>
/// Groups planned activities for one ranch and a sanitary or reproductive purpose.
/// </summary>
public sealed class PlanningCampaign
{
    private PlanningCampaign(Guid id, Guid ranchId, string name, CampaignKind kind, DateOnly startsOn, DateOnly endsOn)
    {
        Id = id;
        RanchId = ranchId;
        Name = name;
        Kind = kind;
        StartsOn = startsOn;
        EndsOn = endsOn;
    }

    /// <summary>
    /// Gets the campaign identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the ranch identifier supplied by the caller and checked through the access contract.
    /// </summary>
    public Guid RanchId { get; }

    /// <summary>
    /// Gets the campaign name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the sanitary or reproductive purpose.
    /// </summary>
    public CampaignKind Kind { get; }

    /// <summary>
    /// Gets the first inclusive campaign date.
    /// </summary>
    public DateOnly StartsOn { get; }

    /// <summary>
    /// Gets the last inclusive campaign date.
    /// </summary>
    public DateOnly EndsOn { get; }

    /// <summary>
    /// Creates a campaign after validating its identifiers and date range.
    /// </summary>
    /// <param name="id">The campaign identifier.</param>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="name">The campaign name.</param>
    /// <param name="kind">The campaign purpose.</param>
    /// <param name="startsOn">The first inclusive campaign date.</param>
    /// <param name="endsOn">The last inclusive campaign date.</param>
    /// <returns>A valid planning campaign.</returns>
    /// <exception cref="PlanningDomainException">An identifier, name, kind, or date range is invalid.</exception>
    public static PlanningCampaign Create(Guid id, Guid ranchId, string name, CampaignKind kind, DateOnly startsOn, DateOnly endsOn)
    {
        if (id == Guid.Empty || ranchId == Guid.Empty)
        {
            throw new PlanningDomainException("planning.invalid_identifier", "Campaign and ranch identifiers must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new PlanningDomainException("planning.campaign_name_required", "A campaign name is required.");
        }

        if (!Enum.IsDefined(kind))
        {
            throw new PlanningDomainException("planning.campaign_kind_invalid", "The campaign kind is not supported.");
        }

        if (endsOn < startsOn)
        {
            throw new PlanningDomainException("planning.campaign_date_range_invalid", "The campaign end date must be on or after its start date.");
        }

        return new PlanningCampaign(id, ranchId, name.Trim(), kind, startsOn, endsOn);
    }
}
