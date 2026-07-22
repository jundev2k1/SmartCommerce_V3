using Notification.Application.Abstractions.Repositories;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationDispatches.Queries.GetNotificationDispatch;

public sealed class GetNotificationDispatchHandler(INotificationDispatchRepository notificationDispatchRepo)
    : IQueryHandler<GetNotificationDispatchQuery, GetNotificationDispatchResponse>
{
    public async Task<GetNotificationDispatchResponse> Handle(GetNotificationDispatchQuery request, CancellationToken ct = default)
    {
        var entity = await notificationDispatchRepo.GetByIdAsync(request.DispatchId, ct)
            ?? throw new NotFoundException("NotificationDispatch", request.DispatchId);

        return new GetNotificationDispatchResponse(
            entity.Id,
            entity.Reference.ReferenceType,
            entity.Reference.ReferenceId,
            entity.Channel,
            entity.TemplateId,
            entity.Payload,
            entity.Status,
            entity.RetryCount,
            entity.NextRetryAt,
            entity.LastError,
            entity.DispatchedAt,
            entity.CreatedAt);
    }
}
