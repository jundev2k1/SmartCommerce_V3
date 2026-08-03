using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;

using Microsoft.Extensions.Configuration;

using SmartEcommerce.User.Application.Abstractions.Services;

namespace SmartEcommerce.User.Infrastructure.Caching;

/// <summary>
/// Redis-backed implementation of User's own UserProfile detail cache. Mirrors Auth's
/// RoleCacheService shape exactly (config-driven TTL with a constant fallback, plain Get/Set/Remove).
/// See docs/reference/caching.md and docs/tasks/2026-07-28/Task11_user-detail-cache-scaffold.md.
/// </summary>
public sealed class UserProfileCacheService(
    ICacheService cacheService,
    IConfiguration configuration) : IUserProfileCacheService
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(
        configuration
            .GetSection("Caching:EntityTtl:UserProfiles:MinutesToExpire")
            .Get<int?>() ?? CacheKeys.UserProfiles.DefaultTtlMinutes);

    public async Task<CachedUserProfile?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var key = CacheKeys.UserProfiles.Detail(userId);
        return await cacheService.GetAsync<CachedUserProfile>(key, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, CachedUserProfile>> GetManyAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct = default)
    {
        if (userIds.Count == 0)
            return new Dictionary<Guid, CachedUserProfile>();

        var idsByKey = userIds.ToDictionary(CacheKeys.UserProfiles.Detail, id => id);
        var cached = await cacheService.GetManyAsync<CachedUserProfile>(idsByKey.Keys, ct);

        var result = new Dictionary<Guid, CachedUserProfile>();
        foreach (var (key, value) in cached)
        {
            if (value is not null)
                result[idsByKey[key]] = value;
        }

        return result;
    }

    public async Task SetAsync(CachedUserProfile profile, CancellationToken ct = default)
    {
        var key = CacheKeys.UserProfiles.Detail(profile.Id);
        await cacheService.SetAsync(key, profile, _defaultTtl, ct);
    }

    public async Task SetManyAsync(IReadOnlyCollection<CachedUserProfile> profiles, CancellationToken ct = default)
    {
        if (profiles.Count == 0)
            return;

        var items = profiles.ToDictionary(p => CacheKeys.UserProfiles.Detail(p.Id), p => p);
        await cacheService.SetManyAsync(items, _defaultTtl, ct);
    }

    public async Task RemoveAsync(Guid userId, CancellationToken ct = default)
    {
        var key = CacheKeys.UserProfiles.Detail(userId);
        await cacheService.RemoveAsync(key, ct);
    }
}
