using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;

/// <summary>
/// Reads withdrawal periods after checking the caller's read permission and the animal reference.
/// </summary>
public interface IWithdrawalPeriodQueryService
{
    Task<Result<IReadOnlyList<WithdrawalPeriod>>> HandleAsync(GetWithdrawalPeriodsQuery query, CancellationToken cancellationToken);
}
