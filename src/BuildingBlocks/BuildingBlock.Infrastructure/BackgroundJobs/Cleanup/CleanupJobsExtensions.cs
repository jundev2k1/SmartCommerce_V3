using BuildingBlock.Application.Abstractions.Jobs;
using BuildingBlock.Infrastructure.Extensions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlock.Infrastructure.BackgroundJobs.Cleanup;

/// <summary>
/// Opt-in registration for the Inbox/Outbox cleanup recurring jobs. Independent of
/// AddHangfireScheduling's own job-assembly markers - a service just needs Hangfire
/// scheduling wired up (via AddHangfireScheduling) and its IOutboxStore/IInboxStore
/// registered (via AddOutboxAndInbox) for these jobs to resolve and run.
/// </summary>
public static class CleanupJobsExtensions
{
    public static IServiceCollection AddInboxOutboxCleanupJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .Configure<OutboxCleanupOptions>(configuration.GetSection(OutboxCleanupOptions.Section))
            .Configure<InboxCleanupOptions>(configuration.GetSection(InboxCleanupOptions.Section))
            .AddScopedByInterfaceAndConcrete<IRecurringJob>(typeof(OutboxCleanupJob));

        return services;
    }
}
