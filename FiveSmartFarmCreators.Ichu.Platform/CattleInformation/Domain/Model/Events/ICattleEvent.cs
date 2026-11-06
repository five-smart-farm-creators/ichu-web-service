namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Events;

public interface ICattleEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
