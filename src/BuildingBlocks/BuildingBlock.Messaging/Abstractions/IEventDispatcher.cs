using BuildingBlock.Contract.Events;

namespace BuildingBlock.Messaging.Abstractions;

public interface IEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent;
}
