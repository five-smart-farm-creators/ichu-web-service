using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Stores herd advisories in the shared context. It is not derived from the base repository, whose lookups take Guid keys.
/// </summary>
public sealed class HerdAdvisoryRepository(AppDbContext context) : IHerdAdvisoryRepository
{
    /// <inheritdoc />
    public Task<HerdAdvisory?> FindByIdAsync(HerdAdvisoryId advisoryId, CancellationToken cancellationToken) =>
        context.Set<HerdAdvisory>()
            .FirstOrDefaultAsync(advisory => advisory.Id == advisoryId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<HerdAdvisory>> FindByRanchAndVeterinarianAsync(Guid ranchId, UserAccountId veterinarianId,
        CancellationToken cancellationToken) =>
        await context.Set<HerdAdvisory>()
            .Where(advisory => advisory.RanchId == ranchId && advisory.VeterinarianId == veterinarianId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<HerdAdvisory>> FindByRanchAsNoTrackingAsync(Guid ranchId, UserAccountId? veterinarianId,
        CancellationToken cancellationToken)
    {
        var query = context.Set<HerdAdvisory>()
            .AsNoTracking()
            .Where(advisory => advisory.RanchId == ranchId);

        if (veterinarianId is { } veterinarian)
        {
            query = query.Where(advisory => advisory.VeterinarianId == veterinarian);
        }

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<HerdAdvisory>> FindByVeterinarianAsNoTrackingAsync(UserAccountId veterinarianId,
        CancellationToken cancellationToken) =>
        await context.Set<HerdAdvisory>()
            .AsNoTracking()
            .Where(advisory => advisory.VeterinarianId == veterinarianId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(HerdAdvisory advisory, CancellationToken cancellationToken)
    {
        await context.Set<HerdAdvisory>().AddAsync(advisory, cancellationToken);
    }
}
