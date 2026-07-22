using Notification.Application.Abstractions.Repositories;
using Notification.Application.Abstractions.Services;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationChannels.Commands.UpdateNotificationChannelConfiguration;

public sealed class UpdateNotificationChannelConfigurationHandler(
    INotificationChannelRepository notificationChannelRepo,
    INotificationChannelCache channelCache,
    IUnitOfWork uow) : ICommandHandler<UpdateNotificationChannelConfigurationCommand>
{
    public async Task Handle(UpdateNotificationChannelConfigurationCommand request, CancellationToken ct = default)
    {
        var entity = await notificationChannelRepo.GetByIdAsync(request.ChannelId, ct)
            ?? throw new NotFoundException("NotificationChannel", request.ChannelId);

        var configuration = ChannelConfiguration.Create(request.ConfigJson);
        entity.UpdateConfiguration(configuration);

        await notificationChannelRepo.UpdateAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
        await channelCache.InvalidateAsync(entity.ChannelType, ct);
    }
}
