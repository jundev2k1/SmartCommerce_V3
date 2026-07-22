using BuildingBlock.Contract.Events;

namespace BuildingBlock.Messaging.Abstractions;

public interface IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}
