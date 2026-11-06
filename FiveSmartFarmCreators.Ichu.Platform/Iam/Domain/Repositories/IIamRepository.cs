using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;

/// <summary>Defines the I Iam Repository contract.</summary>
public interface IIamRepository
{
    Task<UserAccount?> FindAccountAsync(Guid id, CancellationToken cancellationToken);
    Task<UserAccount?> FindByExternalUidAsync(string uid, CancellationToken cancellationToken);
    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> LicenseExistsAsync(string license, Guid exceptAccountId, CancellationToken cancellationToken);
    Task<bool> RanchExistsAsync(Guid ranchId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RanchMembership>> FindMembershipsAsync(Guid accountId, CancellationToken cancellationToken);
    Task<IReadOnlyList<HerdAdvisory>> FindAdvisoriesAsync(Guid ranchId, Guid? veterinarianId, CancellationToken cancellationToken);
    Task<HerdAdvisory?> FindAdvisoryAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<HerdAdvisory>> FindVeterinarianAdvisoriesAsync(Guid accountId, CancellationToken cancellationToken);
    void AddAccessAttempt(AccessAttempt attempt);
    void AddAccount(UserAccount account);
    void AddRanch(RanchRegistration ranch);
    void AddMembership(RanchMembership membership);
    void AddAdvisory(HerdAdvisory advisory);
    Task SaveAsync(CancellationToken cancellationToken);
}
