using Notification.Application.Abstractions.Persistence.NotificationChannels;
using Notification.Application.Abstractions.Services;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationChannels.Commands.EnableNotificationChannel;

public sealed class EnableNotificationChannelHandler(
    INotificationChannelReadService notificationChannelReadService,
    INotificationChannelWriteService notificationChannelWriteService,
    INotificationChannelCache channelCache) : ICommandHandler<EnableNotificationChannelCommand>
{
    public async Task Handle(EnableNotificationChannelCommand request, CancellationToken ct = default)
    {
        var entity = await notificationChannelReadService.GetByIdAsync(request.ChannelId, ct)
            ?? throw new NotFoundException("NotificationChannel", request.ChannelId);

        entity.Enable();

        await notificationChannelWriteService.UpdateAsync(entity, ct);
        await channelCache.InvalidateAsync(entity.ChannelType, ct);
    }
}
