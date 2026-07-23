using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Extensions;
using BuildingBlock.Persistence.Audit;
using BuildingBlock.Persistence.Ef.DependencyInjection;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;

using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Application.Abstractions.Persistence.StockDeductions;
using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Persistence.Inbox;
using Inventory.Persistence.InventoryTransactions.Read;
using Inventory.Persistence.InventoryTransactions.Repositories;
using Inventory.Persistence.InventoryTransactions.Write;
using Inventory.Persistence.Inventories.Read;
using Inventory.Persistence.Inventories.Repositories;
using Inventory.Persistence.Inventories.Write;
using Inventory.Persistence.Outbox;
using Inventory.Persistence.StockDeductions.Read;
using Inventory.Persistence.StockDeductions.Repositories;
using Inventory.Persistence.StockDeductions.Write;
using Inventory.Persistence.Warehouses.Read;
using Inventory.Persistence.Warehouses.Repositories;
using Inventory.Persistence.Warehouses.Write;

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

    // None of these repos implement the generic IRepository<T> - each aggregate's Read/Write
    // Service (see docs/refactoring/persistence-refactor-plan.md) owns workflow/query composition
    // instead, so the repos themselves are pure Add/Delete-by-filter primitives with no shape a
    // generic Scrutor scan could match. All three tiers are registered explicitly per aggregate.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IInventoryRepository, InventoryRepo>();
        services.AddScoped<IInventoryReadService, InventoryReadService>();
        services.AddScoped<IInventoryWriteService, InventoryWriteService>();

        services.AddScoped<IWarehouseRepository, WarehouseRepo>();
        services.AddScoped<IWarehouseReadService, WarehouseReadService>();
        services.AddScoped<IWarehouseWriteService, WarehouseWriteService>();

        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepo>();
        services.AddScoped<IInventoryTransactionReadService, InventoryTransactionReadService>();
        services.AddScoped<IInventoryTransactionWriteService, InventoryTransactionWriteService>();

        services.AddScoped<IStockDeductionRepository, StockDeductionRepo>();
        services.AddScoped<IStockDeductionReadService, StockDeductionReadService>();
        services.AddScoped<IStockDeductionWriteService, StockDeductionWriteService>();

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
