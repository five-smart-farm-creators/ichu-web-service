namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;

/// <summary>Records the initial productive-unit registration.</summary>
public sealed class RanchRegistration
{
    private RanchRegistration() { }

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
