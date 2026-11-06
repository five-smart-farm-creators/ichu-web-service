using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
///     Aggregate root representing the permitted grazing area of one lot. Its boundary is a closed, simple polygon;
///     positions whose declared accuracy exceeds the tolerance are not conclusive for the geofence.
/// </summary>
public class Geofence
{
    /// <summary>Maximum number of characters of a geofence name.</summary>
    public const int MaximumNameLength = 80;

    /// <summary>Maximum number of vertices of a boundary.</summary>
    public const int MaximumVertices = 500;

    /// <summary>Maximum tolerance in meters.</summary>
    public const int MaximumToleranceMeters = short.MaxValue;

    private readonly List<GeofenceVertex> _vertices = [];

    private Geofence()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Geofence"/> class.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the lot.</param>
    /// <param name="lotId">The lot the geofence applies to.</param>
    /// <param name="name">The geofence name.</param>
    /// <param name="toleranceMeters">The maximum declared GPS accuracy, in meters, for a position to be conclusive.</param>
    /// <param name="severity">The severity of the alerts raised when an animal leaves the area.</param>
    /// <param name="boundary">The boundary vertices, optionally repeating the first vertex at the end.</param>
    /// <param name="revision">The rule revision that published the geofence.</param>
    /// <exception cref="ArgumentException">Thrown when an identifier is empty or the definition is invalid.</exception>
    public Geofence(
        Guid ranchId,
        Guid lotId,
        string name,
        int toleranceMeters,
        AlertSeverity severity,
        IReadOnlyList<GeoCoordinate> boundary,
        long revision)
    {
        if (ranchId == Guid.Empty || lotId == Guid.Empty)
        {
            throw new ArgumentException("Ranch and lot identifiers are required.");
        }

        Id = Guid.NewGuid();
        RanchId = ranchId;
        LotId = lotId;
        Redefine(name, toleranceMeters, severity, boundary, revision);
    }

    /// <summary>Gets the identifier of the geofence.</summary>
    public Guid Id { get; private set; }

    /// <summary>Gets the ranch that owns the lot.</summary>
    public Guid RanchId { get; private set; }

    /// <summary>Gets the lot the geofence applies to.</summary>
    public Guid LotId { get; private set; }

    /// <summary>Gets the geofence name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the maximum declared GPS accuracy, in meters, for a position to be conclusive.</summary>
    public int ToleranceMeters { get; private set; }

    /// <summary>Gets the severity of the alerts raised when an animal leaves the area.</summary>
    public AlertSeverity Severity { get; private set; }

    /// <summary>Gets the rule revision that last changed the geofence.</summary>
    public long Revision { get; private set; }

    /// <summary>Gets the stored boundary vertices.</summary>
    public IReadOnlyCollection<GeofenceVertex> Vertices => _vertices;

    /// <summary>
    ///     Replaces the name, tolerance, severity and boundary of the geofence.
    /// </summary>
    /// <param name="name">The geofence name.</param>
    /// <param name="toleranceMeters">The maximum declared GPS accuracy, in meters, for a position to be conclusive.</param>
    /// <param name="severity">The severity of the alerts raised when an animal leaves the area.</param>
    /// <param name="boundary">The boundary vertices, optionally repeating the first vertex at the end.</param>
    /// <param name="revision">The rule revision that publishes the change.</param>
    /// <exception cref="ArgumentException">Thrown when the name, tolerance or boundary is invalid.</exception>
    public void Redefine(string name, int toleranceMeters, AlertSeverity severity, IReadOnlyList<GeoCoordinate> boundary, long revision)
    {
        var normalizedName = name.Trim();
        if (normalizedName.Length == 0 || normalizedName.Length > MaximumNameLength)
        {
            throw new ArgumentException($"A geofence name must contain 1 to {MaximumNameLength} characters.");
        }

        if (toleranceMeters is < 1 or > MaximumToleranceMeters)
        {
            throw new ArgumentException($"The tolerance must be between 1 and {MaximumToleranceMeters} meters.");
        }

        var vertices = OpenRing(boundary);
        EnsureSimplePolygon(vertices);

        Name = normalizedName;
        ToleranceMeters = toleranceMeters;
        Severity = severity;
        Revision = revision;
        _vertices.Clear();
        _vertices.AddRange(vertices.Select((v, index) => new GeofenceVertex(index, v.Latitude, v.Longitude)));
    }

