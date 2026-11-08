using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

/// <summary>
///     Owns an animal's identity, productive-stage history and lifecycle within one ranch. Stage changes and exits close
///     the current period instead of deleting it, so the animal's history stays complete.
/// </summary>
public sealed class Cattle : AggregateRoot
{
    private readonly List<StagePeriod> _stagePeriods = [];

    // Entity Framework Core materializes animals without running the factory.
    private Cattle()
    {
    }

    public CattleId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public EarTag EarTag { get; private set; } = null!;

    public Breed Breed { get; private set; }

    public Sex Sex { get; private set; }

    public DateOnly BirthDate { get; private set; }

    public CattleStatus Status { get; private set; }

    public LifeStage CurrentStage { get; private set; }

    public ExitReason? ExitReason { get; private set; }

    public DateOnly? ExitDate { get; private set; }

    /// <summary>
    ///     Advanced by every change, so a caller holding an older version is refused instead of overwriting newer data.
    /// </summary>
    public Guid Version { get; private set; }

    public IReadOnlyCollection<StagePeriod> StagePeriods => _stagePeriods.AsReadOnly();

    /// <summary>
    ///     Registers an active animal whose first known stage starts on the registration date.
    /// </summary>
    public static Result<Cattle> Register(RegisterCattleCommand command, DateOnly today)
    {
        if (command.RanchId == Guid.Empty)
        {
            return Result<Cattle>.Failure(CattleInformationErrors.RanchIdentifierRequired);
        }

        var earTag = EarTag.Create(command.EarTag);
        if (earTag.IsFailure)
        {
            return Result<Cattle>.Failure(earTag.DomainError);
        }

        var details = ValidateDetails(command.Breed, command.Sex, command.BirthDate, today);
        if (details.IsFailure)
        {
            return Result<Cattle>.Failure(details.DomainError);
        }

        if (!Enum.IsDefined(command.LifeStage))
        {
            return Result<Cattle>.Failure(CattleInformationErrors.InvalidLifeStage);
        }

        var cattle = new Cattle
        {
            Id = new CattleId(Guid.NewGuid()),
            RanchId = command.RanchId,
            EarTag = earTag.Value!,
            Breed = command.Breed,
            Sex = command.Sex,
            BirthDate = command.BirthDate,
            Status = CattleStatus.Active,
            CurrentStage = command.LifeStage,
            Version = Guid.NewGuid()
        };
        cattle._stagePeriods.Add(new StagePeriod(cattle.Id, cattle.CurrentStage, today));
        cattle.AddDomainEvent(new CattleRegisteredEvent(cattle.RanchId, cattle.Id, cattle.EarTag.Value,
            cattle.Breed, cattle.Sex, cattle.BirthDate, cattle.CurrentStage));
        return Result<Cattle>.Success(cattle);
    }

    /// <summary>
    ///     Corrects an active animal's identity while keeping its birth date consistent with its recorded stages.
    /// </summary>
    public Result Update(UpdateCattleCommand command, DateOnly today)
    {
        var mutation = VerifyMutation(command.RanchId, command.CattleId, command.ExpectedVersion);
        if (mutation.IsFailure)
        {
            return mutation;
        }

        var earTag = EarTag.Create(command.EarTag);
        if (earTag.IsFailure)
        {
            return Result.Failure(earTag.DomainError);
        }

        var details = ValidateDetails(command.Breed, command.Sex, command.BirthDate, today);
        if (details.IsFailure)
        {
            return details;
        }

        if (_stagePeriods.Any(period => period.ValidFrom < command.BirthDate))
        {
            return Result.Failure(CattleInformationErrors.BirthDateAfterLifeStage);
        }

        EarTag = earTag.Value!;
        Breed = command.Breed;
        Sex = command.Sex;
        BirthDate = command.BirthDate;
        Version = Guid.NewGuid();
        return Result.Success();
    }

