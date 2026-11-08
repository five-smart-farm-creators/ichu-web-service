namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     The animal data that the monitoring rules need from Cattle Information.
/// </summary>
public record CattleSnapshot(Guid CattleId, Guid RanchId, DateTimeOffset IncorporatedAt, DateTimeOffset? DeactivatedAt)
{
    /// <summary>
    ///     Determines whether the animal belonged to the herd at an instant. The herd membership is half-open: joining counts, leaving does not.
    /// </summary>
    public bool WasActiveAt(DateTimeOffset instant) =>
        IncorporatedAt <= instant && (DeactivatedAt is null || instant < DeactivatedAt);
}
