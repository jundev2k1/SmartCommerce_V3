using SmartEcommerce.User.Application.Abstractions.Persistence.UserProfiles;
using SmartEcommerce.User.Application.Abstractions.Services;

namespace SmartEcommerce.User.Application.Features.Users.Caching;

/// <summary>
/// Read-through orchestration for the User Detail cache: cache -&gt; DB on miss -&gt; refresh cache -&gt; return.
/// Single lookup and batch lookup both live here so both GetUserDetailHandler (REST) and the
/// gRPC GetUser/GetUsers RPCs (Task 14) share one implementation. The batch path never falls
/// back to a loop of single lookups - exactly one IUserProfileReadService.GetByIdsAsync call for
/// whatever wasn't already cached. See docs/tasks/2026-07-28/Task11_user-detail-cache-scaffold.md.
/// </summary>
public sealed class CachedUserProfileReader(
    IUserProfileCacheService cache,
    IUserProfileReadService userReadService)
{
    public async Task<CachedUserProfile?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var cached = await cache.GetAsync(userId, ct);
        if (cached is not null)
            return cached;

        var user = await userReadService.GetByIdAsync(userId, ct);
        if (user is null)
            return null;

        var profile = CachedUserProfile.FromEntity(user);
        await cache.SetAsync(profile, ct);
        return profile;
    }

    public async Task<IReadOnlyDictionary<Guid, CachedUserProfile>> GetManyAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken ct = default)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, CachedUserProfile>();

        var cached = await cache.GetManyAsync(userIds, ct);
        var missingIds = userIds.Where(id => !cached.ContainsKey(id)).ToList();

        if (missingIds.Count == 0)
            return cached;

        var freshUsers = await userReadService.GetByIdsAsync(missingIds, ct);
        var freshProfiles = freshUsers.Select(CachedUserProfile.FromEntity).ToList();

        if (freshProfiles.Count > 0)
            await cache.SetManyAsync(freshProfiles, ct);

        var result = new Dictionary<Guid, CachedUserProfile>(cached);
        foreach (var profile in freshProfiles)
            result[profile.Id] = profile;

        return result;
    }
}
