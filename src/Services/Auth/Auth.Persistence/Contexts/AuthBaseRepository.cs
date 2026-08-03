using SmartEcommerce.Auth.Persistence.Engine;

using SmartEcommerce.BuildingBlock.Domain.Abstractions;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Repository;

namespace SmartEcommerce.Auth.Persistence.Contexts;

public abstract class AuthBaseRepository<TEntity>(AuthDbContext context)
    : GenericRepository<AuthDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
