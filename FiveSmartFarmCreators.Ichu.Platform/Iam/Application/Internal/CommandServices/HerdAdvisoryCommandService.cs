using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;

/// <summary>Coordinates advisory requests and owner decisions.</summary>
public sealed class HerdAdvisoryCommandService(IIamRepository repository, ICurrentAccount currentAccount,
    AuthorizationDomainService authorization, TimeProvider clock) : IHerdAdvisoryCommandService
{
    /// <inheritdoc />
    public async Task<HerdAdvisory> HandleAsync(RequestHerdAdvisoryCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        if (actor.Role != UserRole.Veterinarian || actor.Profile is null)
            throw new IamException("forbidden", "A complete veterinarian profile is required.");
        if (!await repository.RanchExistsAsync(command.RanchId, cancellationToken))
            throw new IamException("not_found", "The ranch was not found.");
        var now = clock.GetUtcNow();
        var previous = await repository.FindAdvisoriesAsync(command.RanchId, actor.Id, cancellationToken);
        foreach (var advisory in previous) advisory.Expire(now);
        if (previous.Any(a => a.Status == AdvisoryStatus.Pending || a.IsValidOn(now)))
            throw new IamException("conflict", "A pending request or active advisory already exists.");
        await repository.SaveAsync(cancellationToken);
        var request = new HerdAdvisory(actor.Id, command.RanchId, command.Reason, now);
        repository.AddAdvisory(request);
        await repository.SaveAsync(cancellationToken);
        return request;
    }

    /// <inheritdoc />
    public async Task<HerdAdvisory> HandleAsync(DecideHerdAdvisoryCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        var advisory = await repository.FindAdvisoryAsync(command.AdvisoryId, cancellationToken)
            ?? throw new IamException("not_found", "The advisory was not found.");
        var memberships = await repository.FindMembershipsAsync(actor.Id, cancellationToken);
        if (!authorization.CanAccessRanch(actor, advisory.RanchId, AccessScope.Manage, memberships, [], clock.GetUtcNow()))
            throw new IamException("forbidden", "Only the ranch owner may decide this advisory.");
        var vet = await repository.FindAccountAsync(advisory.VeterinarianId, cancellationToken);
        switch (command.Decision)
        {
            case "grant":
                if (vet is null || !vet.IsActive(clock.GetUtcNow()) || vet.Profile is null)
                    throw new IamException("conflict", "An active veterinarian with a complete profile is required.");
                advisory.Grant(actor.Id, command.Scope, command.ExpiresAt ?? throw new ArgumentException("An expiration is required."), clock.GetUtcNow());
                break;
            case "reject": advisory.End(actor.Id, true, clock.GetUtcNow()); break;
            case "revoke": advisory.End(actor.Id, false, clock.GetUtcNow()); break;
            default: throw new ArgumentException("Decision must be grant, reject or revoke.");
        }
        await repository.SaveAsync(cancellationToken);
        return advisory;
    }

    /// <inheritdoc />
    public async Task<HerdAdvisory> HandleAsync(GrantHerdAdvisoryCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var memberships = await repository.FindMembershipsAsync(actor.Id, cancellationToken);
        if (!authorization.CanAccessRanch(actor, command.RanchId, AccessScope.Manage, memberships, [], now))
            throw new IamException("forbidden", "Only the ranch owner may grant an advisory.");
        var vet = await repository.FindAccountAsync(command.VeterinarianId, cancellationToken);
        if (vet is null || vet.Role != UserRole.Veterinarian || !vet.IsActive(now) || vet.Profile is null)
            throw new IamException("conflict", "An active veterinarian with a complete profile is required.");
        var entries = await repository.FindAdvisoriesAsync(command.RanchId, command.VeterinarianId, cancellationToken);
        foreach (var entry in entries) entry.Expire(now);
        if (entries.Any(a => a.IsValidOn(now))) throw new IamException("conflict", "An active advisory already exists.");
        var advisory = entries.FirstOrDefault(a => a.Status == AdvisoryStatus.Pending);
        await repository.SaveAsync(cancellationToken);
        if (advisory is null)
        {
            advisory = new HerdAdvisory(command.VeterinarianId, command.RanchId, "Direct authorization by the ranch owner.", now);
            repository.AddAdvisory(advisory);
        }
        advisory.Grant(actor.Id, command.Scope, command.ExpiresAt, now);
        await repository.SaveAsync(cancellationToken);
        return advisory;
    }
}
