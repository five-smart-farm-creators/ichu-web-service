using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;

public interface IWithdrawalPeriodRepository
{
    Task AddAsync(WithdrawalPeriod withdrawalPeriod, CancellationToken cancellationToken);

    Task<IReadOnlyList<WithdrawalPeriod>> FindByRanchAsNoTrackingAsync(Guid ranchId, Guid? cattleId, CancellationToken cancellationToken);
}
