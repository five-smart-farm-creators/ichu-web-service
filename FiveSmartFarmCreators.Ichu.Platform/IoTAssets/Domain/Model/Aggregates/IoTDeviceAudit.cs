using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

public partial class IoTDevice : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
