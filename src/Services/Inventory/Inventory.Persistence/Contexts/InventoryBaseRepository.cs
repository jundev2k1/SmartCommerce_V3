using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Persistence.Ef.Repository;

using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts;

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
