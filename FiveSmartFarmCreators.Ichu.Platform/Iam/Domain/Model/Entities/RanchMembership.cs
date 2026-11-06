namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

/// <summary>
///     Associates an account with a ranch and records whether the account is its owner.
/// </summary>
public sealed class RanchMembership
{
    private RanchMembership() { }

    /// <summary>
    ///     Creates a membership between an account and a ranch.
    /// </summary>
    /// <param name="userAccountId">
    ///     The account joining the ranch.
    /// </param>
    /// <param name="ranchId">
    ///     The ranch associated with the account.
    /// </param>
    /// <param name="isOwner">
    ///     <see langword="true"/> when the account owns the ranch.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Either identifier is empty.
    /// </exception>
    public RanchMembership(Guid userAccountId, Guid ranchId, bool isOwner)
    {
        if (userAccountId == Guid.Empty || ranchId == Guid.Empty) throw new ArgumentException("Account and ranch identifiers are required.");
        Id = Guid.NewGuid();
        UserAccountId = userAccountId;
        RanchId = ranchId;
        IsOwner = isOwner;
    }

    public Guid Id { get; private set; }
    public Guid UserAccountId { get; private set; }
    public Guid RanchId { get; private set; }
    public bool IsOwner { get; private set; }

}
