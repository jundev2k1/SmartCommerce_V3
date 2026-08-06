using NovaCore.BuildingBlock.Application.Abstractions.Outbox;
using NovaCore.BuildingBlock.Application.Abstractions.Persistence;
using NovaCore.BuildingBlock.Application.Abstractions.Services;
using NovaCore.BuildingBlock.Application.Extensions;
using NovaCore.BuildingBlock.Persistence.Audit;
using NovaCore.BuildingBlock.Persistence.Ef.DependencyInjection;
using NovaCore.BuildingBlock.Persistence.Ef.Inbox;
using NovaCore.BuildingBlock.Persistence.Ef.Outbox;
using NovaCore.BuildingBlock.Persistence.Repository;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

using NovaCore.Promotion.Persistence.Engine;
using NovaCore.Promotion.Persistence.Engine.UnitOfWork;
using NovaCore.Promotion.Persistence.Reliability.Inbox;
using NovaCore.Promotion.Persistence.Reliability.Outbox;

namespace NovaCore.Promotion.Persistence;

public static class DependencyInjection
{
    public static TracerProviderBuilder AddPersistenceTracing(this TracerProviderBuilder builder)
        => builder.AddNpgsql();

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
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
        services.AddPersistenceDbContext<PromotionDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterfaceAndConcrete<IAppService>(typeof(PromotionDbContext));
        return services;
    }

    // No repository/Read/Write service exists yet - no aggregate has been implemented (Phase 2).
    // This scan call is harmless with zero registrations today and picks up every repository
    // this project adds starting Phase 5 (CQRS Skeleton), same pattern every other service uses.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(PromotionDbContext));
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
            .AddEfOutboxStore<PromotionDbContext>()
            .AddEfInboxStore<PromotionDbContext>()
            .AddEfDeadLetterQueryService<PromotionDbContext>();

        services.AddScoped<IOutboxStore, OutboxStore>();
        services.AddScoped<IInboxStore, InboxStore>();

        return services;
    }

    // No IAuditable entity exists yet - entries are added here as each aggregate is implemented
    // (Phase 2), following Payment Service's own precedent of keeping this hierarchy current as
    // aggregates land, even before they have a repository/CQRS surface.
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            // TODO (Phase 2+): builder.Entity<TAggregate>().IsRoot(x => x.Id); / .BelongsTo<TRoot>(x => x.RootId);
        });

        return services;
    }
}
