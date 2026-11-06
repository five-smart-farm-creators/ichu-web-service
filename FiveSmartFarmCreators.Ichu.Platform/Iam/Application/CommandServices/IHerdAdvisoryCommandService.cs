using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

/// <summary>Defines the HerdAdvisoryCommandService application contract.</summary>
public interface IHerdAdvisoryCommandService
{
    /// <summary>Requests advisory access for the current veterinarian.</summary>
    Task<HerdAdvisory> HandleAsync(RequestHerdAdvisoryCommand command, CancellationToken cancellationToken = default);

    /// <summary>Applies an advisory decision by the ranch owner.</summary>
    Task<HerdAdvisory> HandleAsync(DecideHerdAdvisoryCommand command, CancellationToken cancellationToken = default);

    /// <summary>Grants owner-approved advisory access without requiring a prior request.</summary>
    Task<HerdAdvisory> HandleAsync(GrantHerdAdvisoryCommand command, CancellationToken cancellationToken = default);
}
