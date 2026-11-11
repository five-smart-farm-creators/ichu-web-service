using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;

/// <summary>
///     Coordinates registration and credential authentication. A rejected password is recorded before its failure is
///     returned, so that repeated attempts lock the account.
/// </summary>
public sealed class AccountCommandService(
    IUserAccountRepository accounts,
    IRanchRegistrationRepository ranches,
    IRanchAccessCodeGenerator accessCodes,
    IIdentityProvider identityProvider,
    ICurrentAccount currentAccount,
    IUnitOfWork unitOfWork,
    TimeProvider clock) : IAccountCommandService
{
    // Among 32^8 codes a collision is rare, so several colliding draws in a row point to a fault rather than bad luck.
    private const int MaximumAccessCodeAttempts = 5;

    /// <inheritdoc />
    public async Task<Result<RegisteredAccount>> HandleAsync(RegisterAccountCommand command, CancellationToken cancellationToken = default)
    {
        var role = command.Role;
        RanchRegistration? ranch = null;
        if (role == UserRole.RanchManager)
        {
            var accessCode = await NextUniqueAccessCodeAsync(cancellationToken);
            if (accessCode.IsFailure)
            {
                return Result<RegisteredAccount>.Failure(accessCode.DomainError);
            }

            var registration = CreateRanch(command, accessCode.Value!);
            if (registration.IsFailure)
            {
                return Result<RegisteredAccount>.Failure(registration.DomainError);
            }

            ranch = registration.Value;
        }

        var identity = currentAccount.GetExternalIdentity();
        if (identity.IsFailure)
        {
            return Result<RegisteredAccount>.Failure(identity.DomainError);
        }

        if (role is not (UserRole.RanchManager or UserRole.Veterinarian) || (role == UserRole.RanchManager) != (ranch is not null))
        {
            return Result<RegisteredAccount>.Failure(IamErrors.InvalidRegistration);
        }

        var verified = identity.Value!;
        if (await accounts.FindByEmailAsync(verified.Email, cancellationToken) is not null ||
            await accounts.FindByExternalUidAsync(verified.Uid, cancellationToken) is not null)
        {
            return Result<RegisteredAccount>.Failure(IamErrors.AccountAlreadyExists);
        }

        var creation = UserAccount.Create(verified.Uid, verified.Email, role, clock.GetUtcNow());
        if (creation.IsFailure)
        {
            return Result<RegisteredAccount>.Failure(creation.DomainError);
        }

        var account = creation.Value!;
        await accounts.AddAsync(account, cancellationToken);
        if (ranch is not null)
        {
            var ownership = RanchMembership.Create(account.Id, ranch.Id, isOwner: true);
            if (ownership.IsFailure)
            {
                return Result<RegisteredAccount>.Failure(ownership.DomainError);
            }

            await ranches.AddAsync(ranch, cancellationToken);
            await ranches.AddMembershipAsync(ownership.Value!, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<RegisteredAccount>.Success(new RegisteredAccount(account, ranch?.Id));
    }

    /// <inheritdoc />
    public async Task<Result<string>> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        var address = Email.Create(command.Email);
        if (address.IsFailure)
        {
            return Result<string>.Failure(address.DomainError);
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            return Result<string>.Failure(IamErrors.PasswordRequired);
        }

        var email = address.Value!;
        var account = await accounts.FindByEmailAsync(email, cancellationToken);
        var now = clock.GetUtcNow();
        if (account?.IsBlocked(now) == true)
        {
            return Result<string>.Failure(IamErrors.InvalidCredentials);
        }

        var signIn = await identityProvider.SignInAsync(email, command.Password, cancellationToken);
        if (signIn.IsFailure)
        {
            if (signIn.DomainError == IamErrors.InvalidCredentials && account is not null)
            {
                account.RecordFailedAttempt(now);
                var failedAttempt = AccessAttempt.Create(account.Id, now, succeeded: false);
                if (failedAttempt.IsFailure)
                {
                    return Result<string>.Failure(failedAttempt.DomainError);
                }

                await accounts.AddAccessAttemptAsync(failedAttempt.Value!, cancellationToken);
                await unitOfWork.CompleteAsync(cancellationToken);
            }

            return Result<string>.Failure(signIn.DomainError);
        }

        var token = signIn.Value!;
        var identity = await identityProvider.VerifyIdTokenAsync(token, cancellationToken);
        if (identity.IsFailure)
        {
            return Result<string>.Failure(identity.DomainError);
        }

        var verified = identity.Value!;
        if (account is null || account.ExternalUid != verified.Uid || !verified.EmailVerified)
        {
            return Result<string>.Failure(IamErrors.InvalidCredentials);
        }

        account.Verify();
        var access = account.RecordSuccessfulAccess(now);
        if (access.IsFailure)
        {
            return Result<string>.Failure(access.DomainError);
        }

        var successfulAttempt = AccessAttempt.Create(account.Id, now, succeeded: true);
        if (successfulAttempt.IsFailure)
        {
            return Result<string>.Failure(successfulAttempt.DomainError);
        }

        await accounts.AddAccessAttemptAsync(successfulAttempt.Value!, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<string>.Success(token);
    }

    private async Task<Result<RanchAccessCode>> NextUniqueAccessCodeAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaximumAccessCodeAttempts; attempt++)
        {
            var candidate = accessCodes.Next();
            if (!await ranches.ExistsByAccessCodeAsync(candidate, cancellationToken))
            {
                return Result<RanchAccessCode>.Success(candidate);
            }
        }

        return Result<RanchAccessCode>.Failure(IamErrors.RanchAccessCodeUnavailable);
    }

    private static Result<RanchRegistration> CreateRanch(RegisterAccountCommand command, RanchAccessCode accessCode) =>
        command.EstimatedHeadCount is { } headCount
            ? RanchRegistration.Create(command.RanchName ?? string.Empty, command.Region ?? string.Empty,
                command.District ?? string.Empty, headCount, accessCode)
            : Result<RanchRegistration>.Failure(IamErrors.RanchDetailsRequired);
}
