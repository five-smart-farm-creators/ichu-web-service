namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>
/// Transfers the birth and exit dates of an animal to other contexts, which place readings within the animal's herd period.
/// </summary>
public sealed record CattleLifecycleSnapshot(Guid CattleId, Guid RanchId, DateOnly BirthDate, DateOnly? ExitDate);
