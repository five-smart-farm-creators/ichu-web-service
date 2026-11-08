using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.OperationsMonitoring.Domain;

public class GeofenceTests
{
    private static readonly Guid LotId = Guid.Parse("33333333-3333-4333-8333-333333333333");

    [Fact]
    public void SquareBoundaryIsAcceptedWithItsNameTrimmed()
    {
        var geofence = Create(Square(), name: "  North paddock  ");

        Assert.True(geofence.IsSuccess);
        Assert.Equal("North paddock", geofence.Value!.Name);
        Assert.Equal(4, geofence.Value.Vertices.Count);
    }

    [Fact]
    public void BoundaryThatRepeatsItsFirstVertexAtTheEndKeepsEachVertexOnce()
    {
        var boundary = Square().Append(Square()[0]).ToList();

        var geofence = Create(boundary);

        Assert.Equal(4, geofence.Value!.Vertices.Count);
    }

    [Fact]
    public void BoundaryWithFewerThanThreeVerticesIsRejected()
    {
        var geofence = Create([Point(0, 0), Point(0, 1)]);

        Assert.Equal(OperationsMonitoringErrors.GeofenceVertexCount.Code, geofence.DomainError.Code);
    }

    [Fact]
    public void BoundaryThatRepeatsAVertexIsRejected()
    {
        var geofence = Create([Point(0, 0), Point(0, 1), Point(0, 1), Point(1, 0)]);

        Assert.Equal(OperationsMonitoringErrors.GeofenceRepeatedVertices.Code, geofence.DomainError.Code);
    }

    [Fact]
    public void BoundaryWithZeroAreaIsRejected()
    {
        var geofence = Create([Point(0, 0), Point(0, 1), Point(0, 2)]);

        Assert.Equal(OperationsMonitoringErrors.GeofenceNotEnclosing.Code, geofence.DomainError.Code);
    }

    [Fact]
    public void SelfCrossingBoundaryIsRejected()
    {
        var geofence = Create([Point(0, 0), Point(2, 2), Point(0, 2), Point(3, 0)]);

        Assert.Equal(OperationsMonitoringErrors.GeofenceBoundaryCrossing.Code, geofence.DomainError.Code);
    }

    [Fact]
    public void BlankNameAndOutOfRangeToleranceAreRejected()
    {
        Assert.Equal(OperationsMonitoringErrors.GeofenceNameLength.Code, Create(Square(), name: "   ").DomainError.Code);
        Assert.Equal(OperationsMonitoringErrors.GeofenceToleranceRange.Code, Create(Square(), tolerance: 0).DomainError.Code);
    }

    [Fact]
    public void RejectedRedefinitionKeepsThePreviousDefinition()
    {
        var geofence = Create(Square()).Value!;

        var redefinition = geofence.Redefine("Renamed", 15, AlertSeverity.Warning,
            [Point(0, 0), Point(2, 2), Point(0, 2), Point(3, 0)], 2);

        Assert.True(redefinition.IsFailure);
        Assert.Equal("North paddock", geofence.Name);
        Assert.Equal(4, geofence.Vertices.Count);
    }

    private static Result<Geofence> Create(IReadOnlyList<GeoCoordinate> boundary, string name = "North paddock", int tolerance = 15) =>
        Geofence.Create(TelemetryFixtures.RanchId, LotId, name, tolerance, AlertSeverity.Warning, boundary, 1);

    private static List<GeoCoordinate> Square() => [Point(0, 0), Point(0, 1), Point(1, 1), Point(1, 0)];

    private static GeoCoordinate Point(double latitude, double longitude) => GeoCoordinate.Create(latitude, longitude).Value!;
}
