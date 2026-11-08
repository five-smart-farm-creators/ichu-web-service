using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Domain;

public class EdgeGatewayTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly DateTimeOffset At = new(2026, 11, 7, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_TrimsTheNameAndStartsActive()
    {
        var result = EdgeGateway.Register(new RegisterEdgeGatewayCommand(RanchId, "  Barn gateway  "), GatewayKeyHash.Of("secret"), At);

        Assert.True(result.IsSuccess);
        Assert.Equal("Barn gateway", result.Value!.Name);
        Assert.Equal(EdgeGatewayStatus.Active, result.Value.Status);
        Assert.Equal(At, result.Value.RegisteredAt);
        Assert.Null(result.Value.RevokedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_RejectsBlankNames(string name)
    {
        var result = EdgeGateway.Register(new RegisterEdgeGatewayCommand(RanchId, name), GatewayKeyHash.Of("secret"), At);

        Assert.Equal(IoTAssetsErrors.InvalidEdgeGatewayName.Code, result.DomainError.Code);
    }

    [Fact]
    public void Register_RejectsNamesLongerThanEightyCharacters()
    {
        var result = EdgeGateway.Register(new RegisterEdgeGatewayCommand(RanchId, new string('g', 81)), GatewayKeyHash.Of("secret"), At);

        Assert.Equal(IoTAssetsErrors.InvalidEdgeGatewayName.Code, result.DomainError.Code);
    }

    [Fact]
    public void Authenticates_OnlyTheRegisteredSecretWhileActive()
    {
        var gateway = Register("secret");

        Assert.True(gateway.Authenticates("secret"));
        Assert.False(gateway.Authenticates("other-secret"));
    }

    [Fact]
    public void Authenticates_RefusesARevokedGatewayEvenWithItsSecret()
    {
        var gateway = Register("secret");

        gateway.Revoke(At.AddHours(1));

        Assert.False(gateway.Authenticates("secret"));
    }

    [Fact]
    public void Revoke_TwiceReportsTheGatewayAsAlreadyRevoked()
    {
        var gateway = Register("secret");

        var first = gateway.Revoke(At.AddHours(1));
        var second = gateway.Revoke(At.AddHours(2));

        Assert.True(first.IsSuccess);
        Assert.Equal(EdgeGatewayStatus.Revoked, gateway.Status);
        Assert.Equal(At.AddHours(1), gateway.RevokedAt!.Value);
        Assert.Equal(IoTAssetsErrors.EdgeGatewayAlreadyRevoked.Code, second.DomainError.Code);
    }

    private static EdgeGateway Register(string secret) =>
        EdgeGateway.Register(new RegisterEdgeGatewayCommand(RanchId, "Barn gateway"), GatewayKeyHash.Of(secret), At).Value!;
}
