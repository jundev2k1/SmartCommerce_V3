using NovaCore.BuildingBlock.Domain.Abstractions;
using NovaCore.BuildingBlock.Persistence.Ef.Repository;

using NovaCore.Promotion.Persistence.Engine;

namespace NovaCore.Promotion.Persistence.Contexts;

public abstract class PromotionBaseRepository<TEntity>(PromotionDbContext context)
    : GenericRepository<PromotionDbContext, TEntity>(context)
    where TEntity : class, IEntity
{
}

public abstract class PromotionBaseRepository<TEntity, TId>(PromotionDbContext context)
    : EntityGenericRepository<PromotionDbContext, TEntity, TId>(context)
    where TEntity : class, IEntity<TId>
{
}
