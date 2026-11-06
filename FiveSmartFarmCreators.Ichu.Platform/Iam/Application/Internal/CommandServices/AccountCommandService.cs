using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;

/// <summary>
///     Coordinates registration and credential authentication.
/// </summary>
public sealed class AccountCommandService(IIamRepository repository, IIdentityProvider identityProvider,
    ICurrentAccount currentAccount, TimeProvider clock) : IAccountCommandService
{
    /// <inheritdoc />
    public async Task<RegisteredAccount> HandleAsync(RegisterAccountCommand command, CancellationToken cancellationToken = default)
    {
        var role = command.Role;
        RanchRegistration? ranch = role == UserRole.RanchManager
            ? new RanchRegistration(command.RanchName ?? "", command.Region ?? "", command.District ?? "",
                command.EstimatedHeadCount ?? throw new ArgumentException("An estimated head count is required.")) : null;

        var identity = currentAccount.GetExternalIdentity();
        if (role is not (UserRole.RanchManager or UserRole.Veterinarian) ||
            (role == UserRole.RanchManager) != (ranch is not null))
            throw new ArgumentException("Register a manager with a ranch or a veterinarian without a ranch.");
        if (await repository.FindByEmailAsync(identity.Email.Value, cancellationToken) is not null ||
            await repository.FindByExternalUidAsync(identity.Uid, cancellationToken) is not null)
            throw new IamException("conflict", "An account already exists for this identity.");
        var account = new UserAccount(identity.Uid, identity.Email, role, clock.GetUtcNow());
        repository.AddAccount(account);
        if (ranch is not null)
        {
            repository.AddRanch(ranch);
            repository.AddMembership(new RanchMembership(account.Id, ranch.Id, true));
        }
        await repository.SaveAsync(cancellationToken);
        return new RegisteredAccount(account, ranch?.Id);
    }

    /// <inheritdoc />
    public async Task<string> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        var address = new Email(command.Email);
        if (string.IsNullOrWhiteSpace(command.Password)) throw new ArgumentException("A password is required.");
        var account = await repository.FindByEmailAsync(address.Value, cancellationToken);
        var now = clock.GetUtcNow();
        if (account?.IsBlocked(now) == true) throw new IamException("invalid_credentials", "Access could not be granted.");
        string token;
        try
        {
            token = await identityProvider.SignInAsync(address, command.Password, cancellationToken);
        }
        catch (IamException error) when (error.Code == "invalid_credentials")
        {
            if (account is not null)
            {
                account.RecordFailedAttempt(now);
                repository.AddAccessAttempt(new AccessAttempt(account.Id, now, false));
                await repository.SaveAsync(cancellationToken);
            }
            throw;
        }
        var identity = await identityProvider.VerifyIdTokenAsync(token, cancellationToken);
        if (account is null || account.ExternalUid != identity.Uid || !identity.EmailVerified)
            throw new IamException("invalid_credentials", "Access could not be granted.");
        account.Verify();
        account.RecordSuccessfulAccess(now);
        repository.AddAccessAttempt(new AccessAttempt(account.Id, now, true));
        await repository.SaveAsync(cancellationToken);
        return token;
    }
}
