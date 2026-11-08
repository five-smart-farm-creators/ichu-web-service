using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

/// <summary>
///     Groups animals for management while keeping their identities independent of lot membership.
/// </summary>
public sealed class Lot : AggregateRoot
{
    private const int MaximumNameLength = 80;
    private const int MaximumCriterionLength = 120;

    private readonly List<LotMembership> _memberships = [];

    // Entity Framework Core materializes lots without running the factory.
    private Lot()
    {
    }

    public LotId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Criterion { get; private set; }

    /// <summary>
    ///     Advanced by every membership change, so that concurrent assignments cannot both succeed silently.
    /// </summary>
    public Guid Version { get; private set; }

    public IReadOnlyCollection<LotMembership> Memberships => _memberships.AsReadOnly();

    public static Result<Lot> Create(CreateLotCommand command)
    {
        if (command.RanchId == Guid.Empty)
        {
            return Result<Lot>.Failure(CattleInformationErrors.RanchIdentifierRequired);
        }

        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > MaximumNameLength)
        {
            return Result<Lot>.Failure(CattleInformationErrors.InvalidLotName);
        }

        if (command.Criterion?.Trim().Length > MaximumCriterionLength)
        {
            return Result<Lot>.Failure(CattleInformationErrors.InvalidLotCriterion);
        }

        var lot = new Lot
        {
            Id = new LotId(Guid.NewGuid()),
            RanchId = command.RanchId,
            Name = command.Name.Trim(),
            Criterion = string.IsNullOrWhiteSpace(command.Criterion) ? null : command.Criterion.Trim(),
            Version = Guid.NewGuid()
        };
        return Result<Lot>.Success(lot);
    }

    /// <summary>
    ///     Adds an active animal of the same ranch. Adding an animal that is already a member changes nothing.
    /// </summary>
    public Result AddCattle(Cattle cattle, Guid expectedVersion, DateTimeOffset assignedAt)
    {
        var version = VerifyVersion(expectedVersion);
        if (version.IsFailure)
        {
            return version;
        }

        if (cattle.RanchId != RanchId)
        {
            return Result.Failure(CattleInformationErrors.CattleFromAnotherRanch);
        }

        if (cattle.Status != CattleStatus.Active)
        {
            return Result.Failure(CattleInformationErrors.InactiveCattleAssignment);
        }

        if (assignedAt == default)
        {
            return Result.Failure(CattleInformationErrors.AssignmentTimeRequired);
        }

        if (_memberships.Any(membership => membership.CattleId == cattle.Id))
        {
            return Result.Success();
        }

        cattle.RecordLotAssignment();
        _memberships.Add(new LotMembership(Id, RanchId, cattle.Id, assignedAt));
        Version = Guid.NewGuid();
        return Result.Success();
    }

    /// <summary>
    ///     Removes membership without deleting the animal. Removing a non-member changes nothing.
    /// </summary>
    public Result RemoveCattle(CattleId cattleId, Guid expectedVersion)
    {
        var version = VerifyVersion(expectedVersion);
        if (version.IsFailure)
        {
            return version;
        }

        if (cattleId.Value == Guid.Empty)
        {
            return Result.Failure(CattleInformationErrors.CattleIdentifierRequired);
        }

        var membership = _memberships.SingleOrDefault(item => item.CattleId == cattleId);
        if (membership is null)
        {
            return Result.Success();
        }

        _memberships.Remove(membership);
        Version = Guid.NewGuid();
        return Result.Success();
    }

    private Result VerifyVersion(Guid expectedVersion) =>
        expectedVersion == Guid.Empty || expectedVersion != Version
            ? Result.Failure(CattleInformationErrors.LotVersionMismatch)
            : Result.Success();
}
