namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>Ranch-scoped queries that enforce the current caller's access through Iam.</summary>
public interface ICattleInformationContextFacade
{
    Task<CattleSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);
    Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);
}
