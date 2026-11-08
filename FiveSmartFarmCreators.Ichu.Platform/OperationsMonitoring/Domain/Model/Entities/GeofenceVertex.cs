namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;

/// <summary>
///     One vertex of a geofence boundary, kept in ring order so that a redefinition replaces the whole ring.
/// </summary>
public class GeofenceVertex
{
    private GeofenceVertex()
    {
    }

    public GeofenceVertex(int sequence, double latitude, double longitude)
    {
        Sequence = sequence;
        Latitude = latitude;
        Longitude = longitude;
    }

    public int Sequence { get; private set; }

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }
}
