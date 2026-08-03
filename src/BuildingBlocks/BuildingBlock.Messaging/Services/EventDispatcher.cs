using SmartEcommerce.BuildingBlock.Messaging.Abstractions;
using SmartEcommerce.BuildingBlock.Contract.Events;

namespace SmartEcommerce.BuildingBlock.Messaging.Services;

public sealed class EventDispatcher(IEventPublisher eventPublisher) : IEventDispatcher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent
    {
        await eventPublisher.PublishAsync(@event, ct);
    }
}
