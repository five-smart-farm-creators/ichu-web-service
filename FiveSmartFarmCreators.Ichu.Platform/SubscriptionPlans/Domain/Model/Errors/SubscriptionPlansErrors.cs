using System.Globalization;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;

/// <summary>
///     Business errors of the Subscription Plans context. Each code is also the resource key of its localized message,
///     so the English text lives in one place.
/// </summary>
public static class SubscriptionPlansErrors
{
    // Declared first because the error fields below read their messages during static initialization.
    private static readonly ResourceManager Messages = new(typeof(SubscriptionPlansMessages));

    public static readonly Error PlanNotFound = Define("PlanNotFound", EErrorType.NotFound);

    /// <summary>The plan is not published, so it cannot be quoted or sold.</summary>
    public static readonly Error PlanUnavailable = Define("PlanUnavailable", EErrorType.Conflict);

    /// <summary>The head count is above the device limit of the plan.</summary>
    public static readonly Error HeadCountExceedsPlan = Define("HeadCountExceedsPlan", EErrorType.Validation);

    /// <summary>The head count is outside the accepted range.</summary>
    public static readonly Error InvalidHeadCount = Define("InvalidHeadCount", EErrorType.Validation);

    /// <summary>The expected total no longer matches the current quote.</summary>
    public static readonly Error QuoteChanged = Define("QuoteChanged", EErrorType.Conflict);

    /// <summary>The request accepts terms that are no longer current.</summary>
    public static readonly Error TermsVersionOutdated = Define("TermsVersionOutdated", EErrorType.Conflict);

    /// <summary>The ranch already has a pending or active subscription.</summary>
    public static readonly Error SubscriptionAlreadyExists = Define("SubscriptionAlreadyExists", EErrorType.Conflict);

    /// <summary>The operation identifier already belongs to a request with different data.</summary>
    public static readonly Error OperationConflict = Define("OperationConflict", EErrorType.Conflict);

    public static readonly Error SubscriptionNotFound = Define("SubscriptionNotFound", EErrorType.NotFound);

    public static readonly Error InvalidStatusTransition = Define("InvalidStatusTransition", EErrorType.Conflict);

    /// <summary>The cancellation reason is empty or longer than the allowed length.</summary>
    public static readonly Error CancellationReasonRequired = Define("CancellationReasonRequired", EErrorType.Validation);

    public static readonly Error RanchAccessDenied = Define("RanchAccessDenied", EErrorType.Forbidden);

    public static readonly Error Unauthenticated = Define("Unauthenticated", EErrorType.Unauthorized);

    private static Error Define(string name, EErrorType type)
    {
        var code = $"SubscriptionPlans.{name}";
        return new Error(code, Messages.GetString(code, CultureInfo.InvariantCulture) ?? code, type);
    }
}
