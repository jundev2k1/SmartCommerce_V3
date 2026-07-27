using BuildingBlock.Application.Abstractions.Jobs;
using BuildingBlock.SharedKernel.Constants;

namespace BuildingBlock.Infrastructure.BackgroundJobs.Monitoring;

public sealed class InboxDeadLetterMonitorOptions : IJobOptions
{
    public const string Section = "Jobs:InboxDeadLetterMonitor";

    public string JobId { get; set; } = "inbox-dead-letter-monitor";
    public string CronExpression { get; set; } = "*/15 * * * *";
    public string Queue { get; set; } = JobQueue.DEFAULT;
    public bool IsInit { get; set; }

    /// <summary>Whether the job actually queries/logs anything when it runs. Off = no-op, cron stays registered.</summary>
    public bool Enabled { get; set; } = true;
}
