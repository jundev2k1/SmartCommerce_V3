using BuildingBlock.Application.Abstractions.Common;

using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationTemplates.Queries.ListNotificationTemplates;

public sealed class ListNotificationTemplatesHandler(INotificationTemplateRepository notificationTemplateRepo)
    : IQueryHandler<ListNotificationTemplatesQuery, PaginatedResult<NotificationTemplateSummaryResponse>>
{
    public async Task<PaginatedResult<NotificationTemplateSummaryResponse>> Handle(ListNotificationTemplatesQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await notificationTemplateRepo.SearchAsync(request.Channel, request.Page, request.PageSize, ct);

        var mapped = items
            .Select(x => new NotificationTemplateSummaryResponse(x.Id, x.Name, x.Channel, x.Status, x.CreatedAt))
            .ToList();

        return PaginatedResult<NotificationTemplateSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
