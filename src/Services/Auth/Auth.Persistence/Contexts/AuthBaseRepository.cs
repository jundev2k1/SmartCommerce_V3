using Auth.Persistence.Engine;

using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Persistence.Ef.Repository;

namespace Auth.Persistence.Contexts;

public abstract class AuthBaseRepository<TEntity>(AuthDbContext context)
    : GenericRepository<AuthDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
