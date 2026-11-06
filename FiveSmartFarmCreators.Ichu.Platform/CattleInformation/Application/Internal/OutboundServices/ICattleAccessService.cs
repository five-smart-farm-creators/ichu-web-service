namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>
/// Checks current Iam permissions using trusted caller context rather than request-supplied access flags.
/// </summary>
public interface ICattleAccessService
{
    /// <summary>
    /// Requires current read access before animal, lot or external profile information is loaded.
    /// </summary>
    /// <exception cref="Domain.Model.Errors.CattleInformationException">The caller is unauthenticated or lacks current ranch access.</exception>
    Task EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken);

    /// <summary>
    /// Requires current ranch management permission before a command loads or changes data.
    /// </summary>
    /// <exception cref="Domain.Model.Errors.CattleInformationException">The caller is unauthenticated or lacks current management permission.</exception>
    Task EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken);
}
