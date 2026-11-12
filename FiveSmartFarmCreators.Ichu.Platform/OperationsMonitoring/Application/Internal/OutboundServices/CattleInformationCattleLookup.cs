using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Maps the animal lifecycle of Cattle Information to the herd membership that the monitoring rules evaluate.
/// </summary>
public class CattleInformationCattleLookup(ICattleInformationContextFacade cattleInformation) : ICattleLookup
{
    /// <inheritdoc />
    public async Task<CattleSnapshot?> FindAsync(Guid cattleId, CancellationToken cancellationToken)
    {
        var lifecycle = await cattleInformation.FindLifecycleAsync(cattleId, cancellationToken);
        if (lifecycle is null)
        {
            return null;
        }

        // Cattle Information keeps calendar days, so membership covers the whole exit day.
        var deactivatedAt = lifecycle.ExitDate is { } exitDate ? StartOfDayUtc(exitDate.AddDays(1)) : (DateTimeOffset?)null;
        return new CattleSnapshot(lifecycle.CattleId, lifecycle.RanchId, StartOfDayUtc(lifecycle.BirthDate), deactivatedAt);
    }

    /// <inheritdoc />
    public async Task<CattleProfileSnapshot?> FindProfileAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        var animal = await cattleInformation.FindByIdAsync(ranchId, cattleId, cancellationToken);
        return animal is null
            ? null
            : new CattleProfileSnapshot(animal.Id, animal.RanchId, Enum.Parse<AnimalSex>(animal.Sex), animal.LifeStage, animal.BirthDate);
    }

    /// <inheritdoc />
    public Task<bool> LotBelongsToRanchAsync(Guid lotId, Guid ranchId, CancellationToken cancellationToken) =>
        cattleInformation.LotExistsAsync(ranchId, lotId, cancellationToken);

    private static DateTimeOffset StartOfDayUtc(DateOnly day) => new(day.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
}
