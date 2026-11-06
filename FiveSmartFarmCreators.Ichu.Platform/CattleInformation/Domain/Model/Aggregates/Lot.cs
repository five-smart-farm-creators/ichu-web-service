using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

/// <summary>
/// Groups animals for management while keeping their identities independent of lot membership.
/// </summary>
public sealed class Lot
{
    private readonly List<LotMembership> _memberships = [];

    private Lot()
    {
    }

    public Lot(CreateLotCommand command)
    {
        if (command.RanchId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "A ranch identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > 80)
        {
            throw new CattleInformationException("invalid_input", "The lot name must contain between 1 and 80 characters.");
        }

        if (command.Criterion?.Trim().Length > 120)
        {
            throw new CattleInformationException("invalid_input", "The lot criterion cannot exceed 120 characters.");
        }

        Id = Guid.NewGuid();
        RanchId = command.RanchId;
        Name = command.Name.Trim();
        Criterion = string.IsNullOrWhiteSpace(command.Criterion) ? null : command.Criterion.Trim();
        Version = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public Guid RanchId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Criterion { get; private set; }
    public Guid Version { get; private set; }
    public IReadOnlyCollection<LotMembership> Memberships => _memberships.AsReadOnly();

    /// <summary>
    /// Adds active cattle from the same ranch; a new membership advances both versions to guard against concurrent deactivation.
    /// </summary>
    /// <param name="cattle">The animal whose identity remains independent of the lot.</param>
    /// <param name="expectedVersion">The lot version observed by the caller.</param>
    /// <param name="assignedAt">The assignment instant, stored in UTC for a new membership.</param>
    /// <remarks>A duplicate membership leaves both versions unchanged when the expected lot version is current.</remarks>
    /// <exception cref="CattleInformationException">The lot version is stale, the animal belongs to another ranch or is inactive, or the timestamp is missing.</exception>
    public void AddCattle(Cattle cattle, Guid expectedVersion, DateTimeOffset assignedAt)
    {
        ValidateVersion(expectedVersion);
        if (cattle.RanchId != RanchId)
        {
            throw new CattleInformationException("invalid_input", "Cattle must belong to the lot ranch.");
        }

        if (cattle.Status != CattleStatus.Active)
        {
            throw new CattleInformationException("invalid_state", "Inactive cattle cannot be assigned to a lot.");
        }

        if (assignedAt == default)
        {
            throw new CattleInformationException("invalid_input", "An assignment timestamp is required.");
        }

        if (_memberships.Any(membership => membership.CattleId == cattle.Id))
        {
            return;
        }

        cattle.RecordLotAssignment();
        _memberships.Add(new LotMembership(Id, RanchId, cattle.Id, assignedAt));
        Version = Guid.NewGuid();
    }

    /// <summary>
    /// Removes membership without deleting the animal; an absent membership leaves a current lot version unchanged.
    /// </summary>
    /// <exception cref="CattleInformationException">The lot version is stale or the animal identifier is empty.</exception>
    public void RemoveCattle(Guid cattleId, Guid expectedVersion)
    {
        ValidateVersion(expectedVersion);
        if (cattleId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "A cattle identifier is required.");
        }

        var membership = _memberships.SingleOrDefault(item => item.CattleId == cattleId);
        if (membership is null)
        {
            return;
        }

        _memberships.Remove(membership);
        Version = Guid.NewGuid();
    }

    private void ValidateVersion(Guid expectedVersion)
    {
        if (expectedVersion == Guid.Empty || expectedVersion != Version)
        {
            throw new CattleInformationException("conflict", "The lot has changed. Reload it before retrying.");
        }
    }
}
