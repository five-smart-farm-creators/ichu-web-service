namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;

/// <summary>
/// Links an animal to a lot in the same ranch and records the assignment instant in UTC.
/// </summary>
public sealed class LotMembership
{
    private LotMembership()
    {
    }

    internal LotMembership(Guid lotId, Guid ranchId, Guid cattleId, DateTimeOffset assignedAt)
    {
        LotId = lotId;
        RanchId = ranchId;
        CattleId = cattleId;
        AssignedAt = assignedAt.ToUniversalTime();
    }

    public Guid LotId { get; private set; }
    public Guid RanchId { get; private set; }
    public Guid CattleId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
}
