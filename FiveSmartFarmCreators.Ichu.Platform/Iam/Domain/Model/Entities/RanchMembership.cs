namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;

/// <summary>Associates an account with a ranch and its owner status.</summary>
public sealed class RanchMembership
{
    private RanchMembership() { }

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
