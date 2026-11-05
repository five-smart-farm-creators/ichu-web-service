using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence;

public sealed class SubscriptionPlansUnitOfWork(DbContext context) : ISubscriptionPlansUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw SubscriptionException.Conflict("The subscription changed concurrently. Reload it before retrying.");
        }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw SubscriptionException.Conflict("A conflicting subscription or operation already exists.");
        }
    }
}
