using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;

/// <summary>
/// Projects composed profile data to a REST response while preserving integration availability.
/// </summary>
public static class CattleProfileResourceFromModelAssembler
{
    /// <summary>
    /// Keeps device availability distinct from a null collar assignment in the profile response.
    /// </summary>
    public static CattleProfileResource ToResourceFromModel(CattleProfile profile)
    {
        var cattle = CattleResourceFromEntityAssembler.ToResourceFromEntity(profile.Cattle);
        var device = new CattleDeviceResource(
            profile.Device.IsAvailable, profile.Device.CollarId, profile.Device.LastReadingReceivedAt);
        return new CattleProfileResource(cattle, device);
    }
}
