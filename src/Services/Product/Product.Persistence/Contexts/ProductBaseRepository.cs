using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Persistence.Ef.Repository;

using Product.Persistence.Engine;

namespace Product.Persistence.Contexts;

public abstract class ProductBaseRepository<TEntity>(ProductDbContext context)
    : GenericRepository<ProductDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
