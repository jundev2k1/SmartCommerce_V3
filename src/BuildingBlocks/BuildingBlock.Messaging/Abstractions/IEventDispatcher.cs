using SmartEcommerce.BuildingBlock.Contract.Events;

namespace SmartEcommerce.BuildingBlock.Messaging.Abstractions;

public interface IEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent;
}
