using OpenTelemetry.Trace;

namespace SmartEcommerce.BuildingBlock.Messaging.Kafka.Tracing;

public static class KafkaTracingExtensions
{
    public static TracerProviderBuilder AddKafkaMessagingTracing(this TracerProviderBuilder builder)
    {
        return builder.AddSource(KafkaTracing.ActivitySourceName);
    }
}
