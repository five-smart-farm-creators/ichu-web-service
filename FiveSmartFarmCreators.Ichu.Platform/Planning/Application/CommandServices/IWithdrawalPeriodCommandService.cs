using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;

/// <summary>
/// Coordinates withdrawal period changes after checking clinical permission on the ranch and the animal reference.
/// </summary>
public interface IWithdrawalPeriodCommandService
{
    Task<Result<WithdrawalPeriod>> HandleAsync(RecordWithdrawalPeriodCommand command, CancellationToken cancellationToken);
}
