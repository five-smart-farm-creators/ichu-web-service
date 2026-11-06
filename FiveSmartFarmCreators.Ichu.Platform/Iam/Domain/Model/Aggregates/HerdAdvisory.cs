using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

/// <summary>Defines the Herd Advisory contract.</summary>
public sealed class HerdAdvisory
{
    private HerdAdvisory() { }

    public HerdAdvisory(Guid veterinarianId, Guid ranchId, string reason, DateTimeOffset now)
    {
        if (veterinarianId == Guid.Empty || ranchId == Guid.Empty || string.IsNullOrWhiteSpace(reason) || reason.Length > 1000)
        {
            throw new ArgumentException("Veterinarian, ranch and advisory reason are required.");
        }
        Id = Guid.NewGuid();
        VeterinarianId = veterinarianId;
        RanchId = ranchId;
        Reason = reason.Trim();
        RequestedAt = now;
        Revision = Guid.NewGuid();
    }

    public Guid Id { get; private set; }
    public Guid VeterinarianId { get; private set; }
    public Guid RanchId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public AdvisoryStatus Status { get; private set; }
    public AccessScope Scope { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset? GrantedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public Guid? DecidedBy { get; private set; }
    public Guid Revision { get; private set; }

    /// <summary>Executes the operation defined by this contract.</summary>
    public bool IsValidOn(DateTimeOffset now) => Status == AdvisoryStatus.Granted && GrantedAt <= now && ExpiresAt > now;

    /// <summary>Executes the operation defined by this contract.</summary>
    public void Grant(Guid actorId, AccessScope scope, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        if (Status != AdvisoryStatus.Pending) throw new IamException("conflict", "Only pending advisories can be granted.");
        if (actorId == Guid.Empty || scope is not (AccessScope.Read or AccessScope.ClinicalWrite) || expiresAt <= now)
        {
            throw new ArgumentException("A deciding account, permitted scope and future expiration are required.");
        }
        Status = AdvisoryStatus.Granted;
        Scope = scope;
        GrantedAt = now;
        ExpiresAt = expiresAt;
        DecidedBy = actorId;
        Revision = Guid.NewGuid();
    }

    /// <summary>Executes the operation defined by this contract.</summary>
    public void End(Guid actorId, bool reject, DateTimeOffset now)
    {
        if (actorId == Guid.Empty || (reject ? Status != AdvisoryStatus.Pending : !IsValidOn(now)))
        {
            throw new IamException("conflict", "The advisory cannot transition from its current state.");
        }
        Status = reject ? AdvisoryStatus.Rejected : AdvisoryStatus.Revoked;
        EndedAt = now;
        DecidedBy = actorId;
        Revision = Guid.NewGuid();
    }

    /// <summary>Executes the operation defined by this contract.</summary>
    public void Expire(DateTimeOffset now)
    {
        if (Status != AdvisoryStatus.Granted || ExpiresAt > now) return;
        Status = AdvisoryStatus.Expired;
        EndedAt = ExpiresAt;
        Revision = Guid.NewGuid();
    }
}
