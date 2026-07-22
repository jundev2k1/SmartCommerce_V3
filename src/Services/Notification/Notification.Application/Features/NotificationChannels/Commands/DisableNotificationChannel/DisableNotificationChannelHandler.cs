using Notification.Application.Abstractions.Repositories;
using Notification.Application.Abstractions.Services;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationChannels.Commands.DisableNotificationChannel;

public sealed class DisableNotificationChannelHandler(
    INotificationChannelRepository notificationChannelRepo,
    INotificationChannelCache channelCache,
    IUnitOfWork uow) : ICommandHandler<DisableNotificationChannelCommand>
{
    public async Task Handle(DisableNotificationChannelCommand request, CancellationToken ct = default)
    {
        var entity = await notificationChannelRepo.GetByIdAsync(request.ChannelId, ct)
            ?? throw new NotFoundException("NotificationChannel", request.ChannelId);

        entity.Disable();

        await notificationChannelRepo.UpdateAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        await channelCache.InvalidateAsync(entity.ChannelType, ct);
    }
}
