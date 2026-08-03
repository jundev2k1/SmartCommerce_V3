namespace SmartEcommerce.User.Application.Abstractions.Services;

/// <summary>
/// Flat, JSON-serializable snapshot of a UserProfile for caching purposes. A separate DTO
/// (rather than caching UserProfile itself) is deliberate: UserProfile's setters are private
/// (domain encapsulation), so it can't be reconstructed from deserialized JSON outside
/// SmartEcommerce.User.Domain - the same reasoning that gives Elasticsearch its own read-model document
/// instead of indexing the aggregate directly. See docs/tasks/2026-07-28/Task11_user-detail-cache-scaffold.md.
/// </summary>
public sealed record CachedUserProfile(
    Guid Id,
    string Email,
    string UserName,
    string PhoneNumber,
    string FirstName,
    string MiddleName,
    string LastName,
    string Status,
    string[] Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static CachedUserProfile FromEntity(UserProfile user) => new(
        user.Id,
        user.Email,
        user.UserName,
        user.PhoneNumber,
        user.FirstName,
        user.MiddleName,
        user.LastName,
        user.Status.ToString(),
        user.Roles,
        user.CreatedAt,
        user.UpdatedAt);
}

/// <summary>
/// Owning-service cache primitives for User's own UserProfile detail cache - mirrors Auth's
/// RoleCacheService shape (Get/Set/Remove, config-driven TTL with a constant fallback). This is
/// the write-owning side; SmartEcommerce.User.Infrastructure.Caching.UserProfileCacheService is the Redis-backed
/// implementation. See docs/reference/caching.md.
/// </summary>
public interface IUserProfileCacheService
{
    Task<CachedUserProfile?> GetAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, CachedUserProfile>> GetManyAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct = default);

    Task SetAsync(CachedUserProfile profile, CancellationToken ct = default);

    Task SetManyAsync(IReadOnlyCollection<CachedUserProfile> profiles, CancellationToken ct = default);

    Task RemoveAsync(Guid userId, CancellationToken ct = default);
}
