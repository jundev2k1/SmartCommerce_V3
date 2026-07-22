using System.Text.Json;

using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Messaging.Abstractions;
using BuildingBlock.Messaging.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlock.Infrastructure.Messaging;

/// <summary>
/// Extensions for registering Inbox/Outbox background infrastructure.
/// Registers the OutboxRelayHostedService/InboxRetryHostedService and wires up the Inbox
/// dedup/retry delegate consumed by IntegrationEventConsumerRegistry.
/// </summary>
public static class MessagingInfrastructureExtensions
{
    /// <summary>
    /// Registers the Outbox Relay + Inbox Retry hosted services and Inbox dedup/retry support.
    /// Must be called BEFORE AddKafkaMessaging; it overrides the placeholder delegate with the
    /// real Inbox implementation.
    /// </summary>
    public static IServiceCollection AddInboxOutboxInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .Configure<OutboxRelayOptions>(configuration.GetSection(OutboxRelayOptions.Section));
        services
            .Configure<InboxRetryOptions>(configuration.GetSection(InboxRetryOptions.Section));

        services.AddScoped<InboxAttemptExecutor>();

        // Replace the placeholder delegate with the real Inbox implementation.
        // MUST be registered before AddKafkaMessaging's DiscoverConsumerTopics call,
        // otherwise the temp provider won't find it.
        services.AddScoped(BuildInboxExecutionDelegate);

        // Register Outbox relay + Inbox retry background services
        services.AddSingleton<IHostedService, OutboxRelayHostedService>();
        services.AddSingleton<IHostedService, InboxRetryHostedService>();

        return services;
    }

    /// <summary>
    /// Builds the executeWithInboxAsync delegate for IntegrationEventConsumerRegistry. This
    /// enables generic Inbox dedup/retry tracking without the Messaging project needing to
    /// depend on Application or Persistence.
    /// </summary>
    private static Func<InboxDispatchContext, Func<Task>, CancellationToken, Task> BuildInboxExecutionDelegate(
        IServiceProvider provider)
    {
        return async (dispatchContext, handlerAction, ct) =>
        {
            var inboxStore = provider.GetRequiredService<IInboxStore>();
            var executor = provider.GetRequiredService<InboxAttemptExecutor>();
            var headersJson = JsonSerializer.Serialize(dispatchContext.Headers);

            await executor.ExecuteAsync(
                inboxStore,
                dispatchContext.MessageId,
                dispatchContext.ConsumerName,
                dispatchContext.Topic,
                dispatchContext.Payload,
                headersJson,
                handlerAction,
                ct);
        };
    }
}
