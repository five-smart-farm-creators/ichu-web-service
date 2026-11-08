using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;

/// <summary>
///     An edge gateway of a ranch that authenticates with its secret. The secret is kept only as a digest, and a revoked
///     gateway never authenticates again.
/// </summary>
public class EdgeGateway : AggregateRoot
{
    private const int MaximumNameLength = 80;

    // Entity Framework Core materializes gateways without running the factory.
    private EdgeGateway()
    {
    }

    public EdgeGatewayId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public GatewayKeyHash KeyHash { get; private set; } = null!;

    public EdgeGatewayStatus Status { get; private set; }

    public DateTimeOffset RegisteredAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>
    ///     Advanced by every change, so that a concurrent revocation cannot silently overwrite another change.
    /// </summary>
    public Guid Version { get; private set; }

    public static Result<EdgeGateway> Register(RegisterEdgeGatewayCommand command, GatewayKeyHash keyHash, DateTimeOffset registeredAt)
    {
        if (command.RanchId == Guid.Empty || registeredAt == default)
        {
            return Result<EdgeGateway>.Failure(IoTAssetsErrors.InvalidInput);
        }

        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Trim().Length > MaximumNameLength)
        {
            return Result<EdgeGateway>.Failure(IoTAssetsErrors.InvalidEdgeGatewayName);
        }

        var gateway = new EdgeGateway
        {
            Id = new EdgeGatewayId(Guid.NewGuid()),
            RanchId = command.RanchId,
            Name = command.Name.Trim(),
            KeyHash = keyHash,
            Status = EdgeGatewayStatus.Active,
            RegisteredAt = registeredAt.ToUniversalTime(),
            Version = Guid.NewGuid()
        };
        return Result<EdgeGateway>.Success(gateway);
    }

    public Result Revoke(DateTimeOffset at)
    {
        if (Status == EdgeGatewayStatus.Revoked)
        {
            return Result.Failure(IoTAssetsErrors.EdgeGatewayAlreadyRevoked);
        }

        Status = EdgeGatewayStatus.Revoked;
        RevokedAt = at.ToUniversalTime();
        Version = Guid.NewGuid();
        return Result.Success();
    }

    public bool Authenticates(string secret) => Status == EdgeGatewayStatus.Active && KeyHash.Matches(secret);
}
