using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Application command service that implements <see cref="IGeofenceCommandService"/> to define and redefine the
///     geofences of a ranch.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="cattleLookup">The anti-corruption port towards Cattle Information.</param>
/// <param name="geofences">The geofence repository.</param>
/// <param name="ruleRevisionService">The domain service that advances the rule revision.</param>
/// <param name="unitOfWork">The unit of work used to commit changes.</param>
/// <param name="logger">Logger for diagnostics and error reporting.</param>
public class GeofenceCommandService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    IGeofenceRepository geofences,
    IMonitoringRuleRevisionService ruleRevisionService,
    IUnitOfWork unitOfWork,
    ILogger<GeofenceCommandService> logger) : IGeofenceCommandService
{
    /// <inheritdoc />
    public async Task<Result<Geofence>> Handle(DefineGeofenceCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.Unauthenticated, OperationsMonitoringError.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, command.RanchId, MonitoringPermission.ManageRules, cancellationToken))
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.Forbidden, OperationsMonitoringError.Forbidden);
        }

        if (!await cattleLookup.LotBelongsToRanchAsync(command.LotId, command.RanchId, cancellationToken))
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.LotNotFound, OperationsMonitoringError.LotNotFound);
        }

        if (await geofences.FindByLotAsync(command.RanchId, command.LotId, cancellationToken) is not null)
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.GeofenceAlreadyDefined, OperationsMonitoringError.GeofenceAlreadyDefined);
        }

        var revision = await ruleRevisionService.AdvanceAsync(command.RanchId, cancellationToken);
        var geofence = new Geofence(command.RanchId, command.LotId, command.Name, command.ToleranceMeters,
            command.Severity, command.Boundary, revision);
        await geofences.AddAsync(geofence, cancellationToken);

        try
        {
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Geofence>.Success(geofence);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "The changes could not be saved.");
            return Result<Geofence>.Failure(OperationsMonitoringErrors.ConcurrentModification, OperationsMonitoringError.ConcurrentModification);
        }
    }

    /// <inheritdoc />
    public async Task<Result<Geofence>> Handle(RedefineGeofenceCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.Unauthenticated, OperationsMonitoringError.Unauthenticated);
        }

        var geofence = await geofences.FindByIdAsync(command.GeofenceId, cancellationToken);
        if (geofence is null)
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.GeofenceNotFound, OperationsMonitoringError.GeofenceNotFound);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, geofence.RanchId, MonitoringPermission.ManageRules, cancellationToken))
        {
            return Result<Geofence>.Failure(OperationsMonitoringErrors.Forbidden, OperationsMonitoringError.Forbidden);
        }

        var revision = await ruleRevisionService.AdvanceAsync(geofence.RanchId, cancellationToken);
        geofence.Redefine(command.Name, command.ToleranceMeters, command.Severity, command.Boundary, revision);

        try
        {
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<Geofence>.Success(geofence);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "The changes could not be saved.");
            return Result<Geofence>.Failure(OperationsMonitoringErrors.ConcurrentModification, OperationsMonitoringError.ConcurrentModification);
        }
    }
}
