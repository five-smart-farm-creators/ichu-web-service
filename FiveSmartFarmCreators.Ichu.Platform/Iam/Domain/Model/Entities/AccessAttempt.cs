namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

/// <summary>Records an account access attempt without retaining credentials.</summary>
public sealed class AccessAttempt
{
    private AccessAttempt() { }

    public AccessAttempt(Guid accountId, DateTimeOffset recordedAt, bool succeeded)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("An account identifier is required.");
        Id = Guid.NewGuid();
        UserAccountId = accountId;
        RecordedAt = recordedAt;
        Succeeded = succeeded;
    }

    public Guid Id { get; private set; }
    public Guid UserAccountId { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public bool Succeeded { get; private set; }

}
