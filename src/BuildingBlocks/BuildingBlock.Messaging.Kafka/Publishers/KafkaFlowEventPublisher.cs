using System.Text;
using System.Text.Json;

using BuildingBlock.Contract.Events;
using BuildingBlock.Messaging.Abstractions;
using BuildingBlock.Messaging.Kafka.Configuration;

using KafkaFlow;
using KafkaFlow.Producers;

namespace BuildingBlock.Messaging.Kafka.Publishers;

public sealed class KafkaFlowEventPublisher(
    IProducerAccessor producerAccessor) : IEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IIntegrationEvent
    {
        var producer = producerAccessor.GetProducer(KafkaFlowDefaults.ProducerName);
        var topic = GenerateTopicName(@event);
        var value = JsonSerializer.SerializeToUtf8Bytes(@event);

        var headers = new MessageHeaders
        {
            { "event-type", Encoding.UTF8.GetBytes(typeof(TEvent).Name) },
            { "correlation-id", Encoding.UTF8.GetBytes(@event.CorrelationId) },
        };

        await producer.ProduceAsync(topic, @event.CorrelationId, value, headers);
    }

    public async Task PublishBatchAsync<TEvent>(
        IEnumerable<TEvent> events,
        CancellationToken ct = default) where TEvent : class, IIntegrationEvent
    {
        foreach (var @event in events)
        {
            await PublishAsync(@event, ct);
        }
    }

    private static string GenerateTopicName<TEvent>(TEvent @event) where TEvent : IIntegrationEvent
    {
        return @event.EventType.ToLowerInvariant();
    }
}
