using Notification.Application.Abstractions.Repositories;
using Notification.Domain.ValueObjects;
using Notification.Persistence.Inbox;
using Notification.Persistence.Outbox;
using Notification.Persistence.Repository;

using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Persistence.Mongo.DependencyInjection;
using BuildingBlock.Persistence.Mongo.Inbox;
using BuildingBlock.Persistence.Mongo.Outbox;
using BuildingBlock.Persistence.Mongo.Serialization;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notification.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RegisterValueObjectClassMaps();

        services
            .AddDatabaseContext(configuration)
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox();

        return services;
    }

    /// <summary>
    /// Every get-only-property value object in Notification.Domain needs an explicit BsonClassMap
    /// or it silently round-trips as an empty subdocument - see BsonImmutableValueObjectRegistrar
    /// and docs/tasks/2026-07-22/Task2_notification-list-null-fields.md for why.
    /// </summary>
    private static void RegisterValueObjectClassMaps()
    {
        BsonImmutableValueObjectRegistrar.Register<NotificationCategory>("Value");
        BsonImmutableValueObjectRegistrar.Register<NotificationType>("Value");
        BsonImmutableValueObjectRegistrar.Register<NotificationContent>("Title", "Body");
        BsonImmutableValueObjectRegistrar.Register<AudienceSelector>("Type", "ConfigJson");
        BsonImmutableValueObjectRegistrar.Register<ChannelConfiguration>("ConfigJson");
        BsonImmutableValueObjectRegistrar.Register<DispatchReference>("ReferenceType", "ReferenceId");
        BsonImmutableValueObjectRegistrar.Register<NotificationSchedule>("ExecutionType", "StartAt", "EndAt", "CronExpression");
        BsonImmutableValueObjectRegistrar.Register<TemplateContent>("Subject", "Body", "Variables");
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mongo")
            ?? throw new InvalidOperationException("Connection string 'Mongo' not found.");
        var databaseName = configuration["Mongo:DatabaseName"]
            ?? throw new InvalidOperationException("Configuration 'Mongo:DatabaseName' not found.");

        services.AddPersistenceMongoContext<NotificationMongoContext>(connectionString, databaseName);

        return services;
    }

    // No generic IRepository<T> to Scrutor-scan for here - Mongo repos have no DbContext/
    // IQueryable shape to satisfy it, same reasoning as Audit's IAuditLogRepository. Registered
    // explicitly rather than discovered, one per aggregate root (Repository Design: repositories
    // are designed only around aggregate roots, never per child entity).
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserNotificationRepository, UserNotificationRepo>();
        services.AddScoped<INotificationGroupRepository, NotificationGroupRepo>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepo>();
        services.AddScoped<INotificationRuleRepository, NotificationRuleRepo>();
        services.AddScoped<INotificationCampaignRepository, NotificationCampaignRepo>();
        services.AddScoped<INotificationChannelRepository, NotificationChannelRepo>();
        services.AddScoped<INotificationDispatchRepository, NotificationDispatchRepo>();
        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutboxAndInbox(this IServiceCollection services)
    {
        services
            .AddMongoOutboxStore<NotificationMongoContext>()
            .AddMongoInboxStore<NotificationMongoContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
