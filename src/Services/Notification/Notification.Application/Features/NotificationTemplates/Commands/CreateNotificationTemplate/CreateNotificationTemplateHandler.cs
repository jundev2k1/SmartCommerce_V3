using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationTemplates.Commands.CreateNotificationTemplate;

public sealed class CreateNotificationTemplateHandler(
    INotificationTemplateRepository notificationTemplateRepo,
    IUnitOfWork uow) : ICommandHandler<CreateNotificationTemplateCommand, CreateNotificationTemplateResponse>
{
    public async Task<CreateNotificationTemplateResponse> Handle(CreateNotificationTemplateCommand request, CancellationToken ct = default)
    {
        var content = TemplateContent.Create(request.Subject, request.Body, request.Variables);

        var entity = NotificationTemplate.Create(
            Guid.CreateVersion7(), request.Name, request.Channel, content);

        await notificationTemplateRepo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return new CreateNotificationTemplateResponse(entity.Id);
    }
}
