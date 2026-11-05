namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;

/// <summary>The minimal animal data that monitoring rules need from Cattle Information.</summary>
public sealed record CattleSnapshot(Guid CattleId, Guid RanchId, DateTimeOffset IncorporatedAt, DateTimeOffset? DeactivatedAt)
{
    public bool WasActiveAt(DateTimeOffset instant) =>
        IncorporatedAt <= instant && (DeactivatedAt is null || instant < DeactivatedAt);
}
