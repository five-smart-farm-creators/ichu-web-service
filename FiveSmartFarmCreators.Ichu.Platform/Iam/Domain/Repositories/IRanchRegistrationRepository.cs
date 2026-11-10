using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;

public interface IRanchRegistrationRepository
{
    Task<bool> ExistsAsync(Guid ranchId, CancellationToken cancellationToken);

    Task AddAsync(RanchRegistration ranch, CancellationToken cancellationToken);

    Task AddMembershipAsync(RanchMembership membership, CancellationToken cancellationToken);

    Task<IReadOnlyList<RanchMembership>> FindMembershipsByAccountAsync(UserAccountId accountId, CancellationToken cancellationToken);
}
