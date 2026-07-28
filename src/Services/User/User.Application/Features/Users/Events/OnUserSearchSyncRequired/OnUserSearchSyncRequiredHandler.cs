using BuildingBlock.Application.Abstractions.Events;
using BuildingBlock.Application.Abstractions.Services;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Abstractions.Search;
using User.Application.Features.Users.Search;

namespace User.Application.Features.Users.Events.OnUserSearchSyncRequired;

/// <summary>The Search sync reaction: rebuild the document from current Postgres state and upsert it. See docs/reference/search.md.</summary>
public sealed class OnUserSearchSyncRequiredHandler(
    IUserProfileReadService userReadService,
    UserSearchProjectionBuilder projectionBuilder,
    IUserSearchIndexer searchIndexer,
    IAppLogger<OnUserSearchSyncRequiredHandler> logger) : IInternalEventHandler<OnUserSearchSyncRequiredEvent>
{
    public async Task Handle(OnUserSearchSyncRequiredEvent @event, CancellationToken ct = default)
    {
        var user = await userReadService.GetByIdAsync(@event.UserId, ct);
        if (user is null)
        {
            logger.Warning("User {UserId} no longer exists - skipping search sync", @event.UserId);
            return;
        }

        var document = await projectionBuilder.BuildAsync(user, ct);
        await searchIndexer.IndexAsync(document, ct);
    }
}
