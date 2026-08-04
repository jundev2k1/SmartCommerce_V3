using SmartEcommerce.Auth.Application.Abstractions.Persistence.Accounts;
using SmartEcommerce.Auth.Application.Abstractions.Persistence.RefreshTokens;
using SmartEcommerce.Auth.Domain.Entities.Accounts;
using SmartEcommerce.Auth.Domain.Entities.Invitations;
using SmartEcommerce.Auth.Domain.Entities.Permissions;
using SmartEcommerce.Auth.Domain.Entities.Positions;
using SmartEcommerce.Auth.Domain.Entities.Roles;
using SmartEcommerce.Auth.Persistence.Contexts.Accounts.Read;
using SmartEcommerce.Auth.Persistence.Contexts.Accounts.Repositories;
using SmartEcommerce.Auth.Persistence.Contexts.Accounts.Write;
using SmartEcommerce.Auth.Persistence.Contexts.RefreshTokens.Read;
using SmartEcommerce.Auth.Persistence.Contexts.RefreshTokens.Write;
using SmartEcommerce.Auth.Persistence.Engine;
using SmartEcommerce.Auth.Persistence.Engine.UnitOfWork;
using SmartEcommerce.Auth.Persistence.Reliability.Inbox;
using SmartEcommerce.Auth.Persistence.Reliability.Outbox;
using SmartEcommerce.Auth.Persistence.Storage.Seeders;

using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Extensions;
using SmartEcommerce.BuildingBlock.Persistence.Audit;
using SmartEcommerce.BuildingBlock.Persistence.Ef.DependencyInjection;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Inbox;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Outbox;
using SmartEcommerce.BuildingBlock.Persistence.Repository;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

namespace SmartEcommerce.Auth.Persistence;

public static class DependencyInjection
{
    public static TracerProviderBuilder AddPersistenceTracing(this TracerProviderBuilder builder)
    {
        return builder.AddNpgsql();
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabaseContext(configuration)
            .AddIdentityService()
            .AddApplicationServices()
            .AddRepositories()
            .AddUnitOfWork()
            .AddOutboxAndInbox()
            .AddSeeding()
            .AddAuditHierarchy();

        return services;
    }

    // Account, Role, Position, PermissionGroup, PermissionDefinition and Invitation are
    // independent aggregates. AccountPosition is a genuine history-tracked child of Account (like
    // Order's OrderItem/OrderOwner) - registered via BelongsTo so its Assign/Revoke/Expire
    // transitions are audited under the owning Account. AccountRole/RolePermission/PositionRole
    // (pure mapping, no independent lifecycle) and every *Translation entity are intentionally
    // left unregistered - none of the former are IAuditable, and translations are not audited
    // individually project-wide (see docs/conventions/domain-coding-conventions.md).
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<Account>().IsRoot(x => x.Id);
            builder.Entity<AccountPosition>()
                .BelongsTo<Account>(x => x.AccountId);

            builder.Entity<Role>().IsRoot(x => x.Id);
            builder.Entity<Position>().IsRoot(x => x.Id);
            builder.Entity<PermissionGroup>().IsRoot(x => x.Id);
            builder.Entity<PermissionDefinition>().IsRoot(x => x.Id);
            builder.Entity<Invitation>().IsRoot(x => x.Id);
        });

        return services;
    }

    private static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentException("ConnectionString was not configured.");

        services.AddPersistenceDbContext<AuthDbContext>(connectionString);

        return services;
    }

    private static IServiceCollection AddIdentityService(this IServiceCollection services)
    {
        services.AddIdentityCore<Account>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<AuthDbContext>();

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScopedByInterface<IAppService>(typeof(AuthDbContext));
        return services;
    }

    // RefreshTokenRepo implements the generic IRepository<T> (restored - it genuinely needs the
    // tracked-load-and-mutate UpdateAsync), so it's Scrutor-scanned; AccountRepo doesn't (its only
    // real operation, DeleteIfExistAsync, is a bulk ExecuteDeleteAsync that doesn't fit the
    // generic shape - Account mutation itself goes through ASP.NET Identity's UserManager, not
    // this repository at all), so it stays manually registered.
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScopedByInterface(typeof(IRepository<>), typeof(AuthDbContext));

        services.AddScoped<IAccountRepository, AccountRepo>();
        services.AddScoped<IAccountReadService, AccountReadService>();
        services.AddScoped<IAccountWriteService, AccountWriteService>();

        services.AddScoped<IRefreshTokenReadService, RefreshTokenReadService>();
        services.AddScoped<IRefreshTokenWriteService, RefreshTokenWriteService>();

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, AuthUnitOfWork>();
        return services;
    }

    private static IServiceCollection AddSeeding(this IServiceCollection services)
    {
        services.AddScoped<DatabaseSeeder>();
        return services;
    }

    // Manually registered (not Scrutor-scanned) because the adapter needs a literal
    // serviceName - must match the string passed to AddKafkaMessaging so relayed
    // messages land on the topic other services actually listen on.
    private static IServiceCollection AddOutboxAndInbox(this IServiceCollection services)
    {
        services
            .AddEfOutboxStore<AuthDbContext>()
            .AddEfInboxStore<AuthDbContext>()
            .AddEfDeadLetterQueryService<AuthDbContext>();

        services.AddScoped<IOutboxStore>(sp => new OutboxStore(
            sp.GetRequiredService<SmartEcommerce.BuildingBlock.Persistence.Outbox.IOutboxStore>(),
            "auth-service"));
        services.AddScoped<IInboxStore>(sp => new InboxStore(
            sp.GetRequiredService<SmartEcommerce.BuildingBlock.Persistence.Inbox.IInboxStore>()));

        return services;
    }
}
