using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>
/// Reads IoT assignment and monitoring reception data through their owning contexts without storing copies.
/// </summary>
public interface ICattleProfileIntegrationService
{
    /// <summary>
    /// Retrieves device information for an animal whose ranch access has already been checked.
    /// </summary>
    /// <returns>Explicit availability; unavailable integration data must not be reported as an unassigned collar.</returns>
    Task<CattleDeviceDetails> GetAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);
}
