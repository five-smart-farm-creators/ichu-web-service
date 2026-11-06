namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>Checks the authenticated caller's current ranch permissions through Iam.</summary>
public interface ICattleAccessService
{
    Task EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken);
    Task EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken);
}
