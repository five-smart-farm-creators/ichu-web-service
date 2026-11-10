using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

public interface IHerdAdvisoryCommandService
{
    Task<Result<HerdAdvisory>> HandleAsync(RequestHerdAdvisoryCommand command, CancellationToken cancellationToken = default);

    Task<Result<HerdAdvisory>> HandleAsync(DecideHerdAdvisoryCommand command, CancellationToken cancellationToken = default);

    Task<Result<HerdAdvisory>> HandleAsync(GrantHerdAdvisoryCommand command, CancellationToken cancellationToken = default);
}
