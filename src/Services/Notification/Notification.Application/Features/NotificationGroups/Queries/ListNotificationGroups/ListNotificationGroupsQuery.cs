using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;

namespace SmartEcommerce.Notification.Application.Features.NotificationGroups.Queries.ListNotificationGroups;

public sealed record ListNotificationGroupsQuery(
    string? Search,
    int Page = 1,
    int PageSize = 20) : IQuery<PaginatedResult<NotificationGroupSummaryResponse>>;

public sealed record NotificationGroupSummaryResponse(
    Guid Id,
    string Name,
    NotificationGroupStatus Status,
    AudienceType AudienceType,
    DateTime CreatedAt);
