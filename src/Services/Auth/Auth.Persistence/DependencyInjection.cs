using Auth.Application.Abstractions.Persistence.Accounts;
using Auth.Application.Abstractions.Persistence.RefreshTokens;
using Auth.Domain.Entities;
using Auth.Persistence.Contexts.Accounts.Read;
using Auth.Persistence.Contexts.Accounts.Repositories;
using Auth.Persistence.Contexts.Accounts.Write;
using Auth.Persistence.Contexts.RefreshTokens.Read;
using Auth.Persistence.Contexts.RefreshTokens.Write;
using Auth.Persistence.Engine;
using Auth.Persistence.Engine.UnitOfWork;
using Auth.Persistence.Reliability.Inbox;
using Auth.Persistence.Reliability.Outbox;
using Auth.Persistence.Storage.Seeders;

using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Extensions;
using BuildingBlock.Persistence.Audit;
using BuildingBlock.Persistence.Ef.DependencyInjection;
using BuildingBlock.Persistence.Ef.Inbox;
using BuildingBlock.Persistence.Ef.Outbox;
using BuildingBlock.Persistence.Repository;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenTelemetry.Trace;

namespace Auth.Persistence;

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

    // Account and Role are independent aggregates (many-to-many via AccountRole, which isn't
    // itself audited - a join row has no single owning parent this model can express).
    private static IServiceCollection AddAuditHierarchy(this IServiceCollection services)
    {
        services.ConfigureAuditHierarchy(builder =>
        {
            builder.Entity<Account>().IsRoot(x => x.Id);
            builder.Entity<Role>().IsRoot(x => x.Id);
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
            .AddEfInboxStore<AuthDbContext>();

        services.AddScoped<IOutboxStore>(sp => new OutboxStore(
            sp.GetRequiredService<BuildingBlock.Persistence.Outbox.IOutboxStore>(),
            "auth-service"));
        services.AddScoped<IInboxStore>(sp => new InboxStore(
            sp.GetRequiredService<BuildingBlock.Persistence.Inbox.IInboxStore>()));

        return services;
    }
}
