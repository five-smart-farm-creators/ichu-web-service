using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using System.Security.Claims;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Pipeline.Middleware.Services;

/// <summary>
///     Resolves the request identity without trusting client-supplied account identifiers.
/// </summary>
public sealed class HttpCurrentAccount(IHttpContextAccessor accessor, IIamRepository repository, TimeProvider clock) : ICurrentAccount
{
    /// <inheritdoc />
    public ExternalIdentity GetExternalIdentity()
    {
        var principal = accessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true ||
            principal.FindFirstValue("firebase_uid") is not { Length: > 0 } uid ||
            principal.FindFirstValue(ClaimTypes.Email) is not { Length: > 0 } email)
            throw new IamException("unauthorized", "A verified identity is required.");
        return new ExternalIdentity(uid, new Email(email), principal.FindFirstValue("email_verified") == "true");
    }

    /// <inheritdoc />
    public async Task<UserAccount> GetAsync(CancellationToken cancellationToken)
    {
        var identity = GetExternalIdentity();
        var account = await repository.FindByExternalUidAsync(identity.Uid, cancellationToken);
        if (account is null || !identity.EmailVerified || account.IsBlocked(clock.GetUtcNow()))
            throw new IamException("unauthorized", "An active verified account is required.");
        account.Verify();
        if (!account.IsActive(clock.GetUtcNow())) throw new IamException("unauthorized", "The account is not active.");
        await repository.SaveAsync(cancellationToken);
        return account;
    }
}
