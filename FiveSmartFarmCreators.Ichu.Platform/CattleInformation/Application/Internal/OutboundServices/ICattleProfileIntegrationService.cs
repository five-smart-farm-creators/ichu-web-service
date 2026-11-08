using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>
///     Reads an animal's collar and reception data from the contexts that own them, without storing copies.
/// </summary>
public interface ICattleProfileIntegrationService
{
    /// <summary>
    ///     Retrieves the device details of an animal whose ranch access the caller has already been checked for.
    /// </summary>
    Task<CattleDeviceDetails> GetAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);
}
