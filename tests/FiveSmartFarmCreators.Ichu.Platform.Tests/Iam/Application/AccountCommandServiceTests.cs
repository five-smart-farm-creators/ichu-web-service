using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Application;

public class AccountCommandServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);
    private const string ManagerEmail = "manager@example.com";

    private readonly FakeUserAccounts _accounts = new();
    private readonly FakeRanches _ranches = new();
    private readonly FakeIdentityProvider _identityProvider = new();
    private readonly FakeCurrentAccount _currentAccount = new();

    [Fact]
    public async Task Register_ManagerWithoutHeadCount_ReturnsRanchDetailsRequired()
    {
        _currentAccount.Identity = VerifiedIdentity();

        var result = await Service().HandleAsync(
            new RegisterAccountCommand(UserRole.RanchManager, "Los Andes", "Cusco", "Urubamba", null));

        Assert.Equal(IamErrors.RanchDetailsRequired, result.DomainError);
        Assert.Empty(_accounts.Accounts);
    }

    [Fact]
    public async Task Register_FieldOperator_ReturnsInvalidRegistration()
    {
        _currentAccount.Identity = VerifiedIdentity();

        var result = await Service().HandleAsync(
            new RegisterAccountCommand(UserRole.FieldOperator, null, null, null, null));

        Assert.Equal(IamErrors.InvalidRegistration, result.DomainError);
    }

    [Fact]
    public async Task Register_WhenTheIdentityAlreadyHasAnAccount_ReturnsAccountAlreadyExists()
    {
        _currentAccount.Identity = VerifiedIdentity();
        _accounts.Accounts.Add(VerifiedAccount());

        var result = await Service().HandleAsync(new RegisterAccountCommand(UserRole.Veterinarian, null, null, null, null));

        Assert.Equal(IamErrors.AccountAlreadyExists, result.DomainError);
    }

    [Fact]
    public async Task Register_Manager_CreatesTheAccountAndItsOwnedRanch()
    {
        _currentAccount.Identity = VerifiedIdentity();
        var unitOfWork = new CountingUnitOfWork();

        var result = await Service(unitOfWork).HandleAsync(
            new RegisterAccountCommand(UserRole.RanchManager, "Los Andes", "Cusco", "Urubamba", 120));

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value!.RanchId, Assert.Single(_ranches.Ranches).Id);
        var membership = Assert.Single(_ranches.Memberships);
        Assert.True(membership.IsOwner);
        Assert.Equal(1, unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task SignIn_WithAnInvalidEmail_ReturnsInvalidEmail()
    {
        var result = await Service().HandleAsync(new SignInCommand("not-an-email", "password"));

        Assert.Equal(IamErrors.InvalidEmail, result.DomainError);
    }

    [Fact]
    public async Task SignIn_WithABlankPassword_ReturnsPasswordRequired()
    {
        var result = await Service().HandleAsync(new SignInCommand(ManagerEmail, "   "));

        Assert.Equal(IamErrors.PasswordRequired, result.DomainError);
    }

    [Fact]
    public async Task SignIn_WithAWrongPassword_RecordsAFailedAttemptAndReturnsInvalidCredentials()
    {
        var account = VerifiedAccount();
        _accounts.Accounts.Add(account);
        _identityProvider.SignInResult = Result<string>.Failure(IamErrors.InvalidCredentials);

        var result = await Service().HandleAsync(new SignInCommand(ManagerEmail, "wrong-password"));

        Assert.Equal(IamErrors.InvalidCredentials, result.DomainError);
        Assert.Equal(1, account.FailedAttempts);
        var attempt = Assert.Single(_accounts.Attempts);
        Assert.False(attempt.Succeeded);
    }

    [Fact]
    public async Task SignIn_WhenTheProviderIsUnavailable_ReturnsIdentityProviderUnavailableWithoutRecordingAnAttempt()
    {
        _accounts.Accounts.Add(VerifiedAccount());
        _identityProvider.SignInResult = Result<string>.Failure(IamErrors.IdentityProviderUnavailable);

        var result = await Service().HandleAsync(new SignInCommand(ManagerEmail, "password"));

        Assert.Equal(IamErrors.IdentityProviderUnavailable, result.DomainError);
        Assert.Empty(_accounts.Attempts);
    }

    [Fact]
    public async Task SignIn_WhileBlocked_ReturnsInvalidCredentialsWithoutCallingTheProvider()
    {
        var account = VerifiedAccount();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            account.RecordFailedAttempt(Now);
        }

        _accounts.Accounts.Add(account);

        var result = await Service().HandleAsync(new SignInCommand(ManagerEmail, "password"));

        Assert.Equal(IamErrors.InvalidCredentials, result.DomainError);
        Assert.Equal(0, _identityProvider.SignInCalls);
    }

    private AccountCommandService Service(CountingUnitOfWork? unitOfWork = null) =>
        new(_accounts, _ranches, _identityProvider, _currentAccount, unitOfWork ?? new CountingUnitOfWork(),
            new FakeTimeProvider(Now));

    private static Result<ExternalIdentity> VerifiedIdentity() =>
        Result<ExternalIdentity>.Success(new ExternalIdentity("uid-manager", new Email(ManagerEmail), true));

    private static UserAccount VerifiedAccount()
    {
        var account = UserAccount.Create("uid-manager", new Email(ManagerEmail), UserRole.RanchManager, Now).Value!;
        account.Verify();
        return account;
    }
}
