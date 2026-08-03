namespace SmartEcommerce.User.Application.Abstractions.Persistence.UserProfiles;

public interface IUserProfileReadService
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Batch lookup by id, one query - used by the cache read-through and the gRPC batch RPC to avoid N+1.</summary>
    Task<IReadOnlyList<UserProfile>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    /// <summary>Paged, ordered read of every UserProfile - used only by RebuildUserSearchIndex's batch loop.</summary>
    Task<IReadOnlyList<UserProfile>> GetAllAsync(int skip, int take, CancellationToken ct = default);
}
