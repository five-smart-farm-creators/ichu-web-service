using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Projects an application record to the REST contract with its activity identifier as a plain value.
/// </summary>
public static class ApplicationRecordResourceFromEntityAssembler
{
    public static ApplicationRecordResource ToResourceFromEntity(ApplicationRecord application) =>
        new(application.Id.Value, application.ActivityId.Value, application.CattleId, application.AppliedAt, application.InterventionId);
}
