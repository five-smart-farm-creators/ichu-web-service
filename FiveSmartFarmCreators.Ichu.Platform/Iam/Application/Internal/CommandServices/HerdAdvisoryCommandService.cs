using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;

/// <summary>
///     Coordinates advisory requests and owner decisions. Expired grants are saved before a request is refused, so
///     the stored state stays current.
/// </summary>
public sealed class HerdAdvisoryCommandService(
    IRanchRegistrationRepository ranches,
    IUserAccountRepository accounts,
    IHerdAdvisoryRepository advisories,
    ICurrentAccount currentAccount,
    AuthorizationDomainService authorization,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : IHerdAdvisoryCommandService
{
    private const string DirectGrantReason = "Direct authorization by the ranch owner.";

    /// <inheritdoc />
    public async Task<Result<HerdAdvisory>> HandleAsync(RequestHerdAdvisoryCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        if (actor.IsFailure)
        {
            return Result<HerdAdvisory>.Failure(actor.DomainError);
        }

        var veterinarian = actor.Value!;
        if (veterinarian.Role != UserRole.Veterinarian || veterinarian.Profile is null)
        {
            return Result<HerdAdvisory>.Failure(IamErrors.VeterinarianProfileRequired);
        }

        if (!await ranches.ExistsAsync(command.RanchId, cancellationToken))
        {
            return Result<HerdAdvisory>.Failure(IamErrors.RanchNotFound);
        }

        var now = clock.GetUtcNow();
        var previous = await advisories.FindByRanchAndVeterinarianAsync(command.RanchId, veterinarian.Id, cancellationToken);
        foreach (var advisory in previous)
        {
            advisory.Expire(now);
        }

        if (previous.Any(advisory => advisory.Status == AdvisoryStatus.Pending || advisory.IsValidOn(now)))
        {
            return Result<HerdAdvisory>.Failure(IamErrors.AdvisoryRequestAlreadyOpen);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        var request = HerdAdvisory.Create(veterinarian.Id, command.RanchId, command.Reason, now);
        if (request.IsFailure)
        {
            return request;
        }

        await advisories.AddAsync(request.Value!, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return request;
    }

    /// <inheritdoc />
    public async Task<Result<HerdAdvisory>> HandleAsync(DecideHerdAdvisoryCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        if (actor.IsFailure)
        {
            return Result<HerdAdvisory>.Failure(actor.DomainError);
        }

        var owner = actor.Value!;
        var now = clock.GetUtcNow();
        var memberships = await ranches.FindMembershipsByAccountAsync(owner.Id, cancellationToken);
        if (!authorization.CanAccessRanch(owner, command.RanchId, AccessScope.Manage, memberships, [], now))
        {
            return Result<HerdAdvisory>.Failure(IamErrors.OwnerRequired);
        }

        // The ranch is checked before the lookup, and an advisory of another ranch is reported as missing, so identifiers cannot be probed.
        var advisory = await advisories.FindByIdAsync(new HerdAdvisoryId(command.AdvisoryId), cancellationToken);
        if (advisory is null || advisory.RanchId != command.RanchId)
        {
            return Result<HerdAdvisory>.Failure(IamErrors.AdvisoryNotFound);
        }

        var veterinarian = await accounts.FindByIdAsNoTrackingAsync(advisory.VeterinarianId, cancellationToken);
        var outcome = command.Decision switch
        {
            "grant" => GrantPending(advisory, owner, veterinarian, command, now),
            "reject" => advisory.End(owner.Id, reject: true, now),
            "revoke" => advisory.End(owner.Id, reject: false, now),
            _ => Result.Failure(IamErrors.InvalidDecision)
        };
        if (outcome.IsFailure)
        {
            return Result<HerdAdvisory>.Failure(outcome.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<HerdAdvisory>.Success(advisory);
    }

    /// <inheritdoc />
    public async Task<Result<HerdAdvisory>> HandleAsync(GrantHerdAdvisoryCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        if (actor.IsFailure)
        {
            return Result<HerdAdvisory>.Failure(actor.DomainError);
        }

        var owner = actor.Value!;
        var now = clock.GetUtcNow();
        var memberships = await ranches.FindMembershipsByAccountAsync(owner.Id, cancellationToken);
        if (!authorization.CanAccessRanch(owner, command.RanchId, AccessScope.Manage, memberships, [], now))
        {
            return Result<HerdAdvisory>.Failure(IamErrors.OwnerRequired);
        }

        var veterinarianId = new UserAccountId(command.VeterinarianId);
        var veterinarian = await accounts.FindByIdAsNoTrackingAsync(veterinarianId, cancellationToken);
        if (veterinarian is null || veterinarian.Role != UserRole.Veterinarian || !veterinarian.IsActive(now) ||
            veterinarian.Profile is null)
        {
            return Result<HerdAdvisory>.Failure(IamErrors.VeterinarianNotEligible);
        }

        var entries = await advisories.FindByRanchAndVeterinarianAsync(command.RanchId, veterinarianId, cancellationToken);
        foreach (var entry in entries)
        {
            entry.Expire(now);
        }

        if (entries.Any(entry => entry.IsValidOn(now)))
        {
            return Result<HerdAdvisory>.Failure(IamErrors.AdvisoryAlreadyActive);
        }

        var advisory = entries.FirstOrDefault(entry => entry.Status == AdvisoryStatus.Pending);
        await unitOfWork.CompleteAsync(cancellationToken);
        if (advisory is null)
        {
            var created = HerdAdvisory.Create(veterinarianId, command.RanchId, DirectGrantReason, now);
            if (created.IsFailure)
            {
                return created;
            }

            advisory = created.Value!;
            await advisories.AddAsync(advisory, cancellationToken);
        }

        var grant = advisory.Grant(owner.Id, command.Scope, command.ExpiresAt, now);
        if (grant.IsFailure)
        {
            return Result<HerdAdvisory>.Failure(grant.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<HerdAdvisory>.Success(advisory);
    }

    private static Result GrantPending(HerdAdvisory advisory, UserAccount owner, UserAccount? veterinarian,
        DecideHerdAdvisoryCommand command, DateTimeOffset now)
    {
        if (veterinarian is null || !veterinarian.IsActive(now) || veterinarian.Profile is null)
        {
            return Result.Failure(IamErrors.VeterinarianNotEligible);
        }

        if (command.ExpiresAt is not { } expiresAt)
        {
            return Result.Failure(IamErrors.ExpirationRequired);
        }

        return advisory.Grant(owner.Id, command.Scope, expiresAt, now);
    }
}
