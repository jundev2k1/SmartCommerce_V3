using User.Persistence.Engine;

namespace User.Persistence.Contexts.UserProfiles.Repositories;

public sealed class UserProfileRepo(UserDbContext dbContext)
    : UserBaseRepository<UserProfile>(dbContext), IUserProfileRepository
{
}
