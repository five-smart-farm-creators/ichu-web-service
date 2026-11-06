using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore;

public sealed class CattleUnitOfWork<TContext>(TContext context) : ICattleUnitOfWork where TContext : DbContext
{
    public async Task CommitAsync(Cattle? cattle, CancellationToken cancellationToken)
    {
        var events = cattle?.Events.Select(item => new CattleIntegrationEvent(item)).ToArray() ?? [];
        context.Set<CattleIntegrationEvent>().AddRange(events);
        try
        {
            // EF saves the animal, its history, lot changes and pending events in one transaction.
            await context.SaveChangesAsync(cancellationToken);
            cattle?.ClearEvents();
        }
        catch (DbUpdateConcurrencyException)
        {
            DetachEvents(events);
            throw new CattleInformationException("conflict", "The record changed. Reload it before retrying.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
               { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "uq_cattle_ranch_ear_tag" })
        {
            DetachEvents(events);
            Guid? conflictingId = null;
            if (cattle is not null)
            {
                conflictingId = await context.Set<Cattle>().AsNoTracking()
                    .Where(item => item.RanchId == cattle.RanchId && item.EarTag == cattle.EarTag)
                    .Select(item => (Guid?)item.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            throw new CattleInformationException("duplicate_ear_tag",
                "Another animal in this ranch already uses this ear tag.", conflictingId);
        }
        catch
        {
            DetachEvents(events);
            throw;
        }
    }

    private void DetachEvents(IEnumerable<CattleIntegrationEvent> events)
    {
        foreach (var item in events)
        {
            context.Entry(item).State = EntityState.Detached;
        }
    }
}
