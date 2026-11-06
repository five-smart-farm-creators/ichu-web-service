using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>Reads current collar assignment and its last received reading without copying external state.</summary>
public interface ICattleProfileIntegrationService
{
    Task<CattleDeviceDetails> GetAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);
}
