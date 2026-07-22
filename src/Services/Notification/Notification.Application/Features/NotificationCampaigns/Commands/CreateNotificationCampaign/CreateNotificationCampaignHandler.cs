using Notification.Application.Abstractions.Repositories;

namespace Notification.Application.Features.NotificationCampaigns.Commands.CreateNotificationCampaign;

public sealed class CreateNotificationCampaignHandler(
    INotificationCampaignRepository notificationCampaignRepo,
    IUnitOfWork uow) : ICommandHandler<CreateNotificationCampaignCommand, CreateNotificationCampaignResponse>
{
    public async Task<CreateNotificationCampaignResponse> Handle(CreateNotificationCampaignCommand request, CancellationToken ct = default)
    {
        var schedule = NotificationSchedule.Create(request.ExecutionType, request.StartAt, request.EndAt, request.CronExpression);
        var targets = request.Targets.Select(t => new CampaignTargetCreateModel(t.Channel, t.TemplateId, t.Priority));

        var entity = NotificationCampaign.Create(
            Guid.CreateVersion7(), request.Name, request.Description, request.GroupId, schedule, targets);

        await notificationCampaignRepo.AddAsync(entity, ct);
        await uow.SaveChangesAsync(ct);

        return new CreateNotificationCampaignResponse(entity.Id);
    }
}
