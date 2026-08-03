using SmartEcommerce.BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;
using SmartEcommerce.BuildingBlock.Infrastructure.Extensions;
using SmartEcommerce.BuildingBlock.Infrastructure.Messaging;
using SmartEcommerce.BuildingBlock.Messaging.Abstractions;
using SmartEcommerce.BuildingBlock.Messaging.Kafka.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SmartEcommerce.Notification.Application.Abstractions.Services;
using SmartEcommerce.Notification.Infrastructure.BackgroundJobs;
using SmartEcommerce.Notification.Infrastructure.Caching;
using SmartEcommerce.Notification.Infrastructure.Delivery;
using SmartEcommerce.Notification.Infrastructure.Messaging.Consumers;
using SmartEcommerce.Notification.Infrastructure.SignalR.Facade;
using SmartEcommerce.Notification.Infrastructure.SignalR.Hubs.Global;
using SmartEcommerce.Notification.Infrastructure.Workers;

namespace SmartEcommerce.Notification.Infrastructure;

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
