using System.Globalization;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;

/// <summary>
///     Business errors of the Identity and Access context. Each code is also the resource key of its localized message,
///     so the English text lives in one place.
/// </summary>
public static class IamErrors
{
    // Declared first because the error fields below read their messages during static initialization.
    private static readonly ResourceManager Messages = new(typeof(IamMessages));

    public static readonly Error InvalidIdentifier = Define("InvalidIdentifier", EErrorType.Validation);
    public static readonly Error InvalidRole = Define("InvalidRole", EErrorType.Validation);
    public static readonly Error InvalidRegistration = Define("InvalidRegistration", EErrorType.Validation);
    public static readonly Error InvalidExternalIdentity = Define("InvalidExternalIdentity", EErrorType.Validation);
    public static readonly Error InvalidEmail = Define("InvalidEmail", EErrorType.Validation);
    public static readonly Error PasswordRequired = Define("PasswordRequired", EErrorType.Validation);
    public static readonly Error RanchDetailsRequired = Define("RanchDetailsRequired", EErrorType.Validation);
    public static readonly Error InvalidProfile = Define("InvalidProfile", EErrorType.Validation);
    public static readonly Error InvalidAdvisory = Define("InvalidAdvisory", EErrorType.Validation);
    public static readonly Error InvalidAdvisoryScope = Define("InvalidAdvisoryScope", EErrorType.Validation);
    public static readonly Error ExpirationRequired = Define("ExpirationRequired", EErrorType.Validation);
    public static readonly Error ExpirationNotInFuture = Define("ExpirationNotInFuture", EErrorType.Validation);
    public static readonly Error InvalidDecision = Define("InvalidDecision", EErrorType.Validation);

    public static readonly Error ProfileOnlyForVeterinarians = Define("ProfileOnlyForVeterinarians", EErrorType.Forbidden);
    public static readonly Error VeterinarianProfileRequired = Define("VeterinarianProfileRequired", EErrorType.Forbidden);
    public static readonly Error OwnerRequired = Define("OwnerRequired", EErrorType.Forbidden);
    public static readonly Error RanchAccessDenied = Define("RanchAccessDenied", EErrorType.Forbidden);

    public static readonly Error Unauthenticated = Define("Unauthenticated", EErrorType.Unauthorized);
    public static readonly Error AccountNotActive = Define("AccountNotActive", EErrorType.Unauthorized);
    public static readonly Error InvalidCredentials = Define("InvalidCredentials", EErrorType.Unauthorized);

    public static readonly Error RanchNotFound = Define("RanchNotFound", EErrorType.NotFound);
    public static readonly Error AdvisoryNotFound = Define("AdvisoryNotFound", EErrorType.NotFound);

    public static readonly Error AccountAlreadyExists = Define("AccountAlreadyExists", EErrorType.Conflict);
    public static readonly Error LicenseAlreadyRegistered = Define("LicenseAlreadyRegistered", EErrorType.Conflict);
    public static readonly Error AdvisoryRequestAlreadyOpen = Define("AdvisoryRequestAlreadyOpen", EErrorType.Conflict);
    public static readonly Error AdvisoryAlreadyActive = Define("AdvisoryAlreadyActive", EErrorType.Conflict);
    public static readonly Error AdvisoryNotPending = Define("AdvisoryNotPending", EErrorType.Conflict);
    public static readonly Error AdvisoryTransitionInvalid = Define("AdvisoryTransitionInvalid", EErrorType.Conflict);
    public static readonly Error VeterinarianNotEligible = Define("VeterinarianNotEligible", EErrorType.Conflict);

    public static readonly Error IdentityProviderUnavailable = Define("IdentityProviderUnavailable", EErrorType.Unavailable);
    public static readonly Error IdentityProviderNotConfigured = Define("IdentityProviderNotConfigured", EErrorType.Unavailable);

    private static Error Define(string name, EErrorType type)
    {
        var code = $"Iam.{name}";
        return new Error(code, Messages.GetString(code, CultureInfo.InvariantCulture) ?? code, type);
    }
}
