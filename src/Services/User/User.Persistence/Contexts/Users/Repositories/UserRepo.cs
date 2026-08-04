using SmartEcommerce.User.Persistence.Engine;

namespace SmartEcommerce.User.Persistence.Contexts.Users.Repositories;

public sealed class UserRepo(UserDbContext dbContext)
    : UserBaseRepository<UserEntity>(dbContext), IUserRepository
{
}
