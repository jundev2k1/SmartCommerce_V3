using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Extensions;
using BuildingBlock.Persistence.Audit;
using BuildingBlock.Persistence.Ef.DependencyInjection;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;
using BuildingBlock.Persistence.Repository;
using BuildingBlock.Search.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Abstractions.Search;
using User.Persistence.Engine;
using User.Persistence.Engine.UnitOfWork;
using User.Persistence.Inbox;
using User.Persistence.Outbox;
using User.Persistence.Contexts.UserProfiles.Read;
using User.Persistence.Contexts.UserProfiles.Repositories;
using User.Persistence.Contexts.UserProfiles.Search.Indexers;
using User.Persistence.Contexts.UserProfiles.Search.Repositories;
using User.Persistence.Contexts.UserProfiles.Write;

namespace User.Persistence;

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
            .AddUserProfilePersistence()
            .AddUnitOfWork()
            .AddOutboxAndInbox()
            .AddAuditHierarchy()
            .AddUserSearchServices(configuration);

        return services;
    }

    // Mirrors Product.Persistence's AddProductSearchServices - a business-capability-named
    // method (not AddElasticsearchPersistence) called from AddPersistence, never a separate
    // Program.cs step. See docs/reference/search.md.
    private static IServiceCollection AddUserSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddElasticsearchClient(configuration);
        services.AddScoped<IUserSearchIndexer, UserSearchIndexer>();
        services.AddScoped<IUserSearchRepository, UserSearchRepository>();

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddPersistenceDbContext<UserDbContext>(connectionString);

        return services;
    }

    // UserProfile is a flat aggregate - no children today - so it's registered as a root with no
    // BelongsTo declarations. Its audit graph is just a single node, same shape as any other root.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder => builder.Entity<UserProfile>().IsRoot(x => x.Id));

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(UserDbContext));
        return services;
    }

    // UserProfileRepo implements the generic IRepository<T> again - AsImplementedInterfaces()
    // registers it against both IRepository<UserProfile> and the specific IUserProfileRepository
    // in one scan call.
    private static IServiceCollection AddUserProfilePersistence(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(UserDbContext));

        services.AddScoped<IUserProfileReadService, UserProfileReadService>();
        services.AddScoped<IUserProfileWriteService, UserProfileWriteService>();
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
            .AddEfOutboxStore<UserDbContext>()
            .AddEfInboxStore<UserDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }
}