    /// <summary>
    ///     Changes an active animal's stage and keeps the preceding period, including a change on the same day.
    /// </summary>
    public Result ChangeStage(ChangeLifeStageCommand command, DateOnly today)
    {
        var mutation = VerifyMutation(command.RanchId, command.CattleId, command.ExpectedVersion);
        if (mutation.IsFailure)
        {
            return mutation;
        }

        if (!Enum.IsDefined(command.LifeStage))
        {
            return Result.Failure(CattleInformationErrors.InvalidLifeStage);
        }

        if (command.LifeStage == CurrentStage)
        {
            return Result.Failure(CattleInformationErrors.LifeStageAlreadyCurrent);
        }

        var currentPeriod = _stagePeriods.Single(period => period.ValidTo is null);
        if (command.ValidFrom < BirthDate || command.ValidFrom > today || command.ValidFrom < currentPeriod.ValidFrom)
        {
            return Result.Failure(CattleInformationErrors.InvalidLifeStageDate);
        }

        var previousStage = CurrentStage;
        currentPeriod.Close(command.ValidFrom);
        CurrentStage = command.LifeStage;
        _stagePeriods.Add(new StagePeriod(Id, CurrentStage, command.ValidFrom));
        Version = Guid.NewGuid();
        AddDomainEvent(new CattleLifeStageChangedEvent(RanchId, Id, previousStage, CurrentStage, command.ValidFrom));
        return Result.Success();
    }

    /// <summary>
    ///     Records an exit and closes the current stage without deleting the animal or its history.
    /// </summary>
    public Result Deactivate(DeactivateCattleCommand command, DateOnly today, DateTimeOffset recordedAt)
    {
        var mutation = VerifyMutation(command.RanchId, command.CattleId, command.ExpectedVersion);
        if (mutation.IsFailure)
        {
            return mutation;
        }

        if (!Enum.IsDefined(command.Reason))
        {
            return Result.Failure(CattleInformationErrors.InvalidExitReason);
        }

        var currentPeriod = _stagePeriods.Single(period => period.ValidTo is null);
        if (command.ExitDate < BirthDate || command.ExitDate > today || command.ExitDate < currentPeriod.ValidFrom)
        {
            return Result.Failure(CattleInformationErrors.InvalidExitDate);
        }

        currentPeriod.Close(command.ExitDate);
        Status = CattleStatus.Inactive;
        ExitReason = command.Reason;
        ExitDate = command.ExitDate;
        Version = Guid.NewGuid();
        AddDomainEvent(new CattleDeactivatedEvent(RanchId, Id, command.Reason, command.ExitDate, recordedAt.ToUniversalTime()));
        return Result.Success();
    }

    /// <summary>
    ///     Called only after the lot verified that the animal is active. The version change makes a concurrent
    ///     deactivation conflict with this assignment.
    /// </summary>
    internal void RecordLotAssignment()
    {
        Version = Guid.NewGuid();
    }

    private Result VerifyMutation(Guid ranchId, CattleId cattleId, Guid expectedVersion)
    {
        if (ranchId == Guid.Empty)
        {
            return Result.Failure(CattleInformationErrors.RanchIdentifierRequired);
        }

        if (cattleId.Value == Guid.Empty)
        {
            return Result.Failure(CattleInformationErrors.CattleIdentifierRequired);
        }

        if (ranchId != RanchId || cattleId != Id)
        {
            return Result.Failure(CattleInformationErrors.CattleNotFound);
        }

        if (expectedVersion == Guid.Empty || expectedVersion != Version)
        {
            return Result.Failure(CattleInformationErrors.CattleVersionMismatch);
        }

        return Status == CattleStatus.Active
            ? Result.Success()
            : Result.Failure(CattleInformationErrors.CattleInactive);
    }

    private static Result ValidateDetails(Breed breed, Sex sex, DateOnly birthDate, DateOnly today)
    {
        if (!Enum.IsDefined(breed))
        {
            return Result.Failure(CattleInformationErrors.InvalidBreed);
        }

        if (!Enum.IsDefined(sex))
        {
            return Result.Failure(CattleInformationErrors.InvalidSex);
        }

        return birthDate == default || birthDate > today
            ? Result.Failure(CattleInformationErrors.InvalidBirthDate)
            : Result.Success();
    }
}
