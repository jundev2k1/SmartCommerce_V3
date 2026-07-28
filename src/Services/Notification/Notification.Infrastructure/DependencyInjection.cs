using BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using BuildingBlock.Infrastructure.Extensions;
using BuildingBlock.Infrastructure.Messaging;
using BuildingBlock.Messaging.Abstractions;
using BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Notification.Application.Abstractions.Services;
using Notification.Infrastructure.BackgroundJobs;
using Notification.Infrastructure.Caching;
using Notification.Infrastructure.Delivery;
using Notification.Infrastructure.Messaging.Consumers;
using Notification.Infrastructure.SignalR.Facade;
using Notification.Infrastructure.SignalR.Hubs.Global;
using Notification.Infrastructure.Workers;

namespace Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAppLogger()
            .AddBackgroundJobs(configuration)
            .AddInboxOutboxCleanupJobs(configuration)
            .AddDispatchWorker(configuration);

        // Register application event dispatcher (MediatR - for internal events)
        services.AddApplicationEventDispatcher();

        services.AddMessagingConsumers();
        services.AddKafkaMessaging(configuration, "notification-service");
        services.AddInboxOutboxInfrastructure(configuration);
        services.AddNotificationDelivery();
        services.AddNotificationChannelCache();

        return services;
    }

    private static IServiceCollection AddDispatchWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NotificationDispatchWorkerOptions>(configuration.GetSection(NotificationDispatchWorkerOptions.Section));
        return services;
    }

    private static IServiceCollection AddMessagingConsumers(this IServiceCollection services)
    {
        services.AddScoped<IIntegrationEventConsumer, NotificationTriggerConsumer>();
        return services;
    }

    private static IServiceCollection AddNotificationDelivery(this IServiceCollection services)
    {
        services.AddScoped<ActorHubFacade<GlobalHub, IGlobalHubClient, IGlobalHubClient>>();
        services.AddScoped<IChannelSender, SignalRChannelSender>();
        services.AddScoped<IChannelSenderResolver, ChannelSenderResolver>();
        services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();
        return services;
    }

    private static IServiceCollection AddNotificationChannelCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<INotificationChannelCache, NotificationChannelCache>();
        return services;
    }
}
