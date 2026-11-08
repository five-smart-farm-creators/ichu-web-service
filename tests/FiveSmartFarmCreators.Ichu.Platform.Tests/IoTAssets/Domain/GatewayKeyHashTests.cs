using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Domain;

public class GatewayKeyHashTests
{
    [Fact]
    public void Matches_OnlyTheSecretItWasComputedFrom()
    {
        var hash = GatewayKeyHash.Of("first-secret");

        Assert.True(hash.Matches("first-secret"));
        Assert.False(hash.Matches("second-secret"));
        Assert.False(hash.Matches(string.Empty));
    }

    [Fact]
    public void RebuiltFromItsStoredBytes_StillMatchesTheSecret()
    {
        var stored = GatewayKeyHash.Of("first-secret").ToBytes();

        var rebuilt = new GatewayKeyHash(stored);

        Assert.Equal(GatewayKeyHash.DigestLength, stored.Length);
        Assert.True(rebuilt.Matches("first-secret"));
    }

    [Fact]
    public void Constructor_RejectsDigestsThatAreNot32Bytes()
    {
        Assert.Throws<ArgumentException>(() => new GatewayKeyHash(new byte[31]));
    }
}
