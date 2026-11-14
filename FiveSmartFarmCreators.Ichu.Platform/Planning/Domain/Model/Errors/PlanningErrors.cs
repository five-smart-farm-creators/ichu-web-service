using System.Globalization;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

/// <summary>
///     Business errors of the Planning context. Each code is also the resource key of its localized message, so the English text lives in one place.
/// </summary>
public static class PlanningErrors
{
    // Declared first because the error fields below read their messages during static initialization.
    private static readonly ResourceManager Messages = new(typeof(PlanningMessages));

    public static readonly Error InvalidIdentifier = Define("InvalidIdentifier", EErrorType.Validation);
    public static readonly Error CampaignNameRequired = Define("CampaignNameRequired", EErrorType.Validation);
    public static readonly Error CampaignKindInvalid = Define("CampaignKindInvalid", EErrorType.Validation);
    public static readonly Error CampaignDateRangeInvalid = Define("CampaignDateRangeInvalid", EErrorType.Validation);
    public static readonly Error ActivityTitleRequired = Define("ActivityTitleRequired", EErrorType.Validation);
    public static readonly Error UtcTimestampRequired = Define("UtcTimestampRequired", EErrorType.Validation);
    public static readonly Error FollowUpDueAtInvalid = Define("FollowUpDueAtInvalid", EErrorType.Validation);
    public static readonly Error CalendarRangeInvalid = Define("CalendarRangeInvalid", EErrorType.Validation);
    public static readonly Error WithdrawalDateRangeInvalid = Define("WithdrawalDateRangeInvalid", EErrorType.Validation);
    public static readonly Error NutritionPlanNameRequired = Define("NutritionPlanNameRequired", EErrorType.Validation);
    public static readonly Error NutritionPlanNameTooLong = Define("NutritionPlanNameTooLong", EErrorType.Validation);
    public static readonly Error NutritionPlanValidFromRequired = Define("NutritionPlanValidFromRequired", EErrorType.Validation);
    public static readonly Error NutritionPlanValidityInvalid = Define("NutritionPlanValidityInvalid", EErrorType.Validation);
    public static readonly Error NutritionPlanItemsRequired = Define("NutritionPlanItemsRequired", EErrorType.Validation);
    public static readonly Error NutritionNotesTooLong = Define("NutritionNotesTooLong", EErrorType.Validation);
    public static readonly Error TargetDailyGainInvalid = Define("TargetDailyGainInvalid", EErrorType.Validation);
    public static readonly Error RationFeedRequired = Define("RationFeedRequired", EErrorType.Validation);
    public static readonly Error RationFeedTooLong = Define("RationFeedTooLong", EErrorType.Validation);
    public static readonly Error RationKindInvalid = Define("RationKindInvalid", EErrorType.Validation);
    public static readonly Error RationQuantityInvalid = Define("RationQuantityInvalid", EErrorType.Validation);

    public static readonly Error CampaignNotFound = Define("CampaignNotFound", EErrorType.NotFound);
    public static readonly Error ActivityNotFound = Define("ActivityNotFound", EErrorType.NotFound);
    public static readonly Error CattleNotFound = Define("CattleNotFound", EErrorType.NotFound);
    public static readonly Error InterventionNotFound = Define("InterventionNotFound", EErrorType.NotFound);
    public static readonly Error LotNotFound = Define("LotNotFound", EErrorType.NotFound);
    public static readonly Error NoCurrentNutritionPlan = Define("NoCurrentNutritionPlan", EErrorType.NotFound);

    public static readonly Error CampaignRanchMismatch = Define("CampaignRanchMismatch", EErrorType.Conflict);
    public static readonly Error ActivityCattleMismatch = Define("ActivityCattleMismatch", EErrorType.Conflict);
    public static readonly Error InterventionCattleMismatch = Define("InterventionCattleMismatch", EErrorType.Conflict);
    public static readonly Error NutritionPlanOverlap = Define("NutritionPlanOverlap", EErrorType.Conflict);
    public static readonly Error ConcurrentModification = Define("ConcurrentModification", EErrorType.Conflict);

    public static readonly Error RanchAccessDenied = Define("RanchAccessDenied", EErrorType.Forbidden);

    private static Error Define(string name, EErrorType type)
    {
        var code = $"Planning.{name}";
        return new Error(code, Messages.GetString(code, CultureInfo.InvariantCulture) ?? code, type);
    }
}
