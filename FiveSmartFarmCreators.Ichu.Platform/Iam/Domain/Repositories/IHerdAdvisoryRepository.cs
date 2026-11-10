using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;

/// <summary>
///     Advisories that a command expires or grants are tracked. Read-only lookups use no tracking.
/// </summary>
public interface IHerdAdvisoryRepository
{
    Task<HerdAdvisory?> FindByIdAsync(HerdAdvisoryId advisoryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<HerdAdvisory>> FindByRanchAndVeterinarianAsync(Guid ranchId, UserAccountId veterinarianId, CancellationToken cancellationToken);

    Task<IReadOnlyList<HerdAdvisory>> FindByRanchAsNoTrackingAsync(Guid ranchId, UserAccountId? veterinarianId, CancellationToken cancellationToken);

    Task<IReadOnlyList<HerdAdvisory>> FindByVeterinarianAsNoTrackingAsync(UserAccountId veterinarianId, CancellationToken cancellationToken);

    Task AddAsync(HerdAdvisory advisory, CancellationToken cancellationToken);
}
