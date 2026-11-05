namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;

public sealed class GeofenceVertex
{
    public int Sequence { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    private GeofenceVertex()
    {
    }

    public GeofenceVertex(int sequence, double latitude, double longitude)
    {
        Sequence = sequence;
        Latitude = latitude;
        Longitude = longitude;
    }
}
