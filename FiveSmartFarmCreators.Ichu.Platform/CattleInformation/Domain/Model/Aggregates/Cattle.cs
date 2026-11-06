using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

public sealed class Cattle
{
    private readonly List<StagePeriod> _stagePeriods = [];
    private readonly List<ICattleEvent> _events = [];

    private Cattle()
    {
    }

    public Cattle(RegisterCattleCommand command, DateOnly today)
    {
        if (command.RanchId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "A ranch identifier is required.");
        }

        var earTag = new EarTag(command.EarTag);
        ValidateDetails(command.Breed, command.Sex, command.BirthDate, today);
        ValidateEnum(command.LifeStage, "life stage");

        Id = Guid.NewGuid();
        RanchId = command.RanchId;
        EarTag = earTag;
        Breed = command.Breed;
        Sex = command.Sex;
        BirthDate = command.BirthDate;
        Status = CattleStatus.Active;
        CurrentStage = command.LifeStage;
        Version = Guid.NewGuid();
        _stagePeriods.Add(new StagePeriod(Id, CurrentStage, today));
        _events.Add(new CattleRegisteredEvent(
            Guid.NewGuid(), DateTimeOffset.UtcNow, RanchId, Id, EarTag.Value,
            Breed, Sex, BirthDate, CurrentStage));
    }

    public Guid Id { get; private set; }
    public Guid RanchId { get; private set; }
    public EarTag EarTag { get; private set; } = null!;
    public Breed Breed { get; private set; }
    public Sex Sex { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public CattleStatus Status { get; private set; }
    public LifeStage CurrentStage { get; private set; }
    public ExitReason? ExitReason { get; private set; }
    public DateOnly? ExitDate { get; private set; }
    public Guid Version { get; private set; }
    public IReadOnlyCollection<StagePeriod> StagePeriods => _stagePeriods.AsReadOnly();
    public IReadOnlyCollection<ICattleEvent> Events => _events.AsReadOnly();

    public void Update(UpdateCattleCommand command, DateOnly today)
    {
        ValidateMutation(command.RanchId, command.CattleId, command.ExpectedVersion);
        var earTag = new EarTag(command.EarTag);
        ValidateDetails(command.Breed, command.Sex, command.BirthDate, today);
        if (_stagePeriods.Any(period => period.ValidFrom < command.BirthDate))
        {
            throw new CattleInformationException(
                "invalid_input", "The birth date cannot follow a recorded life stage.");
        }

        EarTag = earTag;
        Breed = command.Breed;
        Sex = command.Sex;
        BirthDate = command.BirthDate;
        Version = Guid.NewGuid();
    }

    public void ChangeStage(ChangeLifeStageCommand command, DateOnly today)
    {
        ValidateMutation(command.RanchId, command.CattleId, command.ExpectedVersion);
        ValidateEnum(command.LifeStage, "life stage");
        if (command.LifeStage == CurrentStage)
        {
            throw new CattleInformationException("invalid_state", "The life stage is already current.");
        }

        var currentPeriod = _stagePeriods.Single(period => period.ValidTo is null);
        if (command.ValidFrom < BirthDate || command.ValidFrom > today ||
            command.ValidFrom < currentPeriod.ValidFrom)
        {
            throw new CattleInformationException(
                "invalid_input", "The life stage date cannot precede the current period or be in the future.");
        }

        var previousStage = CurrentStage;
        currentPeriod.Close(command.ValidFrom);
        CurrentStage = command.LifeStage;
        _stagePeriods.Add(new StagePeriod(Id, CurrentStage, command.ValidFrom));
        Version = Guid.NewGuid();
        _events.Add(new CattleLifeStageChangedEvent(
            Guid.NewGuid(), DateTimeOffset.UtcNow, RanchId, Id,
            previousStage, CurrentStage, command.ValidFrom));
    }

    public void Deactivate(DeactivateCattleCommand command, DateOnly today)
    {
        ValidateMutation(command.RanchId, command.CattleId, command.ExpectedVersion);
        ValidateEnum(command.Reason, "exit reason");
        var currentPeriod = _stagePeriods.Single(period => period.ValidTo is null);
        if (command.ExitDate < BirthDate || command.ExitDate > today ||
            command.ExitDate < currentPeriod.ValidFrom)
        {
            throw new CattleInformationException(
                "invalid_input", "The exit date cannot precede the current life stage or be in the future.");
        }

        currentPeriod.Close(command.ExitDate);
        Status = CattleStatus.Inactive;
        ExitReason = command.Reason;
        ExitDate = command.ExitDate;
        Version = Guid.NewGuid();
        _events.Add(new CattleDeactivatedEvent(
            Guid.NewGuid(), DateTimeOffset.UtcNow, RanchId, Id, command.Reason, command.ExitDate));
    }

    public void ClearEvents()
    {
        _events.Clear();
    }

    internal void RecordLotAssignment()
    {
        if (Status != CattleStatus.Active)
        {
            throw new CattleInformationException("invalid_state", "Inactive cattle cannot be assigned to a lot.");
        }

        // The cattle version protects lot assignment against concurrent deactivation.
        Version = Guid.NewGuid();
    }

    private void ValidateMutation(Guid ranchId, Guid cattleId, Guid expectedVersion)
    {
        if (ranchId == Guid.Empty || cattleId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "Ranch and cattle identifiers are required.");
        }

        if (ranchId != RanchId || cattleId != Id)
        {
            throw new CattleInformationException("not_found", "Cattle was not found in the ranch.");
        }

        if (expectedVersion == Guid.Empty || expectedVersion != Version)
        {
            throw new CattleInformationException("conflict", "Cattle has changed. Reload it before retrying.");
        }

        if (Status != CattleStatus.Active)
        {
            throw new CattleInformationException("invalid_state", "Inactive cattle cannot be changed.");
        }
    }

    private static void ValidateDetails(Breed breed, Sex sex, DateOnly birthDate, DateOnly today)
    {
        ValidateEnum(breed, "breed");
        ValidateEnum(sex, "sex");
        if (birthDate == default || birthDate > today)
        {
            throw new CattleInformationException("invalid_input", "A valid birth date that is not in the future is required.");
        }
    }

    private static void ValidateEnum<T>(T value, string field) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new CattleInformationException("invalid_input", $"The {field} is invalid.");
        }
    }
}
