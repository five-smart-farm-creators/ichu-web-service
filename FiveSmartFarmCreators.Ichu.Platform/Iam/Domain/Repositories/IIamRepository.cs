using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;

/// <summary>
///     Defines persistence operations needed by IAM use cases.
/// </summary>
public interface IIamRepository
{
    /// <summary>
    ///     Finds an account by its identifier.
    /// </summary>
    /// <param name="id">
    ///     The account identifier.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the account lookup.
    /// </param>
    /// <returns>
    ///     The account, or <see langword="null"/> when it does not exist.
    /// </returns>
    Task<UserAccount?> FindAccountAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    ///     Finds an account by its external identity-provider identifier.
    /// </summary>
    /// <param name="uid">
    ///     The provider's stable user identifier.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the account lookup.
    /// </param>
    /// <returns>
    ///     The account, or <see langword="null"/> when it does not exist.
    /// </returns>
    Task<UserAccount?> FindByExternalUidAsync(string uid, CancellationToken cancellationToken);

    /// <summary>
    ///     Finds an account by its normalized email address.
    /// </summary>
    /// <param name="email">
    ///     The normalized email address.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the account lookup.
    /// </param>
    /// <returns>
    ///     The account, or <see langword="null"/> when it does not exist.
    /// </returns>
    Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks whether a professional license is assigned to another account.
    /// </summary>
    /// <param name="license">
    ///     The normalized professional license number.
    /// </param>
    /// <param name="exceptAccountId">
    ///     The account excluded from the uniqueness check.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the license lookup.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> when another account uses the license.
    /// </returns>
    Task<bool> LicenseExistsAsync(string license, Guid exceptAccountId, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks whether a ranch registration exists.
    /// </summary>
    /// <param name="ranchId">
    ///     The ranch identifier.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the ranch lookup.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> when the ranch exists.
    /// </returns>
    Task<bool> RanchExistsAsync(Guid ranchId, CancellationToken cancellationToken);

    /// <summary>
    ///     Lists ranch memberships belonging to an account.
    /// </summary>
    /// <param name="accountId">
    ///     The account identifier.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the membership lookup.
    /// </param>
    Task<IReadOnlyList<RanchMembership>> FindMembershipsAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    ///     Lists advisory requests for a ranch, optionally filtered by veterinarian.
    /// </summary>
    /// <param name="ranchId">
    ///     The ranch identifier.
    /// </param>
    /// <param name="veterinarianId">
    ///     The optional veterinarian identifier.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the advisory lookup.
    /// </param>
    Task<IReadOnlyList<HerdAdvisory>> FindAdvisoriesAsync(Guid ranchId, Guid? veterinarianId, CancellationToken cancellationToken);

    /// <summary>
    ///     Finds an advisory request by its identifier.
    /// </summary>
    /// <param name="id">
    ///     The advisory identifier.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the advisory lookup.
    /// </param>
    /// <returns>
    ///     The advisory, or <see langword="null"/> when it does not exist.
    /// </returns>
    Task<HerdAdvisory?> FindAdvisoryAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    ///     Lists advisory requests submitted by a veterinarian.
    /// </summary>
    /// <param name="accountId">
    ///     The veterinarian account identifier.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels the advisory lookup.
    /// </param>
    Task<IReadOnlyList<HerdAdvisory>> FindVeterinarianAdvisoriesAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    ///     Adds an account access attempt to the unit of work.
    /// </summary>
    void AddAccessAttempt(AccessAttempt attempt);

    /// <summary>
    ///     Adds an account to the unit of work.
    /// </summary>
    void AddAccount(UserAccount account);

    /// <summary>
    ///     Adds a ranch registration to the unit of work.
    /// </summary>
    void AddRanch(RanchRegistration ranch);

    /// <summary>
    ///     Adds an account-to-ranch membership to the unit of work.
    /// </summary>
    void AddMembership(RanchMembership membership);

    /// <summary>
    ///     Adds a veterinarian advisory to the unit of work.
    /// </summary>
    void AddAdvisory(HerdAdvisory advisory);

    /// <summary>
    ///     Persists changes tracked by the repository.
    /// </summary>
    Task SaveAsync(CancellationToken cancellationToken);
}
