using System.Security.Claims;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Pipeline.Middleware.Services;

/// <summary>
///     Resolves the request identity without trusting client-supplied account identifiers.
/// </summary>
public sealed class HttpCurrentAccount(
    IHttpContextAccessor accessor,
    IUserAccountRepository accounts,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : ICurrentAccount
{
    public Result<ExternalIdentity> GetExternalIdentity()
    {
        var principal = accessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true ||
            principal.FindFirstValue("firebase_uid") is not { Length: > 0 } uid ||
            principal.FindFirstValue(ClaimTypes.Email) is not { Length: > 0 } email)
        {
            return Result<ExternalIdentity>.Failure(IamErrors.Unauthenticated);
        }

        var address = Email.Create(email);
        if (address.IsFailure)
        {
            return Result<ExternalIdentity>.Failure(IamErrors.Unauthenticated);
        }

        return Result<ExternalIdentity>.Success(
            new ExternalIdentity(uid, address.Value!, principal.FindFirstValue("email_verified") == "true"));
    }

    /// <inheritdoc />
    public async Task<Result<UserAccount>> GetAsync(CancellationToken cancellationToken)
    {
        var identity = GetExternalIdentity();
        if (identity.IsFailure)
        {
            return Result<UserAccount>.Failure(identity.DomainError);
        }

        var verified = identity.Value!;
        var now = clock.GetUtcNow();
        var account = await accounts.FindByExternalUidAsync(verified.Uid, cancellationToken);
        if (account is null || !verified.EmailVerified || account.IsBlocked(now))
        {
            return Result<UserAccount>.Failure(IamErrors.AccountNotActive);
        }

        account.Verify();
        if (!account.IsActive(now))
        {
            return Result<UserAccount>.Failure(IamErrors.AccountNotActive);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<UserAccount>.Success(account);
    }
}
