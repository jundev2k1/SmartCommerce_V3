using Audit.Application.Abstractions.Repositories;
using Audit.Persistence.Inbox;
using Audit.Persistence.Outbox;
using Audit.Persistence.Repository;

using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Persistence.Mongo.DependencyInjection;
using BuildingBlock.Persistence.Mongo.Inbox;
using BuildingBlock.Persistence.Mongo.Outbox;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox();

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Mongo")
            ?? throw new InvalidOperationException("Connection string 'Mongo' not found.");
        var databaseName = configuration["Mongo:DatabaseName"]
            ?? throw new InvalidOperationException("Configuration 'Mongo:DatabaseName' not found.");

        services.AddPersistenceMongoContext<AuditMongoContext>(connectionString, databaseName);

        return services;
    }

    // Audit logs are append-only with a single custom read shape (SearchAsync), so there is
    // no generic IRepository<T> to Scrutor-scan for here - same reasoning as Inventory's
    // IInventoryTransactionRepository, registered explicitly rather than discovered.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAuditLogRepository, AuditLogRepo>();
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
            .AddMongoOutboxStore<AuditMongoContext>()
            .AddMongoInboxStore<AuditMongoContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
