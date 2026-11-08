using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

/// <summary>
///     The permitted grazing area of one lot. Its boundary is a closed, simple polygon, and positions whose declared accuracy exceeds the tolerance are not conclusive for the geofence.
/// </summary>
public sealed class Geofence : AggregateRoot
{
    public const int MaximumNameLength = 80;

    public const int MaximumVertices = 500;

    public const int MaximumToleranceMeters = short.MaxValue;

    private readonly List<GeofenceVertex> _vertices = [];

    // Entity Framework Core materializes geofences without running the factory.
    private Geofence()
    {
    }

    public GeofenceId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public Guid LotId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int ToleranceMeters { get; private set; }

    public AlertSeverity Severity { get; private set; }

    public long Revision { get; private set; }

    public IReadOnlyCollection<GeofenceVertex> Vertices => _vertices;

    public static Result<Geofence> Create(
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
            return Result<Geofence>.Failure(OperationsMonitoringErrors.GeofenceIdentifiersRequired);
        }

        var geofence = new Geofence { Id = new GeofenceId(Guid.NewGuid()), RanchId = ranchId, LotId = lotId };
        var definition = geofence.Redefine(name, toleranceMeters, severity, boundary, revision);
        return definition.IsFailure
            ? Result<Geofence>.Failure(definition.DomainError)
            : Result<Geofence>.Success(geofence);
    }

    public Result Redefine(string name, int toleranceMeters, AlertSeverity severity, IReadOnlyList<GeoCoordinate> boundary, long revision)
    {
        var normalizedName = name.Trim();
        if (normalizedName.Length is 0 or > MaximumNameLength)
        {
            return Result.Failure(OperationsMonitoringErrors.GeofenceNameLength);
        }

        if (toleranceMeters is < 1 or > MaximumToleranceMeters)
        {
            return Result.Failure(OperationsMonitoringErrors.GeofenceToleranceRange);
        }

        var ring = OpenRing(boundary);
        if (ring.IsFailure)
        {
            return Result.Failure(ring.DomainError);
        }

        var simplicity = EnsureSimplePolygon(ring.Value!);
        if (simplicity.IsFailure)
        {
            return simplicity;
        }

        Name = normalizedName;
        ToleranceMeters = toleranceMeters;
        Severity = severity;
        Revision = revision;
        _vertices.Clear();
        _vertices.AddRange(ring.Value!.Select((vertex, index) => new GeofenceVertex(index, vertex.Latitude, vertex.Longitude)));
        return Result.Success();
    }

    public IReadOnlyList<GeofenceVertex> Boundary() => _vertices.OrderBy(v => v.Sequence).ToList();

    // A boundary may repeat its first vertex at the end; the stored ring keeps each vertex once.
    private static Result<IReadOnlyList<GeoCoordinate>> OpenRing(IReadOnlyList<GeoCoordinate> boundary)
    {
        var vertices = boundary.ToList();
        if (vertices.Count > 1 && vertices[0] == vertices[^1])
        {
            vertices.RemoveAt(vertices.Count - 1);
        }

        if (vertices.Count is < 3 or > MaximumVertices)
        {
            return Result<IReadOnlyList<GeoCoordinate>>.Failure(OperationsMonitoringErrors.GeofenceVertexCount);
        }

        if (vertices.Distinct().Count() != vertices.Count)
        {
            return Result<IReadOnlyList<GeoCoordinate>>.Failure(OperationsMonitoringErrors.GeofenceRepeatedVertices);
        }

        return Result<IReadOnlyList<GeoCoordinate>>.Success(vertices);
    }

    // A planar test on longitude and latitude is enough for grazing areas to reject crossing, overlapping or zero-area boundaries.
    private static Result EnsureSimplePolygon(IReadOnlyList<GeoCoordinate> vertices)
    {
        var count = vertices.Count;
        if (Math.Abs(SignedArea(vertices)) < double.Epsilon)
        {
            return Result.Failure(OperationsMonitoringErrors.GeofenceNotEnclosing);
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
                    return Result.Failure(OperationsMonitoringErrors.GeofenceBoundaryCrossing);
                }
            }
        }

        return Result.Success();
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
