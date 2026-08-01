using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Extensions;
using BuildingBlock.Persistence.Audit;
using BuildingBlock.Persistence.Ef.DependencyInjection;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;
using BuildingBlock.Persistence.Repository;

using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.InventoryCounts;
using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using Inventory.Application.Abstractions.Persistence.InventoryLots;
using Inventory.Application.Abstractions.Persistence.InventoryReservations;
using Inventory.Application.Abstractions.Persistence.InventorySerials;
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Persistence.Contexts.Inventories.Read;
using Inventory.Persistence.Contexts.Inventories.Write;
using Inventory.Persistence.Contexts.InventoryCounts.Read;
using Inventory.Persistence.Contexts.InventoryCounts.Repositories;
using Inventory.Persistence.Contexts.InventoryCounts.Write;
using Inventory.Persistence.Contexts.InventoryDocuments.Read;
using Inventory.Persistence.Contexts.InventoryDocuments.Repositories;
using Inventory.Persistence.Contexts.InventoryDocuments.Write;
using Inventory.Persistence.Contexts.InventoryLots.Read;
using Inventory.Persistence.Contexts.InventoryLots.Repositories;
using Inventory.Persistence.Contexts.InventoryLots.Write;
using Inventory.Persistence.Contexts.InventoryReservations.Read;
using Inventory.Persistence.Contexts.InventoryReservations.Repositories;
using Inventory.Persistence.Contexts.InventoryReservations.Write;
using Inventory.Persistence.Contexts.InventorySerials.Read;
using Inventory.Persistence.Contexts.InventorySerials.Repositories;
using Inventory.Persistence.Contexts.InventorySerials.Write;
using Inventory.Persistence.Contexts.InventoryTransactions.Read;
using Inventory.Persistence.Contexts.InventoryTransactions.Repositories;
using Inventory.Persistence.Contexts.InventoryTransactions.Write;
using Inventory.Persistence.Contexts.Warehouses.Read;
using Inventory.Persistence.Contexts.Warehouses.Write;
using Inventory.Persistence.Engine;
using Inventory.Persistence.Engine.UnitOfWork;
using Inventory.Persistence.Reliability.Inbox;
using Inventory.Persistence.Reliability.Outbox;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

namespace Inventory.Persistence;

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
