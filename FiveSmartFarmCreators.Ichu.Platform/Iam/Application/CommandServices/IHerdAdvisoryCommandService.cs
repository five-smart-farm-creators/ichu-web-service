using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

/// <summary>
///     Handles veterinarian advisory requests and ranch-owner decisions.
/// </summary>
public interface IHerdAdvisoryCommandService
{
    /// <summary>
    ///     Requests advisory access for the current veterinarian.
    /// </summary>
    /// <returns>
    ///     The newly created advisory request.
    /// </returns>
    Task<HerdAdvisory> HandleAsync(RequestHerdAdvisoryCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Applies an advisory decision by the ranch owner.
    /// </summary>
    /// <returns>
    ///     The advisory after the owner decision.
    /// </returns>
    Task<HerdAdvisory> HandleAsync(DecideHerdAdvisoryCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Grants owner-approved advisory access without requiring a prior request.
    /// </summary>
    /// <returns>
    ///     The newly granted advisory.
    /// </returns>
    Task<HerdAdvisory> HandleAsync(GrantHerdAdvisoryCommand command, CancellationToken cancellationToken = default);
}
