using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

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
