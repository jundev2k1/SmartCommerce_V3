using Notification.Application.Abstractions.Repositories;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationGroups.Queries.GetNotificationGroup;

public sealed class GetNotificationGroupHandler(INotificationGroupRepository notificationGroupRepo)
    : IQueryHandler<GetNotificationGroupQuery, GetNotificationGroupResponse>
{
    public async Task<GetNotificationGroupResponse> Handle(GetNotificationGroupQuery request, CancellationToken ct = default)
    {
        var entity = await notificationGroupRepo.GetByIdAsync(request.GroupId, ct)
            ?? throw new NotFoundException("NotificationGroup", request.GroupId);

        return new GetNotificationGroupResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Status,
            entity.Audience.Type,
            entity.Audience.ConfigJson,
            entity.CreatedAt);
    }
}
