using SmartEcommerce.BuildingBlock.Domain.Abstractions;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Repository;

using SmartEcommerce.User.Persistence.Engine;

namespace SmartEcommerce.User.Persistence.Contexts;

public abstract class UserBaseRepository<TEntity>(UserDbContext context)
    : GenericRepository<UserDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
