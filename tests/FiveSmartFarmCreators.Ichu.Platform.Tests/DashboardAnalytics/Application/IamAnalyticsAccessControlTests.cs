using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Application;

public class IamAnalyticsAccessControlTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid AccountId = Guid.Parse("44444444-4444-4444-8444-444444444444");

    [Fact]
    public async Task CanReadRanch_AsksIamForTheReadScopeOfTheAccount()
    {
        var iam = new FakeIamContextFacade { Grants = [AccessScope.Read] };

        var allowed = await new IamAnalyticsAccessControl(iam).CanReadRanchAsync(AccountId, RanchId, CancellationToken.None);

        Assert.True(allowed);
        Assert.Equal((AccountId, RanchId, AccessScope.Read), Assert.Single(iam.Checks));
    }

    [Fact]
    public async Task GetAccessibleRanchIds_ReturnsTheRanchesThatIamGrants()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var iam = new FakeIamContextFacade
        {
            Ranches =
            [
                new RanchAccess(first, AccessScope.Manage, null),
                new RanchAccess(second, AccessScope.Read, new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero))
            ]
        };

        var ranchIds = await new IamAnalyticsAccessControl(iam).GetAccessibleRanchIdsAsync(AccountId, CancellationToken.None);

        Assert.Equal(new[] { first, second }, ranchIds);
    }

    [Fact]
    public async Task GetCurrentAccountId_ReturnsNullWhenTheRequestHasNoVerifiedAccount()
    {
        var accountId = await new IamAnalyticsAccessControl(new FakeIamContextFacade()).GetCurrentAccountIdAsync(CancellationToken.None);

        Assert.Null(accountId);
    }

    private sealed class FakeIamContextFacade : IIamContextFacade
    {
        public IReadOnlyCollection<AccessScope> Grants { get; init; } = [];

        public IReadOnlyList<RanchAccess> Ranches { get; init; } = [];

        public List<(Guid AccountId, Guid RanchId, AccessScope Scope)> Checks { get; } = [];

        public Task<IReadOnlyList<RanchAccess>> GetAccessibleRanchesAsync(Guid accountId, CancellationToken cancellationToken) =>
            Task.FromResult(Ranches);

        public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, AccessScope scope, CancellationToken cancellationToken)
        {
            Checks.Add((accountId, ranchId, scope));
            return Task.FromResult(Grants.Contains(scope));
        }

        public Task<Guid?> FetchCurrentAccountIdAsync(CancellationToken cancellationToken) => Task.FromResult<Guid?>(null);
    }
}
