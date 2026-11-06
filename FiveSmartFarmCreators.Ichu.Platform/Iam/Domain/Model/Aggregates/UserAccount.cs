using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     Protects account activation, professional role and temporary access lockout.
/// </summary>
public sealed class UserAccount
{
    private UserAccount() { }

    /// <summary>
    ///     Creates a pending account for an external identity.
    /// </summary>
    /// <param name="externalUid">
    ///     The external provider's stable identity identifier.
    /// </param>
    /// <param name="email">
    ///     The account email address.
    /// </param>
    /// <param name="role">
    ///     The business role assigned to the account.
    /// </param>
    /// <param name="now">
    ///     The registration instant.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     The external identity or role is invalid.
    /// </exception>
    public UserAccount(string externalUid, Email email, UserRole role, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(externalUid) || externalUid.Length > 128 || !Enum.IsDefined(role))
        {
            throw new ArgumentException("A valid external identity and role are required.");
        }

        Id = Guid.NewGuid();
        ExternalUid = externalUid;
        Email = email;
        Role = role;
        RegisteredAt = now;
        Status = AccountStatus.PendingVerification;
        Revision = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public string ExternalUid { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public AccountStatus Status { get; private set; }
    public ProfessionalProfile? Profile { get; private set; }
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? BlockedUntil { get; private set; }
    public DateTimeOffset? LastFailedAt { get; private set; }
    public int FailedAttempts { get; private set; }
    public Guid Revision { get; private set; }

    /// <summary>
    ///     Checks verification and temporary lockout at the supplied instant.
    /// </summary>
    /// <param name="now">
    ///     The instant used to evaluate verification and blocking.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> when the account is verified and not blocked.
    /// </returns>
    public bool IsActive(DateTimeOffset now) =>
        Status != AccountStatus.PendingVerification && (BlockedUntil is null || BlockedUntil <= now);

    /// <summary>
    ///     Checks whether the temporary account block is still effective.
    /// </summary>
    /// <param name="now">
    ///     The instant used to evaluate the block expiration.
    /// </param>
    /// <returns>
    ///     <see langword="true"/> while the account remains blocked.
    /// </returns>
    public bool IsBlocked(DateTimeOffset now) => BlockedUntil > now;

    /// <summary>
    ///     Marks a pending account as active.
    /// </summary>
    public void Verify()
    {
        if (Status == AccountStatus.PendingVerification)
        {
            Status = AccountStatus.Active;
            Revision = Guid.NewGuid();
        }
    }

    /// <summary>
    ///     Records a failed attempt and applies the five-attempt lockout rule.
    /// </summary>
    /// <param name="now">
    ///     The instant of the failed attempt.
    /// </param>
    public void RecordFailedAttempt(DateTimeOffset now)
    {
        if (IsBlocked(now)) return;
        if (BlockedUntil <= now) FailedAttempts = 0;
        LastFailedAt = now;
        FailedAttempts++;
        if (FailedAttempts >= 5)
        {
            BlockedUntil = now.AddMinutes(15);
            if (Status != AccountStatus.PendingVerification) Status = AccountStatus.TemporarilyBlocked;
        }
        Revision = Guid.NewGuid();
    }

    /// <summary>
    ///     Clears failed attempts after an authorized sign-in.
    /// </summary>
    /// <param name="now">
    ///     The instant used to validate the account's access status.
    /// </param>
    /// <exception cref="UnauthorizedAccessException">
    ///     The account is unverified or temporarily blocked.
    /// </exception>
    public void RecordSuccessfulAccess(DateTimeOffset now)
    {
        if (!IsActive(now)) throw new UnauthorizedAccessException("The account cannot access the platform.");
        FailedAttempts = 0;
        BlockedUntil = null;
        Status = AccountStatus.Active;
        Revision = Guid.NewGuid();
    }

    /// <summary>
    ///     Updates professional data for a veterinarian account.
    /// </summary>
    /// <param name="profile">
    ///     The professional profile to assign to the account.
    /// </param>
    /// <exception cref="IamException">
    ///     The account does not have the veterinarian role.
    /// </exception>
    public void CompleteProfile(ProfessionalProfile profile)
    {
        if (Role != UserRole.Veterinarian) throw new IamException("forbidden", "Only veterinarians have professional profiles.");
        Profile = profile;
        Revision = Guid.NewGuid();
    }
}
