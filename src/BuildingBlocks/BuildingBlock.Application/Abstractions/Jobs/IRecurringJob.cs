using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

namespace SmartEcommerce.BuildingBlock.Application.Abstractions.Jobs;

public interface IRecurringJob
{
    string JobId { get; }

    string CronExpression { get; }

    string Queue => JobQueueConstant.DEFAULT;

    bool IsInit { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}
