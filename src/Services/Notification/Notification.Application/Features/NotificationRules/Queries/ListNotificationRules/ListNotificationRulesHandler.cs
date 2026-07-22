using BuildingBlock.Application.Abstractions.Common;

using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationRules.Queries.ListNotificationRules;

public sealed class ListNotificationRulesHandler(INotificationRuleRepository notificationRuleRepo)
    : IQueryHandler<ListNotificationRulesQuery, PaginatedResult<NotificationRuleSummaryResponse>>
{
    public async Task<PaginatedResult<NotificationRuleSummaryResponse>> Handle(ListNotificationRulesQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await notificationRuleRepo.SearchAsync(request.EventType, request.Page, request.PageSize, ct);

        var mapped = items
            .Select(x => new NotificationRuleSummaryResponse(x.Id, x.Name, x.EventType, x.Status, x.Targets.Count, x.CreatedAt))
            .ToList();

        return PaginatedResult<NotificationRuleSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
