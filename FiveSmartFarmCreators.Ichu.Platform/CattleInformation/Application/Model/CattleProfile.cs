using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Model;

public sealed record CattleProfile(Cattle Cattle, CattleDeviceDetails Device);
