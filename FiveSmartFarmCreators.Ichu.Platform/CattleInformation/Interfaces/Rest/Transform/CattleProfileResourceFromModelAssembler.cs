using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;

public static class CattleProfileResourceFromModelAssembler
{
    public static CattleProfileResource ToResourceFromModel(CattleProfile profile)
    {
        var cattle = CattleResourceFromEntityAssembler.ToResourceFromEntity(profile.Cattle);
        var device = new CattleDeviceResource(
            profile.Device.IsAvailable, profile.Device.CollarId, profile.Device.LastReadingReceivedAt);
        return new CattleProfileResource(cattle, device);
    }
}
