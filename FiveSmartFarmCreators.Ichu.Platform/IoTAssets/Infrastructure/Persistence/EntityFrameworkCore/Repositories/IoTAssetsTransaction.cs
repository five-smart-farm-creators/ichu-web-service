using System.Data;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class IoTAssetsTransaction(AppDbContext context, IUnitOfWork unitOfWork) : IIoTAssetsTransaction
{
    public async Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction is not null || context.ChangeTracker.HasChanges())
        {
            return Result<T>.Failure(IoTAssetsErrors.IntegrationUnavailable);
        }

        var committed = false;
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var result = await operation(cancellationToken);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result;
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            committed = true;
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<T>.Failure(IoTAssetsErrors.Conflict);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres && IsKnownConflict(postgres))
        {
            return Result<T>.Failure(ToError((PostgresException)exception.InnerException!));
        }
        catch (PostgresException exception) when (IsKnownConflict(exception))
        {
            return Result<T>.Failure(ToError(exception));
        }
        finally
        {
            if (!committed)
            {
                context.ChangeTracker.Clear();
            }
        }
    }

    private static bool IsKnownConflict(PostgresException exception) =>
        exception.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected
        || exception.SqlState == PostgresErrorCodes.UniqueViolation
            && exception.ConstraintName is "ux_iot_devices_serial" or "ux_iot_assignments_open_device" or "ux_iot_assignments_open_target";

    private static Error ToError(PostgresException exception) => exception.ConstraintName switch
    {
        "ux_iot_devices_serial" => IoTAssetsErrors.DuplicateSerial,
        "ux_iot_assignments_open_device" => IoTAssetsErrors.DeviceAlreadyAssigned,
        "ux_iot_assignments_open_target" => IoTAssetsErrors.TargetAlreadyAssigned,
        _ => IoTAssetsErrors.Conflict
    };
}
