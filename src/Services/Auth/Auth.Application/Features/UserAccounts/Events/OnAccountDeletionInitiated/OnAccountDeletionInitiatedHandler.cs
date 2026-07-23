namespace Auth.Application.Features.UserAccounts.Events.OnAccountDeletionInitiated;

public sealed class OnAccountDeletionInitiatedHandler(
    IAccountWriteService accountWriteService
) : IInternalEventHandler<OnAccountDeletionInitiatedEvent>
{
    public async Task Handle(OnAccountDeletionInitiatedEvent @event, CancellationToken ct = default)
    {
        await accountWriteService.DeleteIfExistAsync(@event.AccountId, ct);
    }
}