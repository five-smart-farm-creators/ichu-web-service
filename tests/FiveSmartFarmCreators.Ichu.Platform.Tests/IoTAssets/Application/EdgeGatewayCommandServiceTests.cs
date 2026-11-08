using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Time.Testing;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;

public class EdgeGatewayCommandServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid OtherRanchId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly DateTimeOffset At = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeEdgeGatewayRepository _gateways = new();

    private EdgeGatewayCommandService CreateService() =>
        new(_gateways, new AllowAllAccess(), new DirectTransaction(), new FakeTimeProvider(At));

    [Fact]
    public async Task Register_ReturnsTheKeyWithTheGatewayIdentifierAndASecretThatAuthenticates()
    {
        var result = await CreateService().HandleAsync(new RegisterEdgeGatewayCommand(RanchId, " Barn gateway "), CancellationToken.None);

        var gateway = Assert.Single(_gateways.Items);
        Assert.True(result.IsSuccess);
        Assert.Equal("Barn gateway", gateway.Name);
        Assert.StartsWith($"{gateway.Id.Value:D}.", result.Value!.Key);
        var secret = result.Value.Key[(result.Value.Key.IndexOf('.') + 1)..];
        Assert.Equal(43, secret.Length);
        Assert.True(gateway.Authenticates(secret));
    }

    [Fact]
    public async Task Register_WithABlankNameIsRefusedWithoutSavingTheGateway()
    {
        var result = await CreateService().HandleAsync(new RegisterEdgeGatewayCommand(RanchId, "   "), CancellationToken.None);

        Assert.Equal(IoTAssetsErrors.InvalidEdgeGatewayName.Code, result.DomainError.Code);
        Assert.Empty(_gateways.Items);
    }

    [Fact]
    public async Task Revoke_OfAGatewayInAnotherRanchReportsNotFoundAndKeepsItActive()
    {
        var registration = await CreateService().HandleAsync(new RegisterEdgeGatewayCommand(RanchId, "Barn gateway"), CancellationToken.None);

        var result = await CreateService().HandleAsync(
            new RevokeEdgeGatewayCommand(OtherRanchId, registration.Value!.Gateway.Id), CancellationToken.None);

        Assert.Equal(IoTAssetsErrors.EdgeGatewayNotFound.Code, result.DomainError.Code);
        Assert.Equal(EdgeGatewayStatus.Active, registration.Value.Gateway.Status);
    }

    [Fact]
    public async Task Revoke_TwiceReportsTheGatewayAsAlreadyRevoked()
    {
        var service = CreateService();
        var registration = await service.HandleAsync(new RegisterEdgeGatewayCommand(RanchId, "Barn gateway"), CancellationToken.None);
        var command = new RevokeEdgeGatewayCommand(RanchId, registration.Value!.Gateway.Id);

        var first = await service.HandleAsync(command, CancellationToken.None);
        var second = await service.HandleAsync(command, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(IoTAssetsErrors.EdgeGatewayAlreadyRevoked.Code, second.DomainError.Code);
    }

    private sealed class AllowAllAccess : IIoTAssetsAccessService
    {
        public Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Result.Success());

        public Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken) => Task.FromResult(Result.Success());

        public Task<Result> EnsureCommunicationAccessAsync(Guid ranchId, Guid deviceId, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success());
    }

    private sealed class DirectTransaction : IIoTAssetsTransaction
    {
        public Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> operation, CancellationToken cancellationToken) =>
            operation(cancellationToken);
    }
}
