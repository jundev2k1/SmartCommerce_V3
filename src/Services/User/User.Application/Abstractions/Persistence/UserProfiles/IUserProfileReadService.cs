using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;

namespace User.Application.Abstractions.Persistence.UserProfiles;

public interface IUserProfileReadService
{
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<PaginatedResult<UserProfile>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);
}
