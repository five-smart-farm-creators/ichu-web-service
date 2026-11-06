namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;

/// <summary>
///     The minimal animal data that monitoring rules need from Cattle Information.
/// </summary>
/// <param name="CattleId">The animal identifier.</param>
/// <param name="RanchId">The ranch that owns the animal.</param>
/// <param name="IncorporatedAt">The instant the animal joined the herd.</param>
/// <param name="DeactivatedAt">The instant the animal left the herd, if it did.</param>
public sealed record CattleSnapshot(Guid CattleId, Guid RanchId, DateTimeOffset IncorporatedAt, DateTimeOffset? DeactivatedAt)
{
    /// <summary>
    ///     Determines whether the animal belonged to the herd at an instant.
    /// </summary>
    /// <param name="instant">The instant to check.</param>
    /// <returns><see langword="true"/> when the animal had joined the herd and had not left it yet.</returns>
    public bool WasActiveAt(DateTimeOffset instant) =>
        IncorporatedAt <= instant && (DeactivatedAt is null || instant < DeactivatedAt);
}
