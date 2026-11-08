using System.Buffers.Text;
using System.Security.Cryptography;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.CommandServices;

/// <summary>
///     Registers and revokes edge gateways. Each change runs in one transaction, and the aggregate decides whether it is allowed.
/// </summary>
public class EdgeGatewayCommandService(
    IEdgeGatewayRepository gatewayRepository,
    IIoTAssetsAccessService accessService,
    IIoTAssetsTransaction transaction,
    TimeProvider timeProvider) : IEdgeGatewayCommandService
{
    private const int SecretByteLength = 32;

    public async Task<Result<EdgeGatewayRegistration>> HandleAsync(RegisterEdgeGatewayCommand command, CancellationToken cancellationToken)
    {
        if (command.RanchId == Guid.Empty)
        {
            return Result<EdgeGatewayRegistration>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<EdgeGatewayRegistration>.Failure(access.DomainError);
        }

        return await transaction.ExecuteAsync(async token =>
        {
            var secret = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(SecretByteLength));
            var registration = EdgeGateway.Register(command, GatewayKeyHash.Of(secret), timeProvider.GetUtcNow());
            if (registration.IsFailure)
            {
                return Result<EdgeGatewayRegistration>.Failure(registration.DomainError);
            }

            var gateway = registration.Value!;
            await gatewayRepository.AddAsync(gateway, token);
            return Result<EdgeGatewayRegistration>.Success(new EdgeGatewayRegistration(gateway, $"{gateway.Id.Value:D}.{secret}"));
        }, cancellationToken);
    }

    public async Task<Result<EdgeGateway>> HandleAsync(RevokeEdgeGatewayCommand command, CancellationToken cancellationToken)
    {
        if (command.RanchId == Guid.Empty || command.GatewayId.Value == Guid.Empty)
        {
            return Result<EdgeGateway>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<EdgeGateway>.Failure(access.DomainError);
        }

        return await transaction.ExecuteAsync(async token =>
        {
            var gateway = await gatewayRepository.FindByIdAsync(command.RanchId, command.GatewayId, token);
            if (gateway is null)
            {
                return Result<EdgeGateway>.Failure(IoTAssetsErrors.EdgeGatewayNotFound);
            }

            var revocation = gateway.Revoke(timeProvider.GetUtcNow());
            return revocation.IsFailure
                ? Result<EdgeGateway>.Failure(revocation.DomainError)
                : Result<EdgeGateway>.Success(gateway);
        }, cancellationToken);
    }
}
