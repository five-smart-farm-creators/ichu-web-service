using System.Globalization;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;

/// <summary>
///     Business errors of the Cattle Information context. Each code is also the resource key of its localized message,
///     so the English text lives in one place.
/// </summary>
public static class CattleInformationErrors
{
    // Declared first because the error fields below read their messages during static initialization.
    private static readonly ResourceManager Messages = new(typeof(CattleInformationMessages));

    public static readonly Error RanchIdentifierRequired = Define("RanchIdentifierRequired", EErrorType.Validation);
    public static readonly Error CattleIdentifierRequired = Define("CattleIdentifierRequired", EErrorType.Validation);
    public static readonly Error LotIdentifierRequired = Define("LotIdentifierRequired", EErrorType.Validation);
    public static readonly Error InvalidEarTag = Define("InvalidEarTag", EErrorType.Validation);
    public static readonly Error InvalidBreed = Define("InvalidBreed", EErrorType.Validation);
    public static readonly Error InvalidSex = Define("InvalidSex", EErrorType.Validation);
    public static readonly Error InvalidBirthDate = Define("InvalidBirthDate", EErrorType.Validation);
    public static readonly Error BirthDateAfterLifeStage = Define("BirthDateAfterLifeStage", EErrorType.Validation);
    public static readonly Error InvalidLifeStage = Define("InvalidLifeStage", EErrorType.Validation);
    public static readonly Error InvalidLifeStageDate = Define("InvalidLifeStageDate", EErrorType.Validation);
    public static readonly Error InvalidExitReason = Define("InvalidExitReason", EErrorType.Validation);
    public static readonly Error InvalidExitDate = Define("InvalidExitDate", EErrorType.Validation);
    public static readonly Error InvalidLotName = Define("InvalidLotName", EErrorType.Validation);
    public static readonly Error InvalidLotCriterion = Define("InvalidLotCriterion", EErrorType.Validation);
    public static readonly Error CattleFromAnotherRanch = Define("CattleFromAnotherRanch", EErrorType.Validation);
    public static readonly Error AssignmentTimeRequired = Define("AssignmentTimeRequired", EErrorType.Validation);
    public static readonly Error InvalidSearch = Define("InvalidSearch", EErrorType.Validation);

    public static readonly Error CattleNotFound = Define("CattleNotFound", EErrorType.NotFound);
    public static readonly Error LotNotFound = Define("LotNotFound", EErrorType.NotFound);

    /// <summary>The animal changed after the caller read it, so the caller must reload it before retrying.</summary>
    public static readonly Error CattleVersionMismatch = Define("CattleVersionMismatch", EErrorType.Conflict);

    /// <summary>The lot changed after the caller read it, so the caller must reload it before retrying.</summary>
    public static readonly Error LotVersionMismatch = Define("LotVersionMismatch", EErrorType.Conflict);

    public static readonly Error CattleInactive = Define("CattleInactive", EErrorType.Conflict);
    public static readonly Error InactiveCattleAssignment = Define("InactiveCattleAssignment", EErrorType.Conflict);
    public static readonly Error LifeStageAlreadyCurrent = Define("LifeStageAlreadyCurrent", EErrorType.Conflict);
    public static readonly Error DuplicateEarTag = Define("DuplicateEarTag", EErrorType.Conflict);

    public static readonly Error Unauthenticated = Define("Unauthenticated", EErrorType.Unauthorized);
    public static readonly Error RanchAccessDenied = Define("RanchAccessDenied", EErrorType.Forbidden);

    private static Error Define(string name, EErrorType type)
    {
        var code = $"CattleInformation.{name}";
        return new Error(code, Messages.GetString(code, CultureInfo.InvariantCulture) ?? code, type);
    }
}
