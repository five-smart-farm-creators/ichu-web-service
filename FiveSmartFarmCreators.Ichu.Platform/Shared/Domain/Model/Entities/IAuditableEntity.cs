namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Entities;

/// <summary>
///     Marks an entity whose audit timestamps are maintained by the persistence layer.
/// </summary>
/// <remarks>
///     The auditable entity interceptor sets <see cref="CreatedAt"/> on the first save and refreshes
///     <see cref="UpdatedAt"/> on every save, for every bounded context that shares the application context.
/// </remarks>
public interface IAuditableEntity
{
    /// <summary>
    ///     Gets or sets the UTC instant when the entity was first saved.
    /// </summary>
    DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    ///     Gets or sets the UTC instant when the entity was last saved.
    /// </summary>
    DateTimeOffset? UpdatedAt { get; set; }
}
