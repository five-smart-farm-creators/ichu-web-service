using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;

/// <summary>Records collar releases; the other commands are not used by the tests that rely on it.</summary>
public sealed class RecordingDeviceCommandService : IDeviceCommandService
{
    public List<ReleaseCollarForInactiveCattleCommand> ReleaseCommands { get; } = [];

    public Task<Result<int>> HandleAsync(ReleaseCollarForInactiveCattleCommand command, CancellationToken cancellationToken)
    {
        ReleaseCommands.Add(command);
        return Task.FromResult(Result<int>.Success(1));
    }

    public Task<Result<IoTDevice>> HandleAsync(RegisterDeviceCommand command, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The test only records collar releases.");

    public Task<Result<IoTDevice>> HandleAsync(AssignDeviceCommand command, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The test only records collar releases.");

    public Task<Result<IoTDevice>> HandleAsync(ReleaseDeviceCommand command, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The test only records collar releases.");

    public Task<Result<IoTDevice>> HandleAsync(SetDeviceMaintenanceCommand command, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The test only records collar releases.");

    public Task<Result<IoTDevice>> HandleAsync(RegisterDeviceCommunicationCommand command, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The test only records collar releases.");
}
