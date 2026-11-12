using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;

/// <summary>
///     Derives the breeding history of a female. A confirmed pregnancy is a Pregnant diagnosis that follows a breeding service after the last calving or abortion, so rechecks of one gestation count once.
/// </summary>
public sealed class ReproductiveHistoryCalculator
{
    /// <summary>
    ///     Expects the operations in occurrence order. Only a female can have a history, so the operations of a male are ignored.
    /// </summary>
    public ReproductiveHistory Calculate(AnimalSex sex, IReadOnlyList<LivestockOperation> operations)
    {
        IReadOnlyList<LivestockOperation> chronological = sex == AnimalSex.Female ? operations : [];
        var services = new List<LivestockOperation>();
        var diagnoses = new List<LivestockOperation>();
        var calvings = new List<LivestockOperation>();
        var abortions = new List<LivestockOperation>();
        var heats = new List<LivestockOperation>();
        // The conception service of each confirmed pregnancy, with the calving that the pregnancy follows.
        var pregnancies = new List<(LivestockOperation? Service, LivestockOperation? PreviousCalving)>();
        var confirmedServices = new HashSet<LivestockOperationId>();
        LivestockOperation? latestService = null;
        LivestockOperation? latestCalving = null;

        foreach (var operation in chronological)
        {
            switch (operation.Type)
            {
                case OperationType.Insemination or OperationType.NaturalMating:
                    services.Add(operation);
                    latestService = operation;
                    break;
                case OperationType.PregnancyDiagnosis:
                    diagnoses.Add(operation);
                    // A recheck repeats the diagnosis of the same service, so the service is counted once.
                    if (operation.Reproductive.PregnancyResult == PregnancyResult.Pregnant
                        && latestService is not null
                        && confirmedServices.Add(latestService.Id))
                    {
                        pregnancies.Add((latestService, latestCalving));
                    }

                    break;
                case OperationType.Birth:
                    calvings.Add(operation);
                    latestCalving = operation;
                    latestService = null;
                    break;
                case OperationType.Abortion:
                    // An abortion ends the gestation, so a later service starts a new conception window.
                    abortions.Add(operation);
                    latestService = null;
                    break;
                case OperationType.HeatObserved:
                    heats.Add(operation);
                    break;
            }
        }

        var intervals = calvings.Select(calving => ToInterval(calving, pregnancies)).ToList();
        var servicesPerPregnancy = pregnancies.Count == 0 ? (double?)null : (double)services.Count / pregnancies.Count;
        return new ReproductiveHistory(services, diagnoses, calvings, abortions, heats, intervals, pregnancies.Count, servicesPerPregnancy);
    }

    private static ReproductiveInterval ToInterval(
        LivestockOperation calving,
        IReadOnlyList<(LivestockOperation? Service, LivestockOperation? PreviousCalving)> pregnancies)
    {
        // The latest conception before the next calving closes the open period, so a pregnancy lost to an abortion gives way to the one that followed it.
        var conception = pregnancies.LastOrDefault(pregnancy => pregnancy.PreviousCalving?.Id == calving.Id).Service;
        return conception is null
            ? new ReproductiveInterval(calving.OccurredAt, null, null)
            : new ReproductiveInterval(calving.OccurredAt, conception.OccurredAt, DaysBetween(calving.OccurredAt, conception.OccurredAt));
    }

    private static int DaysBetween(DateTimeOffset from, DateTimeOffset to) =>
        DateOnly.FromDateTime(to.UtcDateTime).DayNumber - DateOnly.FromDateTime(from.UtcDateTime).DayNumber;
}
