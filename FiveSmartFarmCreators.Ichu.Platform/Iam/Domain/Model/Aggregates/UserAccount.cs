using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     An account activates only through a request whose email is verified, and repeated failed sign-ins lock it temporarily.
/// </summary>
public sealed class UserAccount : AggregateRoot
{
    private const int MaximumExternalUidLength = 128;
    private const int MaximumFailedAttempts = 5;
    private const int LockoutMinutes = 15;

    // Entity Framework Core materializes accounts without running the factory.
    private UserAccount()
    {
    }

    public UserAccountId Id { get; private set; }

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
    ///     New accounts stay pending verification until a request with a verified identity activates them.
    /// </summary>
    public static Result<UserAccount> Create(string externalUid, Email email, UserRole role, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(externalUid) || externalUid.Length > MaximumExternalUidLength || !Enum.IsDefined(role))
        {
            return Result<UserAccount>.Failure(IamErrors.InvalidExternalIdentity);
        }

        return Result<UserAccount>.Success(new UserAccount
        {
            Id = new UserAccountId(Guid.NewGuid()),
            ExternalUid = externalUid,
            Email = email,
            Role = role,
            RegisteredAt = now,
            Status = AccountStatus.PendingVerification,
            Revision = Guid.NewGuid()
        });
    }

    public bool IsActive(DateTimeOffset now) =>
        Status != AccountStatus.PendingVerification && (BlockedUntil is null || BlockedUntil <= now);

    public bool IsBlocked(DateTimeOffset now) => BlockedUntil > now;

    public void Verify()
    {
        if (Status == AccountStatus.PendingVerification)
        {
            Status = AccountStatus.Active;
            Revision = Guid.NewGuid();
        }
    }

    /// <summary>
    ///     Five failed attempts block the account for fifteen minutes.
    /// </summary>
    public void RecordFailedAttempt(DateTimeOffset now)
    {
        if (IsBlocked(now))
        {
            return;
        }

        if (BlockedUntil <= now)
        {
            FailedAttempts = 0;
        }

        LastFailedAt = now;
        FailedAttempts++;
        if (FailedAttempts >= MaximumFailedAttempts)
        {
            BlockedUntil = now.AddMinutes(LockoutMinutes);
            if (Status != AccountStatus.PendingVerification)
            {
                Status = AccountStatus.TemporarilyBlocked;
            }
        }

        Revision = Guid.NewGuid();
    }

    /// <summary>
    ///     Clears the failure history only for an account that may use the platform.
    /// </summary>
    public Result RecordSuccessfulAccess(DateTimeOffset now)
    {
        if (!IsActive(now))
        {
            return Result.Failure(IamErrors.AccountNotActive);
        }

        FailedAttempts = 0;
        BlockedUntil = null;
        Status = AccountStatus.Active;
        Revision = Guid.NewGuid();
        return Result.Success();
    }

    /// <summary>
    ///     Only veterinarians declare professional details.
    /// </summary>
    public Result CompleteProfile(ProfessionalProfile profile)
    {
        if (Role != UserRole.Veterinarian)
        {
            return Result.Failure(IamErrors.ProfileOnlyForVeterinarians);
        }

        Profile = profile;
        Revision = Guid.NewGuid();
        return Result.Success();
    }
}
