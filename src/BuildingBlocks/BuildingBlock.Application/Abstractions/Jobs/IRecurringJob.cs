using BuildingBlock.SharedKernel.Constants;

namespace BuildingBlock.Application.Abstractions.Jobs;

public interface IRecurringJob
{
    string JobId { get; }

    string CronExpression { get; }

    string Queue => JobQueue.DEFAULT;

    bool IsInit { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
