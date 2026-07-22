using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Extensions;
using BuildingBlock.Persistence.Audit;
using BuildingBlock.Persistence.Ef.DependencyInjection;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Product.Application.Abstractions.Repositories;
using Product.Persistence.Outbox;

namespace Product.Persistence;

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
            .AddOutbox()
            .AddInbox()
            .AddAuditHierarchy();

        return services;
    }

    // Product, ProductCategory and ProductTag are independent aggregates - Product references
    // category/tag ids but doesn't own them, so each is its own root. ProductVariation is an
    // owned child of Product (no independent identity), so it is not registered separately -
    // its changes are already part of Product's own audit snapshot.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<ProductEntity>().IsRoot(x => x.Id);
            builder.Entity<ProductCategory>().IsRoot(x => x.Id);
            builder.Entity<ProductTag>().IsRoot(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<ProductDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(ProductDbContext));
        return services;
    }

    // Product/ProductCategory/ProductTag repositories no longer match the generic
    // IRepository<T> shape (custom Search/Exists methods, owned-collection aggregate), so they
    // are registered explicitly rather than picked up by the Scrutor IRepository<> scan - same
    // reasoning as Inventory's IInventoryTransactionRepository / Audit's IAuditLogRepository.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, Repository.ProductRepo>();
        services.AddScoped<IProductCategoryRepository, Repository.ProductCategoryRepo>();
        services.AddScoped<IProductTagRepository, Repository.ProductTagRepo>();
        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();
        return services;
    }

    private static IServiceCollection AddOutbox(this IServiceCollection services)
    {
        services.AddEfOutboxStore<ProductDbContext>();
        services.AddScoped<IOutboxStore, OutboxStore>();

        return services;
    }

    // Product now self-consumes its own integration events for Search sync (see
    // docs/reference/search.md), so it needs an Inbox for dedup - previously it only published.
    private static IServiceCollection AddInbox(this IServiceCollection services)
    {
        services.AddEfInboxStore<ProductDbContext>();
        services.AddScoped<IInboxStore, Inbox.InboxStore>();

        return services;
    }
}
