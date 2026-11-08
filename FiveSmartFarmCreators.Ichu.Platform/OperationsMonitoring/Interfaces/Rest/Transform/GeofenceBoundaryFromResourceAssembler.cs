using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class GeofenceBoundaryFromResourceAssembler
{
    public static Result<IReadOnlyList<GeoCoordinate>> ToBoundary(IReadOnlyList<CoordinateResource>? vertices)
    {
        var boundary = new List<GeoCoordinate>();
        foreach (var vertex in vertices ?? [])
        {
            if (vertex.Latitude is not { } latitude || vertex.Longitude is not { } longitude)
            {
                return Result<IReadOnlyList<GeoCoordinate>>.Failure(OperationsMonitoringErrors.VertexCoordinatesRequired);
            }

            var coordinate = GeoCoordinate.Create(latitude, longitude);
            if (coordinate.IsFailure)
            {
                return Result<IReadOnlyList<GeoCoordinate>>.Failure(coordinate.DomainError);
            }

            boundary.Add(coordinate.Value!);
        }

        return Result<IReadOnlyList<GeoCoordinate>>.Success(boundary);
    }
}
