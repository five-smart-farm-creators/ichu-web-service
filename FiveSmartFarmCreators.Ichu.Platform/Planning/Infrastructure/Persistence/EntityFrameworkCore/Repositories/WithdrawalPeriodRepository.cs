using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Stores withdrawal periods in the shared context, filtering every query by ranch.
/// </summary>
public sealed class WithdrawalPeriodRepository(AppDbContext context) : IWithdrawalPeriodRepository
{
    /// <inheritdoc />
    public async Task AddAsync(WithdrawalPeriod withdrawalPeriod, CancellationToken cancellationToken)
    {
        await context.Set<WithdrawalPeriod>().AddAsync(withdrawalPeriod, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WithdrawalPeriod>> FindByRanchAsNoTrackingAsync(
        Guid ranchId,
        Guid? cattleId,
        CancellationToken cancellationToken)
    {
        var query = context.Set<WithdrawalPeriod>()
            .AsNoTracking()
            .Where(period => period.RanchId == ranchId);

        if (cattleId is { } animalId)
        {
            query = query.Where(period => period.CattleId == animalId);
        }

        return await query
            .OrderBy(period => period.StartsAt)
            .ThenBy(period => period.Id)
            .ToListAsync(cancellationToken);
    }
}
