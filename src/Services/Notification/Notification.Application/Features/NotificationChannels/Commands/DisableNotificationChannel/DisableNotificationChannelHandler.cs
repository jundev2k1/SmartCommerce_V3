using SmartEcommerce.Notification.Application.Abstractions.Persistence.NotificationChannels;
using SmartEcommerce.Notification.Application.Abstractions.Services;

using SmartEcommerce.BuildingBlock.Application.Exceptions;

namespace SmartEcommerce.Notification.Application.Features.NotificationChannels.Commands.DisableNotificationChannel;

public sealed class DisableNotificationChannelHandler(
    INotificationChannelReadService notificationChannelReadService,
    INotificationChannelWriteService notificationChannelWriteService,
    INotificationChannelCache channelCache) : ICommandHandler<DisableNotificationChannelCommand>
{
    public async Task Handle(DisableNotificationChannelCommand request, CancellationToken ct = default)
    {
        var entity = await notificationChannelReadService.GetByIdAsync(request.ChannelId, ct)
            ?? throw new NotFoundException("NotificationChannel", request.ChannelId);

        entity.Disable();

        await notificationChannelWriteService.UpdateAsync(entity, ct);
        await channelCache.InvalidateAsync(entity.ChannelType, ct);
    }
}
