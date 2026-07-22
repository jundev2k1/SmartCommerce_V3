namespace Auth.Application.Features.UserAccounts.Events.OnAccountDeletionInitiated;

public sealed class OnAccountDeletionInitiatedHandler(
    IUnitOfWork unitOfWork,
    IAccountRepository accountRepo
) : IInternalEventHandler<OnAccountDeletionInitiatedEvent>
{
    public async Task Handle(OnAccountDeletionInitiatedEvent @event, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await accountRepo.DeleteIfExistAsync(@event.AccountId, ct);
        },
        ct: ct);
    }
}