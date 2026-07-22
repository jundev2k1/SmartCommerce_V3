using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Extensions;
using BuildingBlock.Persistence.Audit;
using BuildingBlock.Persistence.Ef.DependencyInjection;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;
using BuildingBlock.Persistence.Repository;

using Inventory.Application.Abstractions.Repositories;
using Inventory.Persistence.Inbox;
using Inventory.Persistence.Outbox;
using Inventory.Persistence.Repository;

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

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(InventoryDbContext));

        // InventoryRepo gained Search/rollup methods beyond generic CRUD (GetTotalStockBy...,
        // DeleteByProductIdAsync) and no longer implements IRepository<T>, so the Scrutor scan
        // above never finds it either - same reasoning as InventoryTransactionRepo below.
        services.AddScoped<IInventoryRepository, InventoryRepo>();
        services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepo>();
        services.AddScoped<IStockDeductionRepository, StockDeductionRepo>();

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
