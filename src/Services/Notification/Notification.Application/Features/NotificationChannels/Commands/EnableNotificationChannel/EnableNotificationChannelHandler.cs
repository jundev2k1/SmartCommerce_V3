using Notification.Application.Abstractions.Repositories;
using Notification.Application.Abstractions.Services;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationChannels.Commands.EnableNotificationChannel;

public sealed class EnableNotificationChannelHandler(
    INotificationChannelRepository notificationChannelRepo,
    INotificationChannelCache channelCache,
    IUnitOfWork uow) : ICommandHandler<EnableNotificationChannelCommand>
{
    public async Task Handle(EnableNotificationChannelCommand request, CancellationToken ct = default)
    {
        var entity = await notificationChannelRepo.GetByIdAsync(request.ChannelId, ct)
            ?? throw new NotFoundException("NotificationChannel", request.ChannelId);

        entity.Enable();

        await notificationChannelRepo.UpdateAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        await channelCache.InvalidateAsync(entity.ChannelType, ct);
    }
}
