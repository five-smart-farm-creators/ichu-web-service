using System.Security.Cryptography;
using System.Text;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

/// <summary>
///     SHA-256 digest of an edge gateway secret, so that a presented key is never stored.
/// </summary>
public sealed class GatewayKeyHash
{
    public const int DigestLength = 32;

    private readonly byte[] _digest;

    /// <summary>
    ///     Rebuilds a digest that was already computed when it was stored. A new secret must use <see cref="Of"/>.
    /// </summary>
    public GatewayKeyHash(byte[] digest)
    {
        if (digest.Length != DigestLength)
        {
            throw new ArgumentException("A gateway key digest must contain 32 bytes.", nameof(digest));
        }

        _digest = (byte[])digest.Clone();
    }

    public static GatewayKeyHash Of(string secret) => new(Digest(secret));

    public bool Matches(string secret) => CryptographicOperations.FixedTimeEquals(_digest, Digest(secret));

    public byte[] ToBytes() => (byte[])_digest.Clone();

    private static byte[] Digest(string secret)
    {
        // The secret is 256 random bits, so a fast hash is enough and no password derivation function is needed.
        return SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    }
}
