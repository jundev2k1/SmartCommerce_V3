using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;

using User.Application.Abstractions.Persistence.UserProfiles;
using User.Application.Features.Users.Search;

namespace User.Persistence.UserProfiles.Read;

public sealed class UserProfileReadService(UserDbContext dbContext) : IUserProfileReadService
{
    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<PaginatedResult<UserProfile>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.UserProfiles
            .AsNoTracking()
            .ApplyCriteria(UserCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
