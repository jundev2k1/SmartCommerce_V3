using SmartEcommerce.BuildingBlock.Domain.Abstractions;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Repository;

using SmartEcommerce.Inventory.Persistence.Engine;

namespace SmartEcommerce.Inventory.Persistence.Contexts;

public abstract class InventoryBaseRepository<TEntity>(InventoryDbContext context)
    : GenericRepository<InventoryDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class InventoryBaseRepository<TEntity, TId>(InventoryDbContext context)
    : EntityGenericRepository<InventoryDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
