using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;
using Cortex.Mediator.Notifications;

namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Internal.EventHandlers;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
}