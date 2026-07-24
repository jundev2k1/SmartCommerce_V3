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
using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.StockDeductions;
using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Persistence.Contexts.Inventories.Read;
using Inventory.Persistence.Contexts.Inventories.Repositories;
using Inventory.Persistence.Contexts.Inventories.Write;
using Inventory.Persistence.Contexts.InventoryTransactions.Read;
using Inventory.Persistence.Contexts.InventoryTransactions.Repositories;
using Inventory.Persistence.Contexts.InventoryTransactions.Write;
using Inventory.Persistence.Contexts.StockDeductions.Read;
using Inventory.Persistence.Contexts.StockDeductions.Repositories;
using Inventory.Persistence.Contexts.StockDeductions.Write;
using Inventory.Persistence.Contexts.Warehouses.Read;
using Inventory.Persistence.Contexts.Warehouses.Repositories;
using Inventory.Persistence.Contexts.Warehouses.Write;
using Inventory.Persistence.Engine;
using Inventory.Persistence.Engine.UnitOfWork;
using Inventory.Persistence.Inbox;
using Inventory.Persistence.Outbox;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Persistence;

public static class DependencyInjection
{
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

    // InventoryRepo/WarehouseRepo/StockDeductionRepo implement the generic IRepository<T> again
    // (restored per the persistence refactor's Course Correction - see the tracker) and are
    // Scrutor-scanned. InventoryTransactionRepo doesn't (append-only - AddAsync is its only real
    // operation, never updated/deleted/tracked-loaded), so it stays manually registered, same
    // reasoning as Audit's AuditLogRepo.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(InventoryDbContext));

        services.AddScoped<IInventoryReadService, InventoryReadService>();
        services.AddScoped<IInventoryWriteService, InventoryWriteService>();

        services.AddScoped<IWarehouseReadService, WarehouseReadService>();
        services.AddScoped<IWarehouseWriteService, WarehouseWriteService>();

        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepo>();
        services.AddScoped<IInventoryTransactionReadService, InventoryTransactionReadService>();
        services.AddScoped<IInventoryTransactionWriteService, InventoryTransactionWriteService>();

        services.AddScoped<IStockDeductionReadService, StockDeductionReadService>();
        services.AddScoped<IStockDeductionWriteService, StockDeductionWriteService>();

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
            .AddEfInboxStore<InventoryDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }

    // Inventory (root), Warehouse (independent root - a physical location, not owned by
    // Inventory), InventoryTransaction (append-only log entry, belongs to the Inventory record
    // it happened against).
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<InventoryEntity>().IsRoot(x => x.Id);
            builder.Entity<InventoryTransaction>()
                .BelongsTo<InventoryEntity>(x => x.InventoryId);

            builder.Entity<Warehouse>().IsRoot(x => x.Id);
        });

        return services;
    }
}
