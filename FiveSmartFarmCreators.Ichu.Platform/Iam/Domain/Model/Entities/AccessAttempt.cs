namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

/// <summary>
///     Records whether an account access attempt succeeded without storing credentials.
/// </summary>
public sealed class AccessAttempt
{
    private AccessAttempt() { }

    /// <summary>
    ///     Creates an access attempt for an account.
    /// </summary>
    /// <param name="accountId">
    ///     The account associated with the attempt.
    /// </param>
    /// <param name="recordedAt">
    ///     The time the attempt was recorded.
    /// </param>
    /// <param name="succeeded">
    ///     <see langword="true"/> when authentication succeeded; otherwise, <see langword="false"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     <paramref name="accountId"/> is empty.
    /// </exception>
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