    /// <summary>
    ///     Gets the boundary vertices ordered along the ring.
    /// </summary>
    /// <returns>The ordered vertices, without repeating the first one.</returns>
    public IReadOnlyList<GeofenceVertex> Boundary() => _vertices.OrderBy(v => v.Sequence).ToList();

    // A boundary may repeat its first vertex at the end; the stored ring keeps each vertex once.
    private static List<GeoCoordinate> OpenRing(IReadOnlyList<GeoCoordinate> boundary)
    {
        var vertices = boundary.ToList();
        if (vertices.Count > 1 && vertices[0] == vertices[^1])
        {
            vertices.RemoveAt(vertices.Count - 1);
        }

        if (vertices.Count is < 3 or > MaximumVertices)
        {
            throw new ArgumentException($"A geofence requires between 3 and {MaximumVertices} distinct vertices.");
        }

        if (vertices.Distinct().Count() != vertices.Count)
        {
            throw new ArgumentException("A geofence boundary cannot repeat vertices.");
        }

        return vertices;
    }

    // A planar test on longitude and latitude is enough for grazing areas to reject crossing, overlapping or zero-area boundaries.
    private static void EnsureSimplePolygon(IReadOnlyList<GeoCoordinate> vertices)
    {
        var count = vertices.Count;
        if (Math.Abs(SignedArea(vertices)) < double.Epsilon)
        {
            throw new ArgumentException("A geofence boundary must enclose an area.");
        }

        for (var i = 0; i < count; i++)
        {
            var a1 = vertices[i];
            var a2 = vertices[(i + 1) % count];
            for (var j = i + 1; j < count; j++)
            {
                var b1 = vertices[j];
                var b2 = vertices[(j + 1) % count];
                var adjacent = j == i + 1 || (i == 0 && j == count - 1);
                var invalid = adjacent ? AdjacentEdgesOverlap(a1, a2, b1, b2) : SegmentsIntersect(a1, a2, b1, b2);
                if (invalid)
                {
                    throw new ArgumentException("A geofence boundary cannot have crossing or overlapping sides.");
                }
            }
        }
    }

    private static bool AdjacentEdgesOverlap(GeoCoordinate a1, GeoCoordinate a2, GeoCoordinate b1, GeoCoordinate b2)
    {
        var shared = a2 == b1 ? a2 : a1;
        var first = shared == a1 ? a2 : a1;
        var second = shared == b1 ? b2 : b1;
        var collinear = Cross(shared, first, second) == 0;
        var sameDirection = (first.Longitude - shared.Longitude) * (second.Longitude - shared.Longitude)
            + (first.Latitude - shared.Latitude) * (second.Latitude - shared.Latitude) > 0;
        return collinear && sameDirection;
    }

    private static bool SegmentsIntersect(GeoCoordinate p1, GeoCoordinate p2, GeoCoordinate p3, GeoCoordinate p4)
    {
        var d1 = Cross(p3, p4, p1);
        var d2 = Cross(p3, p4, p2);
        var d3 = Cross(p1, p2, p3);
        var d4 = Cross(p1, p2, p4);
        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) && ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
        {
            return true;
        }

        return (d1 == 0 && IsOnSegment(p3, p4, p1)) ||
               (d2 == 0 && IsOnSegment(p3, p4, p2)) ||
               (d3 == 0 && IsOnSegment(p1, p2, p3)) ||
               (d4 == 0 && IsOnSegment(p1, p2, p4));
    }

    private static double Cross(GeoCoordinate origin, GeoCoordinate a, GeoCoordinate b) =>
        (a.Longitude - origin.Longitude) * (b.Latitude - origin.Latitude) -
        (a.Latitude - origin.Latitude) * (b.Longitude - origin.Longitude);

    private static bool IsOnSegment(GeoCoordinate start, GeoCoordinate end, GeoCoordinate point) =>
        point.Longitude >= Math.Min(start.Longitude, end.Longitude) && point.Longitude <= Math.Max(start.Longitude, end.Longitude) &&
        point.Latitude >= Math.Min(start.Latitude, end.Latitude) && point.Latitude <= Math.Max(start.Latitude, end.Latitude);

    private static double SignedArea(IReadOnlyList<GeoCoordinate> vertices)
    {
        var sum = 0d;
        for (var i = 0; i < vertices.Count; i++)
        {
            var current = vertices[i];
            var next = vertices[(i + 1) % vertices.Count];
            sum += current.Longitude * next.Latitude - next.Longitude * current.Latitude;
        }

        return sum / 2;
    }
}
