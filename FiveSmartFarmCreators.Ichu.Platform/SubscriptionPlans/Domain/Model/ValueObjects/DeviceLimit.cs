namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Maximum number of collars a plan covers. A plan always covers at least one device.
/// </summary>
public readonly record struct DeviceLimit
{
    /// <summary>
    ///     Creates a device limit. A limit below one would make the plan impossible to use.
    /// </summary>
    public DeviceLimit(int maximum)
    {
        if (maximum < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "A device limit must allow at least one device.");
        }

        Maximum = maximum;
    }

    public int Maximum { get; }

    /// <summary>Determines whether one more device fits after the devices already in use.</summary>
    public bool Allows(int used) => used < Maximum;

    public bool Covers(int headCount) => headCount <= Maximum;
}
