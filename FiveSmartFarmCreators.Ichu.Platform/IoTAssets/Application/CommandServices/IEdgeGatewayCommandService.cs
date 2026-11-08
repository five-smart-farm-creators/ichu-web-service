using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;

public interface IEdgeGatewayCommandService
{
    Task<Result<EdgeGatewayRegistration>> HandleAsync(RegisterEdgeGatewayCommand command, CancellationToken cancellationToken);
    Task<Result<EdgeGateway>> HandleAsync(RevokeEdgeGatewayCommand command, CancellationToken cancellationToken);
}
