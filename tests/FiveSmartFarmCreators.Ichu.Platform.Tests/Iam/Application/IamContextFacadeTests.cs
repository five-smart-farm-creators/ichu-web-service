using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Application;

public class IamContextFacadeTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task FetchCurrentAccountId_WhenTheAccountIsNotActive_ReturnsNull()
    {
        var currentAccount = new FakeCurrentAccount { Account = Result<UserAccount>.Failure(IamErrors.AccountNotActive) };

        var accountId = await new IamContextFacade(new NoAuthorization(), currentAccount)
            .FetchCurrentAccountIdAsync(CancellationToken.None);

        Assert.Null(accountId);
    }

    [Fact]
    public async Task FetchCurrentAccountId_ForAVerifiedAccount_ReturnsItsIdentifier()
    {
        var account = UserAccount.Create("uid-vet", new Email("vet@example.com"), UserRole.Veterinarian, Now).Value!;
        account.Verify();
        var currentAccount = new FakeCurrentAccount { Account = Result<UserAccount>.Success(account) };

        var accountId = await new IamContextFacade(new NoAuthorization(), currentAccount)
            .FetchCurrentAccountIdAsync(CancellationToken.None);

        Assert.Equal(account.Id.Value, accountId);
    }
}
