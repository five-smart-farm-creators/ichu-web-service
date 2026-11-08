using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.CattleInformation.Application;

public class CattleCommandServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly DateOnly BirthDate = new(2026, 1, 1);

    private readonly FakeCattleRepository _cattle = new();
    private readonly FakeAccessService _access = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly CattleCommandService _service;

    public CattleCommandServiceTests()
    {
        _service = new CattleCommandService(_cattle, _unitOfWork, _access,
            new FakeTimeProvider(new DateTimeOffset(2026, 11, 7, 10, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public async Task Register_SavesTheNewAnimal()
    {
        var result = await _service.HandleAsync(Register("A-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(_cattle.Items);
        Assert.Equal(1, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Register_WithAnEarTagHeldByAnotherAnimalReturnsItsIdentifierAndSavesNothing()
    {
        var holder = Cattle.Register(Register("A-1"), BirthDate).Value!;
        _cattle.Items.Add(holder);

        var result = await _service.HandleAsync(Register("A-1"), CancellationToken.None);

        Assert.True(result.IsFailure);
        var duplicate = Assert.IsType<DuplicateEarTagError>(result.DomainError);
        Assert.Equal(holder.Id, duplicate.ConflictingCattleId);
        Assert.Equal(CattleInformationErrors.DuplicateEarTag.Code, duplicate.Code);
        Assert.Single(_cattle.Items);
        Assert.Equal(0, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Update_KeepingTheSameEarTagOnTheSameAnimalIsAllowed()
    {
        var animal = Cattle.Register(Register("A-1"), BirthDate).Value!;
        _cattle.Items.Add(animal);

        var result = await _service.HandleAsync(
            new UpdateCattleCommand(RanchId, animal.Id, "A-1", Breed.BrownSwiss, Sex.Female, BirthDate, animal.Version),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Register_WithoutRanchPermissionSavesNothing()
    {
        _access.Outcome = Result.Failure(CattleInformationErrors.RanchAccessDenied);

        var result = await _service.HandleAsync(Register("A-1"), CancellationToken.None);

        Assert.Equal(CattleInformationErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Empty(_cattle.Items);
        Assert.Equal(0, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task ChangeStage_OfAnUnknownAnimalIsNotFound()
    {
        var result = await _service.HandleAsync(
            new ChangeLifeStageCommand(RanchId, new CattleId(Guid.NewGuid()), LifeStage.Heifer, BirthDate, Guid.NewGuid()),
            CancellationToken.None);

        Assert.Equal(CattleInformationErrors.CattleNotFound.Code, result.DomainError.Code);
    }

    private static RegisterCattleCommand Register(string earTag) =>
        new(RanchId, earTag, Breed.BrownSwiss, Sex.Female, BirthDate, LifeStage.Calf);

    private sealed class FakeAccessService : ICattleAccessService
    {
        public Result Outcome { get; set; } = Result.Success();

        public Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Outcome);

        public Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Outcome);
    }

    private sealed class FakeCattleRepository : ICattleRepository
    {
        public List<Cattle> Items { get; } = [];

        public Task<Cattle?> FindByIdAsync(Guid ranchId, CattleId cattleId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(cattle => cattle.RanchId == ranchId && cattle.Id == cattleId));

        public Task<Cattle?> FindByIdAsNoTrackingAsync(CattleId cattleId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.SingleOrDefault(cattle => cattle.Id == cattleId));

        public Task<CattleId?> FindIdByEarTagAsync(Guid ranchId, EarTag earTag, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Where(cattle => cattle.RanchId == ranchId && cattle.EarTag == earTag)
                .Select(cattle => (CattleId?)cattle.Id)
                .FirstOrDefault());

        public Task<IReadOnlyList<Cattle>> SearchAsync(SearchCattleQuery query, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Cattle>>(Items);

        public Task AddAsync(Cattle cattle, CancellationToken cancellationToken = default)
        {
            Items.Add(cattle);
            return Task.CompletedTask;
        }
    }

    private sealed class CountingUnitOfWork : IUnitOfWork
    {
        public int CompletedCount { get; private set; }

        public Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            CompletedCount++;
            return Task.CompletedTask;
        }
    }
}
