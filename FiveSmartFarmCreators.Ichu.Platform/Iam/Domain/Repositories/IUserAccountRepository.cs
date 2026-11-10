using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;

/// <summary>
///     Lookups that a command may change are tracked. Read-only lookups use no tracking.
/// </summary>
public interface IUserAccountRepository
{
    Task<UserAccount?> FindByIdAsNoTrackingAsync(UserAccountId accountId, CancellationToken cancellationToken);

    Task<UserAccount?> FindByExternalUidAsync(string externalUid, CancellationToken cancellationToken);

    Task<UserAccount?> FindByEmailAsync(Email email, CancellationToken cancellationToken);

    Task<bool> LicenseExistsAsync(string licenseNumber, UserAccountId exceptAccountId, CancellationToken cancellationToken);

    Task AddAsync(UserAccount account, CancellationToken cancellationToken);

    Task AddAccessAttemptAsync(AccessAttempt attempt, CancellationToken cancellationToken);
}
