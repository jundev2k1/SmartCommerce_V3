using SmartEcommerce.User.Persistence.Engine;

namespace SmartEcommerce.User.Persistence.Contexts.UserProfiles.Repositories;

public sealed class UserProfileRepo(UserDbContext dbContext)
    : UserBaseRepository<UserProfile>(dbContext), IUserProfileRepository
{
}
