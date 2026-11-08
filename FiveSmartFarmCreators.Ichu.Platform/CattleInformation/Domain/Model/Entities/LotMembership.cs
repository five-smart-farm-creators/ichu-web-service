using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;

/// <summary>
///     Links an animal to a lot of the same ranch and keeps the assignment instant in UTC.
/// </summary>
public sealed class LotMembership
{
    // Entity Framework Core materializes memberships without running the constructor.
    private LotMembership()
    {
    }

    internal LotMembership(LotId lotId, Guid ranchId, CattleId cattleId, DateTimeOffset assignedAt)
    {
        LotId = lotId;
        RanchId = ranchId;
        CattleId = cattleId;
        AssignedAt = assignedAt.ToUniversalTime();
    }

    public LotId LotId { get; private set; }

    public Guid RanchId { get; private set; }

    public CattleId CattleId { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }
}
