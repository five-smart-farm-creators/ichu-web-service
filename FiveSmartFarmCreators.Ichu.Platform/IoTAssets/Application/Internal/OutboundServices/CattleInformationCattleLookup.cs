using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;

/// <summary>
///     Resolves animals and lots through the Cattle Information facade, so that IoT Assets never holds Cattle entities.
/// </summary>
public class CattleInformationCattleLookup(ICattleInformationContextFacade cattleInformation) : ICattleLookupPort
{
    /// <inheritdoc />
    public async Task<Result<CattleAssignmentTarget>> GetCattleAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        var snapshot = await cattleInformation.FindByIdAsync(ranchId, cattleId, cancellationToken);
        if (snapshot is null)
        {
            return Result<CattleAssignmentTarget>.Failure(IoTAssetsErrors.TargetNotFound);
        }

        // The snapshot publishes the status as the enum name, so "Active" is the value of the active state.
        return Result<CattleAssignmentTarget>.Success(
            new CattleAssignmentTarget(snapshot.Id, snapshot.RanchId, snapshot.Status == "Active"));
    }

    /// <inheritdoc />
    public async Task<Result<bool>> LotExistsInRanchAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
        Result<bool>.Success(await cattleInformation.LotExistsAsync(ranchId, lotId, cancellationToken));
}
