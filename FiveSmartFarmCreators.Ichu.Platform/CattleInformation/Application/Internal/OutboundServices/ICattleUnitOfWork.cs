using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>Atomically persists tracked changes and the animal's pending integration events.</summary>
public interface ICattleUnitOfWork
{
    Task CommitAsync(Cattle? cattle, CancellationToken cancellationToken);
}
