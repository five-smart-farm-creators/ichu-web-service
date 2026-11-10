using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     A request is granted, rejected, revoked or expired, and never returns to pending once decided.
/// </summary>
public sealed class HerdAdvisory : AggregateRoot
{
    private const int MaximumReasonLength = 1000;

    // Entity Framework Core materializes advisories without running the factory.
    private HerdAdvisory()
    {
    }

    public HerdAdvisoryId Id { get; private set; }

    public UserAccountId VeterinarianId { get; private set; }

    public Guid RanchId { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public AdvisoryStatus Status { get; private set; }

    public AccessScope Scope { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? GrantedAt { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public UserAccountId? DecidedBy { get; private set; }

    public Guid Revision { get; private set; }

    public static Result<HerdAdvisory> Create(UserAccountId veterinarianId, Guid ranchId, string reason, DateTimeOffset now)
    {
        if (veterinarianId.Value == Guid.Empty || ranchId == Guid.Empty ||
            string.IsNullOrWhiteSpace(reason) || reason.Length > MaximumReasonLength)
        {
            return Result<HerdAdvisory>.Failure(IamErrors.InvalidAdvisory);
        }

        return Result<HerdAdvisory>.Success(new HerdAdvisory
        {
            Id = new HerdAdvisoryId(Guid.NewGuid()),
            VeterinarianId = veterinarianId,
            RanchId = ranchId,
            Reason = reason.Trim(),
            Status = AdvisoryStatus.Pending,
            RequestedAt = now,
            Revision = Guid.NewGuid()
        });
    }

    public bool IsValidOn(DateTimeOffset now) =>
        Status == AdvisoryStatus.Granted && GrantedAt <= now && ExpiresAt > now;

    /// <summary>
    ///     Only a pending request can be granted, and a grant must expire in the future.
    /// </summary>
    public Result Grant(UserAccountId actorId, AccessScope scope, DateTimeOffset expiresAt, DateTimeOffset now)
    {
        if (Status != AdvisoryStatus.Pending)
        {
            return Result.Failure(IamErrors.AdvisoryNotPending);
        }

        if (actorId.Value == Guid.Empty)
        {
            return Result.Failure(IamErrors.InvalidIdentifier);
        }

        if (scope is not (AccessScope.Read or AccessScope.ClinicalWrite))
        {
            return Result.Failure(IamErrors.InvalidAdvisoryScope);
        }

        if (expiresAt <= now)
        {
            return Result.Failure(IamErrors.ExpirationNotInFuture);
        }

        Status = AdvisoryStatus.Granted;
        Scope = scope;
        GrantedAt = now;
        ExpiresAt = expiresAt;
        DecidedBy = actorId;
        Revision = Guid.NewGuid();
        return Result.Success();
    }

    /// <summary>
    ///     A pending request can be rejected, and only an effective grant can be revoked.
    /// </summary>
    public Result End(UserAccountId actorId, bool reject, DateTimeOffset now)
    {
        if (actorId.Value == Guid.Empty || (reject ? Status != AdvisoryStatus.Pending : !IsValidOn(now)))
        {
            return Result.Failure(IamErrors.AdvisoryTransitionInvalid);
        }

        Status = reject ? AdvisoryStatus.Rejected : AdvisoryStatus.Revoked;
        EndedAt = now;
        DecidedBy = actorId;
        Revision = Guid.NewGuid();
        return Result.Success();
    }

    /// <summary>
    ///     A grant that has reached its expiration instant becomes expired.
    /// </summary>
    public void Expire(DateTimeOffset now)
    {
        if (Status != AdvisoryStatus.Granted || ExpiresAt > now)
        {
            return;
        }

        Status = AdvisoryStatus.Expired;
        EndedAt = ExpiresAt;
        Revision = Guid.NewGuid();
    }
}
