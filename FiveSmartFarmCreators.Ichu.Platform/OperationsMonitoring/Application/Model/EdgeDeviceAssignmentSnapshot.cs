using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     The collar assignments that an Edge Gateway applies, with the ranch that the gateway serves.
/// </summary>
public record EdgeDeviceAssignmentSnapshot(Guid RanchId, IReadOnlyList<EdgeDeviceAssignment> Assignments);
