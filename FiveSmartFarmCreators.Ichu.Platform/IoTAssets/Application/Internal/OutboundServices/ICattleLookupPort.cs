using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;

/// <summary>Resolves authorized animal and lot facts without sharing foreign domain entities.</summary>
public interface ICattleLookupPort
{
    /// <summary>Returns current lifecycle facts, or a failure when the animal cannot be resolved in the ranch.</summary>
    Task<Result<CattleAssignmentTarget>> GetCattleAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);

    /// <summary>Returns whether the lot exists in the specified ranch, including an empty lot.</summary>
    Task<Result<bool>> LotExistsInRanchAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);
}
