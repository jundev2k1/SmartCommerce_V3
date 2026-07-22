using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationRules.Commands.CreateNotificationRule;

public sealed class CreateNotificationRuleHandler(
    INotificationRuleRepository notificationRuleRepo,
    IUnitOfWork uow) : ICommandHandler<CreateNotificationRuleCommand, CreateNotificationRuleResponse>
{
    public async Task<CreateNotificationRuleResponse> Handle(CreateNotificationRuleCommand request, CancellationToken ct = default)
    {
        var targets = request.Targets.Select(t => new RuleTargetCreateModel(t.Channel, t.TemplateId, t.Priority));

        var entity = NotificationRule.Create(
            Guid.CreateVersion7(), request.Name, request.Description, request.EventType, targets);

        await notificationRuleRepo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return new CreateNotificationRuleResponse(entity.Id);
    }
}
