using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record MobileSyncBatchResource(IReadOnlyList<RegisterLivestockOperationResource>? Operations)
{
    public IReadOnlyList<RegisterLivestockOperationCommand> ToCommands() =>
        (Operations ?? []).Select(o => o.ToCommand()).ToList();
}
