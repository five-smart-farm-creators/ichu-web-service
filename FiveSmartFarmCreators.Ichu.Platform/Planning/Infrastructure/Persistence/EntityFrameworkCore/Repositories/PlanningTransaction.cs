using System.Data;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
/// Runs a Planning operation in one serializable transaction. A serialization failure or a deadlock means that a concurrent change invalidated the check, so the caller is told to send the request again.
/// </summary>
public sealed class PlanningTransaction(AppDbContext context, IUnitOfWork unitOfWork) : IPlanningTransaction
{
    /// <inheritdoc />
    public async Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> operation, CancellationToken cancellationToken)
    {
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
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres && IsConcurrencyConflict(postgres))
        {
            return Result<T>.Failure(PlanningErrors.ConcurrentModification);
        }
        catch (PostgresException exception) when (IsConcurrencyConflict(exception))
        {
            return Result<T>.Failure(PlanningErrors.ConcurrentModification);
        }
        finally
        {
            if (!committed)
            {
                context.ChangeTracker.Clear();
            }
        }
    }

    private static bool IsConcurrencyConflict(PostgresException exception) =>
        exception.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected;
}
