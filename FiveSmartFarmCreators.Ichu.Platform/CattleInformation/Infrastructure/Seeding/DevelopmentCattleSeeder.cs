using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Seeding;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Seeding;

/// <summary>
///     Creates the Development lot and its two animals when they are absent, so that the Development collars and their
///     readings have a herd to belong to. Records that already exist are skipped.
/// </summary>
public sealed class DevelopmentCattleSeeder(
    ICattleRepository cattles,
    ILotRepository lots,
    AppDbContext context,
    IUnitOfWork unitOfWork,
    TimeProvider clock)
{
    private static readonly DevelopmentAnimal[] Animals =
    [
        new(DevelopmentSeed.MilkingCowId, "PE-0001", Breed.Fleckvieh, LifeStage.MilkingCow, new DateOnly(2022, 3, 15)),
        new(DevelopmentSeed.HeiferId, "PE-0002", Breed.BrownSwiss, LifeStage.Heifer, new DateOnly(2025, 6, 1))
    ];

    /// <summary>
    ///     Skips the access check because no HTTP account exists at startup.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var lot = await lots.FindByIdAsync(DevelopmentSeed.RanchId, new LotId(DevelopmentSeed.LotId), cancellationToken);
        if (lot is null)
        {
            var creation = Lot.Create(new CreateLotCommand(DevelopmentSeed.RanchId, "Development lot", null));
            if (creation.IsFailure)
            {
                throw new InvalidOperationException(creation.DomainError.Message);
            }

            lot = creation.Value!;
            // Development-only: the edge's seed references these fixed identifiers.
            context.Entry(lot).Property(entry => entry.Id).CurrentValue = new LotId(DevelopmentSeed.LotId);
            await lots.AddAsync(lot, cancellationToken);
        }

        var today = DateOnly.FromDateTime(now.UtcDateTime);
        foreach (var animal in Animals)
        {
            await AddAnimalAsync(lot, animal, today, now, cancellationToken);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task AddAnimalAsync(Lot lot, DevelopmentAnimal animal, DateOnly today, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (await cattles.FindByIdAsNoTrackingAsync(new CattleId(animal.Id), cancellationToken) is not null)
        {
            return;
        }

        var registration = Cattle.Register(
            new RegisterCattleCommand(DevelopmentSeed.RanchId, animal.EarTag, animal.Breed, Sex.Female, animal.BirthDate, animal.Stage),
            today);
        if (registration.IsFailure)
        {
            throw new InvalidOperationException(registration.DomainError.Message);
        }

        var cattle = registration.Value!;
        context.Entry(cattle).Property(entry => entry.Id).CurrentValue = new CattleId(animal.Id);
        await cattles.AddAsync(cattle, cancellationToken);

        var membership = lot.AddCattle(cattle, lot.Version, now);
        if (membership.IsFailure)
        {
            throw new InvalidOperationException(membership.DomainError.Message);
        }
    }

    private sealed record DevelopmentAnimal(Guid Id, string EarTag, Breed Breed, LifeStage Stage, DateOnly BirthDate);
}
