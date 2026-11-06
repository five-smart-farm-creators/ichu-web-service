using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;

public sealed class IoTAssetsException(Error error) : Exception(error.Message)
{
    public Error Error { get; } = error;
}
