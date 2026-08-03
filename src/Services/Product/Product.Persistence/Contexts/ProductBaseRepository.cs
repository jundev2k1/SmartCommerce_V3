using SmartEcommerce.BuildingBlock.Domain.Abstractions;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Repository;

using SmartEcommerce.Product.Persistence.Engine;

namespace SmartEcommerce.Product.Persistence.Contexts;

public abstract class ProductBaseRepository<TEntity>(ProductDbContext context)
    : GenericRepository<ProductDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}
