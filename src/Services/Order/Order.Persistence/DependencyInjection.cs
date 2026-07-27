using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Extensions;
using BuildingBlock.Persistence.Audit;
using BuildingBlock.Persistence.Ef.DependencyInjection;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;
using BuildingBlock.Persistence.Repository;
using BuildingBlock.Saga.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using Order.Application.Abstractions.Persistence.OrderProductCatalogs;
using Order.Application.Abstractions.Persistence.Orders;
using Order.Persistence.Engine;
using Order.Persistence.Engine.UnitOfWork;
using Order.Persistence.Inbox;
using Order.Persistence.Contexts.OrderProductCatalogs.Read;
using Order.Persistence.Contexts.OrderProductCatalogs.Write;
using Order.Persistence.Contexts.Orders.Read;
using Order.Persistence.Contexts.Orders.Write;
using Order.Persistence.Outbox;
using Order.Persistence.Saga;

namespace Order.Persistence;

public static class DependencyInjection
{
    public static TracerProviderBuilder AddPersistenceTracing(this TracerProviderBuilder builder)
    {
        return builder.AddNpgsql();
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddApplicationServices()
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox()
            .AddAuditHierarchy()
            .AddSagaStore();

        return services;
    }

    // Singleton to match SagaOrchestrator's own lifetime (BuildingBlock.Saga.Extensions.SagaExtensions)
    // - EfSagaStore opens its own DI scope per call rather than taking OrderDbContext directly, see
    // its remarks.
    private static IServiceCollection AddSagaStore(this IServiceCollection services)
    {
        services.AddSingleton<ISagaStore, EfSagaStore>();
        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<OrderDbContext>(connectionString);

        return services;
    }

    // Order aggregate: Order is the root, OrderItem/OrderOwner belong to it via OrderId.
    // OrderProductCatalog is a separate local read-model (kept in sync from Product's events),
    // not part of this aggregate, so it isn't registered here.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<OrderEntity>().IsRoot(x => x.Id);
            builder.Entity<OrderItem>()
                .BelongsTo<OrderEntity>(x => x.OrderId);
            builder.Entity<OrderOwner>()
                .BelongsTo<OrderEntity>(x => x.OrderId);
        });

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(OrderDbContext));
        return services;
    }

    // OrderRepo/OrderProductCatalogRepo both implement the generic IRepository<T> - Scrutor's
    // AsImplementedInterfaces() registers each concrete class against every interface it
    // implements (including the empty per-aggregate marker interfaces), so this one scan call
    // covers both. Read/Write services are registered explicitly since they're one-per-aggregate.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(OrderDbContext));

        services.AddScoped<IOrderReadService, OrderReadService>();
        services.AddScoped<IOrderWriteService, OrderWriteService>();

        services.AddScoped<IOrderProductCatalogReadService, OrderProductCatalogReadService>();
        services.AddScoped<IOrderProductCatalogWriteService, OrderProductCatalogWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutboxAndInbox(this IServiceCollection services)
    {
        services
            .AddEfOutboxStore<OrderDbContext>()
            .AddEfInboxStore<OrderDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
