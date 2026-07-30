using System.Linq.Expressions;

using BuildingBlock.Application.Exceptions;
using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Persistence.Repository;

using Microsoft.EntityFrameworkCore;
using EfDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace BuildingBlock.Persistence.Ef.Repository;

public abstract class GenericRepository<TContext, TEntity>(TContext context) : IRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : EfDbContext
{
    protected readonly TContext _dbContext = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    private static Expression<Func<TEntity, bool>> BuildIdEqualsExpression<TId>(TId id)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entry");
        var property = Expression.Property(parameter, nameof(IEntity<TId>.Id));
        var constant = Expression.Constant(id, typeof(TId));
        var equals = Expression.Equal(property, constant);

        return Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
    }

    public async Task<TEntity?> GetByIdAsync<TId>(TId id, CancellationToken ct = default)
    {
        var predicate = BuildIdEqualsExpression(id);
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, ct);
    }

    public async Task<TEntity?> GetByIdAsync<TId>(TId id, Func<IQueryable<TEntity>, IQueryable<TEntity>> includes, CancellationToken ct = default)
    {
        var query = includes(_dbSet);
        var predicate = BuildIdEqualsExpression(id);
        return await query.FirstOrDefaultAsync(predicate, ct);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
    {
        await _dbSet.AddRangeAsync(entities, ct);
    }

    public async Task UpdateAsync<TId>(
        TId id,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var predicate = BuildIdEqualsExpression(id);
        var entity = await _dbSet.FirstOrDefaultAsync(predicate, ct)
            ?? throw new NotFoundException($"Entity with ID {id} not found.");

        await updateAction(entity);
    }
    public async Task UpdateAsync<TId>(
        TId id,
        Action<TEntity> updateAction,
        CancellationToken ct = default)
    {
        var predicate = BuildIdEqualsExpression(id);
        var entity = await _dbSet.FirstOrDefaultAsync(predicate, ct)
            ?? throw new NotFoundException($"Entity with ID {id} not found.");

        updateAction?.Invoke(entity);
    }
    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Func<TEntity, Task> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(_dbSet);
        var predicate = BuildIdEqualsExpression(id);
        var entity = await query.FirstOrDefaultAsync(predicate, ct)
            ?? throw new NotFoundException($"Entity with ID {id} not found.");

        await updateAction(entity);
    }
    public async Task UpdateAsync<TId>(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>> includes,
        Action<TEntity> updateAction,
        CancellationToken ct = default)
    {
        var query = includes(_dbSet);
        var predicate = BuildIdEqualsExpression(id);
        var entity = await query.FirstOrDefaultAsync(predicate, ct)
            ?? throw new NotFoundException($"Entity with ID {id} not found.");

        updateAction?.Invoke(entity);
    }

    public async Task DeleteAsync<TId>(TId id, CancellationToken ct = default)
    {
        var predicate = BuildIdEqualsExpression(id);
        var entity = await _dbSet.FirstOrDefaultAsync(predicate, ct)
            ?? throw new NotFoundException($"Entity with ID {id} not found.");

        _dbSet.Remove(entity);
    }

    public async Task DeleteRangeAsync<TId>(TId[] ids, CancellationToken ct = default)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "entry");
        var property = Expression.Property(parameter, nameof(IEntity<TId>.Id));

        var containsMethod = typeof(Enumerable)
            .GetMethods()
            .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TId));

        var idsConstant = Expression.Constant(ids);
        var body = Expression.Call(containsMethod, idsConstant, property);
        var predicate = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

        var entitiesToDelete = await _dbSet.Where(predicate)
            .ToArrayAsync(ct);

        _dbSet.RemoveRange(entitiesToDelete);
    }

    public async Task DeleteWithNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default)
    {
        await _dbSet.Where(predicate).ExecuteDeleteAsync(ct);
    }
}
