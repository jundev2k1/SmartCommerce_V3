using Notification.Application.Abstractions.Repositories;

using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.UserNotifications.Commands.MarkUserNotificationAsRead;

public sealed class MarkUserNotificationAsReadHandler(
    ICurrentUserService currentUser,
    IUserNotificationRepository userNotificationRepo,
    IUnitOfWork uow) : ICommandHandler<MarkUserNotificationAsReadCommand>
{
    public async Task Handle(MarkUserNotificationAsReadCommand request, CancellationToken ct = default)
    {
        var userId = currentUser.GetUserId()
            ?? throw new UnauthorizedException();

        var entity = await userNotificationRepo.GetByIdAsync(request.NotificationId, ct)
            ?? throw new NotFoundException("UserNotification", request.NotificationId);

        if (entity.UserId != userId)
            throw new ForbiddenException();

        entity.MarkAsRead();

        await userNotificationRepo.UpdateAsync(entity, ct);
        await uow.SaveChangesAsync(ct);
    }
}
