using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationGroups.Commands.CreateNotificationGroup;

public sealed class CreateNotificationGroupHandler(
    INotificationGroupRepository notificationGroupRepo,
    IUnitOfWork uow) : ICommandHandler<CreateNotificationGroupCommand, CreateNotificationGroupResponse>
{
    public async Task<CreateNotificationGroupResponse> Handle(CreateNotificationGroupCommand request, CancellationToken ct = default)
    {
        var audience = AudienceSelector.Create(request.AudienceType, request.AudienceConfigJson);

        var entity = NotificationGroup.Create(
            Guid.CreateVersion7(), request.Name, request.Description, audience);

        await notificationGroupRepo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return new CreateNotificationGroupResponse(entity.Id);
    }
}
