using SmartEcommerce.BuildingBlock.Application.Abstractions.Events;

using SmartEcommerce.User.Application.Abstractions.Search;

namespace SmartEcommerce.User.Application.Features.Users.Events.OnUserSearchRemovalRequired;

public sealed class OnUserSearchRemovalRequiredHandler(IUserSearchIndexer searchIndexer)
    : IInternalEventHandler<OnUserSearchRemovalRequiredEvent>
{
    public Task Handle(OnUserSearchRemovalRequiredEvent @event, CancellationToken ct = default)
        => searchIndexer.DeleteAsync(@event.UserId, ct);
}
