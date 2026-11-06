namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;

/// <summary>
///     Entity representing one vertex of a geofence boundary.
/// </summary>
public class GeofenceVertex
{
    private GeofenceVertex()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GeofenceVertex"/> class.
    /// </summary>
    /// <param name="sequence">The zero-based position of the vertex in the boundary ring.</param>
    /// <param name="latitude">The latitude in decimal degrees.</param>
    /// <param name="longitude">The longitude in decimal degrees.</param>
    public GeofenceVertex(int sequence, double latitude, double longitude)
    {
        Sequence = sequence;
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Gets the zero-based position of the vertex in the boundary ring.</summary>
    public int Sequence { get; private set; }

    /// <summary>Gets the latitude in decimal degrees.</summary>
    public double Latitude { get; private set; }

    /// <summary>Gets the longitude in decimal degrees.</summary>
    public double Longitude { get; private set; }
}
