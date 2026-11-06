using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

/// <summary>
/// Composes local animal information with external device availability without transferring ownership of either.
/// </summary>
public sealed record CattleProfile(Cattle Cattle, CattleDeviceDetails Device);
