using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Persistence.Ef.Repository;

using User.Persistence.Engine;

namespace User.Persistence.Contexts;

public abstract class UserBaseRepository<TEntity>(UserDbContext context)
    : GenericRepository<UserDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
