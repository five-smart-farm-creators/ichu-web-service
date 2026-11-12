using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Application;

public class ReproductiveHistoryQueryServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("7b2e3d1f-1e4c-4d62-8b88-2c3d4e5f6a71");
    private static readonly Guid AccountId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private static readonly DateTimeOffset Today = new(2026, 11, 12, 23, 0, 0, TimeSpan.Zero);

    private readonly FakeAccess _access = new() { AccountId = AccountId, CanRead = true };
    private readonly FakeCattle _cattle = new();
    private readonly FakeOperations _operations = new();

    [Fact]
    public async Task History_ReportsTheAgeInWholeMonthsFromTheUtcDate()
    {
        _cattle.Profile = Profile(AnimalSex.Female, new DateOnly(2024, 3, 1));

        var result = await Service(Today).Handle(new GetReproductiveHistoryQuery(RanchId, CattleId), CancellationToken.None);

        Assert.Equal(32, result.Value!.AgeInMonths);
        Assert.Equal("MilkingCow", result.Value.Animal.LifeStage);
        Assert.Equal(AnimalSex.Female, result.Value.Animal.Sex);
    }

    [Fact]
    public async Task Age_CountsOnlyCompletedMonths()
    {
        _cattle.Profile = Profile(AnimalSex.Female, new DateOnly(2026, 1, 31));
        var clock = new DateTimeOffset(2026, 2, 27, 12, 0, 0, TimeSpan.Zero);

        var result = await Service(clock).Handle(new GetReproductiveHistoryQuery(RanchId, CattleId), CancellationToken.None);

        Assert.Equal(0, result.Value!.AgeInMonths);
    }

    [Fact]
    public async Task MaleReportsNoHistoryButItsSexStageAndAge()
    {
        _cattle.Profile = Profile(AnimalSex.Male, new DateOnly(2025, 1, 1));
        _operations.Stored.Add(Operation(OperationType.Birth, new DateOnly(2025, 6, 1)));

        var result = await Service(Today).Handle(new GetReproductiveHistoryQuery(RanchId, CattleId), CancellationToken.None);

        Assert.False(result.Value!.History.HasHistory);
        Assert.Equal(AnimalSex.Male, result.Value.Animal.Sex);
        Assert.Equal(22, result.Value.AgeInMonths);
    }

    [Fact]
    public async Task UnknownAnimal_IsNotFound()
    {
        var result = await Service(Today).Handle(new GetReproductiveHistoryQuery(RanchId, CattleId), CancellationToken.None);

        Assert.Equal(OperationsMonitoringErrors.AnimalNotFound.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task CallerWithoutReadAccess_IsForbiddenBeforeTheAnimalIsLoaded()
    {
        _access.CanRead = false;
        _cattle.Profile = Profile(AnimalSex.Female, new DateOnly(2024, 3, 1));

        var result = await Service(Today).Handle(new GetReproductiveHistoryQuery(RanchId, CattleId), CancellationToken.None);

        Assert.Equal(OperationsMonitoringErrors.Forbidden.Code, result.DomainError.Code);
        Assert.False(_cattle.Asked);
    }

    [Fact]
    public async Task UnauthenticatedCaller_IsRejected()
    {
        _access.AccountId = null;

        var result = await Service(Today).Handle(new GetReproductiveHistoryQuery(RanchId, CattleId), CancellationToken.None);

        Assert.Equal(OperationsMonitoringErrors.Unauthenticated.Code, result.DomainError.Code);
    }

    private ReproductiveHistoryQueryService Service(DateTimeOffset now) =>
        new(_access, _cattle, _operations, new ReproductiveHistoryCalculator(), new FakeTimeProvider(now));

    private static CattleProfileSnapshot Profile(AnimalSex sex, DateOnly birthDate) =>
        new(CattleId, RanchId, sex, "MilkingCow", birthDate);

    private static LivestockOperation Operation(OperationType type, DateOnly day)
    {
        var at = new DateTimeOffset(day.ToDateTime(new TimeOnly(8, 0)), TimeSpan.Zero);
        return LivestockOperation.Create(Guid.NewGuid(), RanchId, CattleId, type, at, AccountId, null, null, null, at,
            ReproductiveDetails.Create(null, null, null).Value!).Value!;
    }

    private sealed class FakeAccess : IMonitoringAccessControl
    {
        public Guid? AccountId { get; set; }

        public bool CanRead { get; set; }

        public Task<Guid?> GetCurrentAccountIdAsync(CancellationToken cancellationToken) => Task.FromResult(AccountId);

        public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, MonitoringPermission permission, CancellationToken cancellationToken) =>
            Task.FromResult(CanRead);
    }

    private sealed class FakeCattle : ICattleLookup
    {
        public CattleProfileSnapshot? Profile { get; set; }

        public bool Asked { get; private set; }

        public Task<CattleSnapshot?> FindAsync(Guid cattleId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The history reads the reproductive profile only.");

        public Task<CattleProfileSnapshot?> FindProfileAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
        {
            Asked = true;
            return Task.FromResult(Profile);
        }

        public Task<bool> LotBelongsToRanchAsync(Guid lotId, Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The history never checks lots.");
    }

    private sealed class FakeOperations : ILivestockOperationRepository
    {
        public List<LivestockOperation> Stored { get; } = [];

        public Task AddAsync(LivestockOperation operation, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The history only reads operations.");

        public Task<LivestockOperation?> FindByIdAsync(LivestockOperationId operationId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The history reads the whole animal history.");

        public Task<IReadOnlyList<LivestockOperation>> FindAllByCattleAsync(Guid cattleId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<LivestockOperation>>(Stored);

        public Task<IReadOnlyList<LivestockOperation>> FindByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The history is not limited to a period.");

        public Task<IReadOnlyList<LivestockOperation>> FindByRanchAndPeriodAsNoTrackingAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("The history reads one animal.");
    }
}
