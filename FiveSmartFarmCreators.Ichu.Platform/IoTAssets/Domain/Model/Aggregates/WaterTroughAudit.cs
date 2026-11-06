using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

public partial class WaterTrough : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
