using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Integration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.AspNetCore.Http;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Infrastructure;

public class IoTAssetsEdgeGatewayAuthenticatorTests
{
    private static readonly Guid GatewayId = Guid.Parse("c07d8c6e-6d9b-4cb7-9add-7b8c9daebfc6");
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private const string PresentedKey = "c07d8c6e-6d9b-4cb7-9add-7b8c9daebfc6.development-gateway-key";

    [Fact]
    public async Task AuthenticateAsync_WithoutTheKeyHeaderReturnsNullWithoutAskingIoTAssets()
    {
        var facade = new FakeIoTAssetsFacade();
        var authenticator = new IoTAssetsEdgeGatewayAuthenticator(AccessorFor(new DefaultHttpContext()), facade);

        var principal = await authenticator.AuthenticateAsync(CancellationToken.None);

        Assert.Null(principal);
        Assert.Null(facade.PresentedKey);
    }

    [Fact]
    public async Task AuthenticateAsync_MapsTheVerifiedGatewayToItsPrincipal()
    {
        var facade = new FakeIoTAssetsFacade { Gateway = new EdgeGatewaySnapshot(GatewayId, RanchId) };
        var context = new DefaultHttpContext();
        context.Request.Headers[IoTAssetsEdgeGatewayAuthenticator.KeyHeader] = PresentedKey;
        var authenticator = new IoTAssetsEdgeGatewayAuthenticator(AccessorFor(context), facade);

        var principal = await authenticator.AuthenticateAsync(CancellationToken.None);

        Assert.Equal(new EdgeGatewayPrincipal(GatewayId, RanchId), principal);
        Assert.Equal(PresentedKey, facade.PresentedKey);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNullWhenIoTAssetsRejectsTheKey()
    {
        var facade = new FakeIoTAssetsFacade();
        var context = new DefaultHttpContext();
        context.Request.Headers[IoTAssetsEdgeGatewayAuthenticator.KeyHeader] = "wrong-key";
        var authenticator = new IoTAssetsEdgeGatewayAuthenticator(AccessorFor(context), facade);

        var principal = await authenticator.AuthenticateAsync(CancellationToken.None);

        Assert.Null(principal);
    }

    private static IHttpContextAccessor AccessorFor(HttpContext context) => new HttpContextAccessor { HttpContext = context };

    private sealed class FakeIoTAssetsFacade : IIoTAssetsContextFacade
    {
        public EdgeGatewaySnapshot? Gateway { get; init; }

        public string? PresentedKey { get; private set; }

        public Task<EdgeGatewaySnapshot?> AuthenticateEdgeGatewayAsync(string presentedKey, CancellationToken cancellationToken)
        {
            PresentedKey = presentedKey;
            return Task.FromResult(Gateway);
        }

        public Task<DeviceAttributionSnapshot?> FindDeviceAttributionAsync(Guid deviceId, DateTimeOffset instant, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Result<DeviceAssignmentSnapshot?>> GetCurrentCollarAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Result<DeviceAssignmentSnapshot?>> ResolveAssignmentAtAsync(Guid ranchId, Guid deviceId, DateTimeOffset instant,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Result<IReadOnlyList<DeviceAssignmentSnapshot>>> GetActiveAssignmentsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Result<WaterTroughConfiguration>> GetWaterTroughConfigurationAsync(Guid ranchId, Guid waterTroughId,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Result> RegisterCommunicationAsync(Guid ranchId, Guid deviceId, DateTimeOffset communicatedAt, DateTimeOffset receivedAt,
            int? batteryPercentage, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Result<int>> ReleaseCollarForInactiveCattleAsync(Guid ranchId, Guid cattleId, DateTimeOffset deactivatedAt,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<IReadOnlyList<DeviceAssignmentSnapshot>> FindOpenCollarAssignmentsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
