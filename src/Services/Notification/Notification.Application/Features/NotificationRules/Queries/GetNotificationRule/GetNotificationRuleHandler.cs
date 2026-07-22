using Notification.Application.Abstractions.Repositories;

using BuildingBlock.Application.Exceptions;

namespace Notification.Application.Features.NotificationRules.Queries.GetNotificationRule;

public sealed class GetNotificationRuleHandler(INotificationRuleRepository notificationRuleRepo)
    : IQueryHandler<GetNotificationRuleQuery, GetNotificationRuleResponse>
{
    public async Task<GetNotificationRuleResponse> Handle(GetNotificationRuleQuery request, CancellationToken ct = default)
    {
        var entity = await notificationRuleRepo.GetByIdAsync(request.RuleId, ct)
            ?? throw new NotFoundException("NotificationRule", request.RuleId);

        var targets = entity.Targets
            .Select(t => new NotificationRuleTargetResponse(t.Id, t.Channel, t.TemplateId, t.Priority, t.Enabled))
            .ToList();

        return new GetNotificationRuleResponse(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.EventType,
            entity.Status,
            targets,
            entity.CreatedAt);
    }
}
