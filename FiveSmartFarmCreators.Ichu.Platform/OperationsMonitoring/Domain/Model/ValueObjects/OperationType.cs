namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Identifies the kind of livestock operation recorded for an animal.
/// </summary>
public enum OperationType
{
    /// <summary>A health event observed by an operator.</summary>
    HealthEvent,

    /// <summary>A birth observed by an operator.</summary>
    Birth,

    /// <summary>A vaccination applied by a veterinarian.</summary>
    Vaccination,

    /// <summary>A treatment applied by a veterinarian.</summary>
    Treatment,

    /// <summary>An insemination applied by a veterinarian.</summary>
    Insemination
}
