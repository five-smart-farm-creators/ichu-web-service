using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

/// <summary>Defines the User Account contract.</summary>
public sealed class UserAccount
{
    private UserAccount() { }

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

    /// <summary>Executes the operation defined by this contract.</summary>
    public bool IsActive(DateTimeOffset now) =>
        Status != AccountStatus.PendingVerification && (BlockedUntil is null || BlockedUntil <= now);

    /// <summary>Executes the operation defined by this contract.</summary>
    public bool IsBlocked(DateTimeOffset now) => BlockedUntil > now;

    /// <summary>Executes the operation defined by this contract.</summary>
    public void Verify()
    {
        if (Status == AccountStatus.PendingVerification)
        {
            Status = AccountStatus.Active;
            Revision = Guid.NewGuid();
        }
    }

    /// <summary>Executes the operation defined by this contract.</summary>
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

    /// <summary>Executes the operation defined by this contract.</summary>
    public void RecordSuccessfulAccess(DateTimeOffset now)
    {
        if (!IsActive(now)) throw new UnauthorizedAccessException("The account cannot access the platform.");
        FailedAttempts = 0;
        BlockedUntil = null;
        Status = AccountStatus.Active;
        Revision = Guid.NewGuid();
    }

    /// <summary>Executes the operation defined by this contract.</summary>
    public void CompleteProfile(ProfessionalProfile profile)
    {
        if (Role != UserRole.Veterinarian) throw new IamException("forbidden", "Only veterinarians have professional profiles.");
        Profile = profile;
        Revision = Guid.NewGuid();
    }
}
