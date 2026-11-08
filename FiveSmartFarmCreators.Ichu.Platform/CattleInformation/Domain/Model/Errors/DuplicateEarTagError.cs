using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;

/// <summary>
///     Reports an ear tag held by another animal of the ranch. The holder travels with the error so that clients can open it.
/// </summary>
public sealed record DuplicateEarTagError(CattleId ConflictingCattleId)
    : Error(CattleInformationErrors.DuplicateEarTag.Code, CattleInformationErrors.DuplicateEarTag.Message, EErrorType.Conflict);
