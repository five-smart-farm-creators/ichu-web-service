using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

/// <summary>
/// The ration that a lot receives over an inclusive date range. Its open end, when there is no ValidTo, is the plan in force until a later plan takes over.
/// </summary>
public sealed class NutritionPlan : AggregateRoot
{
    public const int MaximumNameLength = 80;
    public const int MaximumNotesLength = 500;
    public const decimal MaximumTargetDailyGainKilograms = 3m;

    private readonly List<RationItem> _items = [];

    // Entity Framework Core materializes plans without running the factory.
    private NutritionPlan()
    {
    }

    public NutritionPlanId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public Guid LotId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateOnly ValidFrom { get; private set; }

    /// <summary>
    /// Both dates are inclusive. A null end keeps the plan in force after its start.
    /// </summary>
    public DateOnly? ValidTo { get; private set; }

    public IReadOnlyCollection<RationItem> Items => _items.AsReadOnly();

    public decimal? TargetDailyGainKilograms { get; private set; }

    public string? Notes { get; private set; }

    public bool IsOpenEnded => ValidTo is null;

    public static Result<NutritionPlan> Create(RegisterNutritionPlanCommand command)
    {
        if (command.RanchId == Guid.Empty || command.LotId == Guid.Empty)
        {
            return Result<NutritionPlan>.Failure(PlanningErrors.InvalidIdentifier);
        }

        var name = command.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return Result<NutritionPlan>.Failure(PlanningErrors.NutritionPlanNameRequired);
        }

        if (name.Length > MaximumNameLength)
        {
            return Result<NutritionPlan>.Failure(PlanningErrors.NutritionPlanNameTooLong);
        }

        if (command.ValidFrom is not { } validFrom)
        {
            return Result<NutritionPlan>.Failure(PlanningErrors.NutritionPlanValidFromRequired);
        }

        if (command.ValidTo is { } validTo && validTo < validFrom)
        {
            return Result<NutritionPlan>.Failure(PlanningErrors.NutritionPlanValidityInvalid);
        }

        if (command.TargetDailyGainKilograms is { } gain && (gain <= 0 || gain > MaximumTargetDailyGainKilograms))
        {
            return Result<NutritionPlan>.Failure(PlanningErrors.TargetDailyGainInvalid);
        }

        var notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();
        if (notes?.Length > MaximumNotesLength)
        {
            return Result<NutritionPlan>.Failure(PlanningErrors.NutritionNotesTooLong);
        }

        if (command.Items.Count == 0)
        {
            return Result<NutritionPlan>.Failure(PlanningErrors.NutritionPlanItemsRequired);
        }

        var items = new List<RationItem>(command.Items.Count);
        foreach (var draft in command.Items)
        {
            var item = RationItem.Create(draft.Feed, draft.Kind, draft.KilogramsPerHeadPerDay);
            if (item.IsFailure)
            {
                return Result<NutritionPlan>.Failure(item.DomainError);
            }

            items.Add(item.Value!);
        }

        var plan = new NutritionPlan
        {
            Id = new NutritionPlanId(Guid.NewGuid()),
            RanchId = command.RanchId,
            LotId = command.LotId,
            Name = name,
            ValidFrom = validFrom,
            ValidTo = command.ValidTo,
            TargetDailyGainKilograms = command.TargetDailyGainKilograms,
            Notes = notes
        };
        plan._items.AddRange(items);
        return Result<NutritionPlan>.Success(plan);
    }

    /// <summary>
    /// Two plans overlap when they share at least one day. A missing end covers every later day.
    /// </summary>
    public bool Overlaps(DateOnly from, DateOnly? to) =>
        ValidFrom <= (to ?? DateOnly.MaxValue) && (ValidTo ?? DateOnly.MaxValue) >= from;

    /// <summary>
    /// Ends an open-ended plan on the day before a ration change, so that the history keeps the change date. Refused as an overlap when the plan is already bounded, or when it started on that day and would end before it starts.
    /// </summary>
    public Result CloseBefore(DateOnly day)
    {
        if (ValidTo is not null || day <= ValidFrom)
        {
            return Result.Failure(PlanningErrors.NutritionPlanOverlap);
        }

        ValidTo = day.AddDays(-1);
        return Result.Success();
    }
}
