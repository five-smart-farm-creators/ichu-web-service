namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

/// <summary>
///     Records the initial registration details for a ranch.
/// </summary>
public sealed class RanchRegistration
{
    private RanchRegistration() { }

    /// <summary>
    ///     Creates a ranch registration with a nonnegative estimated head count.
    /// </summary>
    /// <param name="name">
    ///     The ranch name.
    /// </param>
    /// <param name="region">
    ///     The region where the ranch is located.
    /// </param>
    /// <param name="district">
    ///     The district where the ranch is located.
    /// </param>
    /// <param name="estimatedHeadCount">
    ///     The estimated number of animals.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     A required value is blank or too long, or the estimated head count is negative.
    /// </exception>
    public RanchRegistration(string name, string region, string district, int estimatedHeadCount)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 160 || string.IsNullOrWhiteSpace(region) ||
            region.Length > 120 || string.IsNullOrWhiteSpace(district) || district.Length > 120 || estimatedHeadCount < 0)
        {
            throw new ArgumentException("Ranch name, region, district and a nonnegative head count are required.");
        }
        Id = Guid.NewGuid();
        Name = name.Trim();
        Region = region.Trim();
        District = district.Trim();
        EstimatedHeadCount = estimatedHeadCount;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public string District { get; private set; } = string.Empty;
    public int EstimatedHeadCount { get; private set; }

}
