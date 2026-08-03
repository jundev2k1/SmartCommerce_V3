using SmartEcommerce.BuildingBlock.Domain.Abstractions;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Repository;

using SmartEcommerce.Order.Persistence.Engine;

namespace SmartEcommerce.Order.Persistence.Contexts;

public abstract class OrderBaseRepository<TEntity>(OrderDbContext context)
    : GenericRepository<OrderDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class OrderBaseRepository<TEntity, TId>(OrderDbContext context)
    : EntityGenericRepository<OrderDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
