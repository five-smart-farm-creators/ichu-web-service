namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record ReleaseCollarForInactiveCattleCommand(Guid RanchId, Guid CattleId, DateTimeOffset DeactivatedAt);
