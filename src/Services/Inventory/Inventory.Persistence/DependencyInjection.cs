using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Extensions;
using SmartEcommerce.BuildingBlock.Persistence.Audit;
using SmartEcommerce.BuildingBlock.Persistence.Ef.DependencyInjection;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Inbox;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Outbox;
using SmartEcommerce.BuildingBlock.Persistence.Repository;

using SmartEcommerce.Inventory.Application.Abstractions.Persistence.Inventories;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryCounts;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryLots;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryReservations;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventorySerials;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.Warehouses;
using SmartEcommerce.Inventory.Persistence.Contexts.Inventories.Read;
using SmartEcommerce.Inventory.Persistence.Contexts.Inventories.Write;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryCounts.Read;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryCounts.Repositories;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryCounts.Write;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryDocuments.Read;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryDocuments.Repositories;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryDocuments.Write;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryLots.Read;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryLots.Repositories;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryLots.Write;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryReservations.Read;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryReservations.Repositories;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryReservations.Write;
using SmartEcommerce.Inventory.Persistence.Contexts.InventorySerials.Read;
using SmartEcommerce.Inventory.Persistence.Contexts.InventorySerials.Repositories;
using SmartEcommerce.Inventory.Persistence.Contexts.InventorySerials.Write;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryTransactions.Read;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryTransactions.Repositories;
using SmartEcommerce.Inventory.Persistence.Contexts.InventoryTransactions.Write;
using SmartEcommerce.Inventory.Persistence.Contexts.Warehouses.Read;
using SmartEcommerce.Inventory.Persistence.Contexts.Warehouses.Write;
using SmartEcommerce.Inventory.Persistence.Engine;
using SmartEcommerce.Inventory.Persistence.Engine.UnitOfWork;
using SmartEcommerce.Inventory.Persistence.Reliability.Inbox;
using SmartEcommerce.Inventory.Persistence.Reliability.Outbox;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

namespace SmartEcommerce.Inventory.Persistence;

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
            .AddAuditHierarchy();

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<InventoryDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(InventoryDbContext));
        return services;
    }

    // Repositories are Scrutor-scanned via AddScopedByInterface for generic IRepository<>.
    // Specialized repositories and services are manually registered.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(InventoryDbContext));

        // Inventory
        services.AddScoped<IInventoryReadService, InventoryReadService>();
        services.AddScoped<IInventoryWriteService, InventoryWriteService>();

        // Warehouse
        services.AddScoped<IWarehouseReadService, WarehouseReadService>();
        services.AddScoped<IWarehouseWriteService, WarehouseWriteService>();

        // InventoryTransaction
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
        services.AddScoped<IInventoryTransactionReadService, InventoryTransactionReadService>();
        services.AddScoped<IInventoryTransactionWriteService, InventoryTransactionWriteService>();

        // InventoryLot
        services.AddScoped<IInventoryLotRepository, InventoryLotRepository>();
        services.AddScoped<IInventoryLotReadService, InventoryLotReadService>();
        services.AddScoped<IInventoryLotWriteService, InventoryLotWriteService>();

        // InventoryReservation
        services.AddScoped<IInventoryReservationRepository, InventoryReservationRepository>();
        services.AddScoped<IInventoryReservationReadService, InventoryReservationReadService>();
        services.AddScoped<IInventoryReservationWriteService, InventoryReservationWriteService>();

        // InventorySerial
        services.AddScoped<IInventorySerialRepository, InventorySerialRepository>();
        services.AddScoped<IInventorySerialReadService, InventorySerialReadService>();
        services.AddScoped<IInventorySerialWriteService, InventorySerialWriteService>();

        // InventoryCount
        services.AddScoped<IInventoryCountRepository, InventoryCountRepository>();
        services.AddScoped<IInventoryCountReadService, InventoryCountReadService>();
        services.AddScoped<IInventoryCountWriteService, InventoryCountWriteService>();

        // InventoryDocument
        services.AddScoped<IInventoryDocumentRepository, InventoryDocumentRepository>();
        services.AddScoped<IInventoryDocumentReadService, InventoryDocumentReadService>();
        services.AddScoped<IInventoryDocumentWriteService, InventoryDocumentWriteService>();

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
            .AddEfOutboxStore<InventoryDbContext>()
            .AddEfInboxStore<InventoryDbContext>()
            .AddEfDeadLetterQueryService<InventoryDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }

    // InventoryStock (root), Warehouse (independent root - a physical location, not owned by
    // InventoryStock), InventoryTransaction (append-only log entry, belongs to the InventoryStock record
    // it happened against).
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<InventoryStock>().IsRoot(x => x.Id);
            builder.Entity<InventoryTransaction>()
                .BelongsTo<InventoryStock>(x => x.InventoryId);

            builder.Entity<Warehouse>().IsRoot(x => x.Id);
        });

        return services;
    }
}
