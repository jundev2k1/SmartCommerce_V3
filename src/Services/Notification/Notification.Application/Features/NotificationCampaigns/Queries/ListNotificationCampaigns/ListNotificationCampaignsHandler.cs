using BuildingBlock.Application.Abstractions.Common;

using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationCampaigns.Queries.ListNotificationCampaigns;

public sealed class ListNotificationCampaignsHandler(INotificationCampaignRepository notificationCampaignRepo)
    : IQueryHandler<ListNotificationCampaignsQuery, PaginatedResult<NotificationCampaignSummaryResponse>>
{
    public async Task<PaginatedResult<NotificationCampaignSummaryResponse>> Handle(ListNotificationCampaignsQuery request, CancellationToken ct = default)
    {
        var (items, totalCount) = await notificationCampaignRepo.SearchAsync(request.Status, request.Page, request.PageSize, ct);

        var mapped = items
            .Select(x => new NotificationCampaignSummaryResponse(x.Id, x.Name, x.Status, x.Schedule.ExecutionType, x.NextExecutionAt, x.CreatedAt))
            .ToList();

        return PaginatedResult<NotificationCampaignSummaryResponse>.Create(mapped, request.Page, request.PageSize, totalCount);
    }
}
