using Notification.Application.Abstractions.Repositories;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationChannels.Queries.GetNotificationChannel;

public sealed class GetNotificationChannelHandler(INotificationChannelRepository notificationChannelRepo)
    : IQueryHandler<GetNotificationChannelQuery, GetNotificationChannelResponse>
{
    public async Task<GetNotificationChannelResponse> Handle(GetNotificationChannelQuery request, CancellationToken ct = default)
    {
        var entity = await notificationChannelRepo.GetByIdAsync(request.ChannelId, ct)
            ?? throw new NotFoundException("NotificationChannel", request.ChannelId);

        return new GetNotificationChannelResponse(
            entity.Id,
            entity.ChannelType,
            entity.DisplayName,
            entity.Status,
            entity.Configuration.ConfigJson,
            entity.ValidationStatus,
            entity.LastValidatedAt,
            entity.LastValidationError);
    }
}
