namespace User.Application.Abstractions.Persistence.UserProfiles;

public interface IUserProfileReadService
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Paged, ordered read of every UserProfile - used only by RebuildUserSearchIndex's batch loop.</summary>
    Task<IReadOnlyList<UserProfile>> GetAllAsync(int skip, int take, CancellationToken ct = default);
}
