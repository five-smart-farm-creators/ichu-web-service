using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Application;

public class AccountQueryServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetCurrentRanches_ExposesTheAccessCodeOnlyForTheManagedRanch()
    {
        var managed = RanchWithCode("ABCDEFGH");
        var shared = RanchWithCode("JKLMNPQR");
        var ranches = new FakeRanches();
        ranches.Ranches.Add(managed);
        ranches.Ranches.Add(shared);
        var service = Service(ranches, new FixedAuthorization(
            new RanchAccess(managed.Id, AccessScope.Manage, null),
            new RanchAccess(shared.Id, AccessScope.Read, Now.AddDays(1))));

        var result = await service.HandleAsync(new GetCurrentRanchesQuery());

        Assert.True(result.IsSuccess);
        var managedEntry = result.Value!.Single(entry => entry.Access.RanchId == managed.Id);
        Assert.Equal("ABCDEFGH", managedEntry.AccessCode!.Value);
        var sharedEntry = result.Value!.Single(entry => entry.Access.RanchId == shared.Id);
        Assert.Equal(shared.Name, sharedEntry.Name);
        Assert.Null(sharedEntry.AccessCode);
    }

    private static AccountQueryService Service(FakeRanches ranches, IAuthorizationQueryService authorization)
    {
        var manager = UserAccount.Create("uid-manager", new Email("manager@example.com"), UserRole.RanchManager, Now).Value!;
        var currentAccount = new FakeCurrentAccount { Account = Result<UserAccount>.Success(manager) };
        return new AccountQueryService(currentAccount, authorization, ranches);
    }

    private static RanchRegistration RanchWithCode(string code) =>
        RanchRegistration.Create("Los Andes", "Cusco", "Urubamba", 120, new RanchAccessCode(code)).Value!;
}
