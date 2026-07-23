using Notification.Application.Abstractions.Persistence.NotificationTemplates;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationTemplates.Queries.GetNotificationTemplate;

public sealed class GetNotificationTemplateHandler(INotificationTemplateReadService notificationTemplateReadService)
    : IQueryHandler<GetNotificationTemplateQuery, GetNotificationTemplateResponse>
{
    public async Task<GetNotificationTemplateResponse> Handle(GetNotificationTemplateQuery request, CancellationToken ct = default)
    {
        var entity = await notificationTemplateReadService.GetByIdAsync(request.TemplateId, ct)
            ?? throw new NotFoundException("NotificationTemplate", request.TemplateId);

        return new GetNotificationTemplateResponse(
            entity.Id,
            entity.Name,
            entity.Channel,
            entity.Content.Subject,
            entity.Content.Body,
            entity.Content.Variables,
            entity.Status,
            entity.CreatedAt);
    }
}
