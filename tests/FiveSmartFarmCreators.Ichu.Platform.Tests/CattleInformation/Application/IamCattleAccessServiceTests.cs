using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.CattleInformation.Application;

public class IamCattleAccessServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid AccountId = Guid.Parse("44444444-4444-4444-8444-444444444444");

    [Fact]
    public async Task EnsureReadAccess_WithoutAVerifiedAccountIsUnauthenticated()
    {
        var service = new IamCattleAccessService(new FakeIamContextFacade { CurrentAccount = null });

        var result = await service.EnsureReadAccessAsync(RanchId, CancellationToken.None);

        Assert.Equal(CattleInformationErrors.Unauthenticated.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task EnsureManageAccess_WithoutTheManageScopeIsRefused()
    {
        var iam = new FakeIamContextFacade { CurrentAccount = AccountId, Grants = [AccessScope.Read] };
        var service = new IamCattleAccessService(iam);

        var result = await service.EnsureManageAccessAsync(RanchId, CancellationToken.None);

        Assert.Equal(CattleInformationErrors.RanchAccessDenied.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task EnsureReadAccess_AsksIamForTheReadScopeOfTheCallerAccount()
    {
        var iam = new FakeIamContextFacade { CurrentAccount = AccountId, Grants = [AccessScope.Read] };
        var service = new IamCattleAccessService(iam);

        var result = await service.EnsureReadAccessAsync(RanchId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((AccountId, RanchId, AccessScope.Read), Assert.Single(iam.Checks));
    }

    private sealed class FakeIamContextFacade : IIamContextFacade
    {
        public Guid? CurrentAccount { get; init; }

        public IReadOnlyCollection<AccessScope> Grants { get; init; } = [];

        public List<(Guid AccountId, Guid RanchId, AccessScope Scope)> Checks { get; } = [];

        public Task<IReadOnlyList<RanchAccess>> GetAccessibleRanchesAsync(Guid accountId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<RanchAccess>>([]);

        public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, AccessScope scope, CancellationToken cancellationToken)
        {
            Checks.Add((accountId, ranchId, scope));
            return Task.FromResult(Grants.Contains(scope));
        }

        public Task<Guid?> FetchCurrentAccountIdAsync(CancellationToken cancellationToken) => Task.FromResult(CurrentAccount);
    }
}
