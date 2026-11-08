using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;

/// <summary>
/// Records that a planned activity was applied to an animal. Dose and clinical history stay outside Planning.
/// </summary>
public sealed class ApplicationRecord
{
    // Entity Framework Core materializes application records without running the constructor.
    private ApplicationRecord()
    {
    }

    public ApplicationRecordId Id { get; private set; }

    public PlannedActivityId ActivityId { get; private set; }

    public Guid CattleId { get; private set; }

    public DateTimeOffset AppliedAt { get; private set; }

    public Guid? InterventionId { get; private set; }

    internal static Result<ApplicationRecord> Create(
        PlannedActivityId activityId,
        Guid cattleId,
        DateTimeOffset appliedAt,
        Guid? interventionId)
    {
        if (cattleId == Guid.Empty || interventionId == Guid.Empty)
        {
            return Result<ApplicationRecord>.Failure(PlanningErrors.InvalidIdentifier);
        }

        if (appliedAt.Offset != TimeSpan.Zero)
        {
            return Result<ApplicationRecord>.Failure(PlanningErrors.UtcTimestampRequired);
        }

        return Result<ApplicationRecord>.Success(new ApplicationRecord
        {
            Id = new ApplicationRecordId(Guid.NewGuid()),
            ActivityId = activityId,
            CattleId = cattleId,
            AppliedAt = appliedAt,
            InterventionId = interventionId
        });
    }
}
