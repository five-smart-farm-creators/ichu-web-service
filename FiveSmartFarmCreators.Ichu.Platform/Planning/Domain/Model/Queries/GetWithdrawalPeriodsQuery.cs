namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;

public sealed record GetWithdrawalPeriodsQuery(Guid RanchId, Guid? CattleId);
