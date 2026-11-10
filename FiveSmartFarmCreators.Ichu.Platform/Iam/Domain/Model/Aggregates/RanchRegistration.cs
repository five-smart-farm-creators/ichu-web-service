using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     Records the ranch identity that other contexts store, so its identifier stays a plain Guid.
/// </summary>
public sealed class RanchRegistration : AggregateRoot
{
    private const int MaximumNameLength = 160;
    private const int MaximumLocationLength = 120;

    // Entity Framework Core materializes ranches without running the factory.
    private RanchRegistration()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Region { get; private set; } = string.Empty;

    public string District { get; private set; } = string.Empty;

    public int EstimatedHeadCount { get; private set; }

    public static Result<RanchRegistration> Create(string name, string region, string district, int estimatedHeadCount)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaximumNameLength ||
            string.IsNullOrWhiteSpace(region) || region.Length > MaximumLocationLength ||
            string.IsNullOrWhiteSpace(district) || district.Length > MaximumLocationLength ||
            estimatedHeadCount < 0)
        {
            return Result<RanchRegistration>.Failure(IamErrors.RanchDetailsRequired);
        }

        return Result<RanchRegistration>.Success(new RanchRegistration
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Region = region.Trim(),
            District = district.Trim(),
            EstimatedHeadCount = estimatedHeadCount
        });
    }
}
