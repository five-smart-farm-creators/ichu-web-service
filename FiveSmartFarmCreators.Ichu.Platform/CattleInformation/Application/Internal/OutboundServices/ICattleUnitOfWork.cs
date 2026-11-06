using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>
/// Persists tracked module changes and pending lifecycle events in one transaction.
/// </summary>
public interface ICattleUnitOfWork
{
    /// <summary>
    /// Commits state changes and durable events, clearing domain events only after a successful save.
    /// </summary>
    /// <param name="cattle">The animal supplying pending events, or null for a lot-only operation.</param>
    /// <param name="cancellationToken">Cancels the persistence operation.</param>
    Task CommitAsync(Cattle? cattle, CancellationToken cancellationToken);
}
