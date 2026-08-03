using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;
using SmartEcommerce.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace SmartEcommerce.Order.Persistence.Engine.UnitOfWork;

public sealed class UnitOfWork(OrderDbContext context)
    : EfUnitOfWork<OrderDbContext>(context), IUnitOfWork
{
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await base.SaveChangesAsync(ct);
    }

    public override async Task<bool> ExecuteTransactionAsync(
        Func<Task> action,
        Func<Task>? rollbackAction = null,
        CancellationToken ct = default)
    {
        return await base.ExecuteTransactionAsync(
            action,
            rollbackAction,
            ct);
    }
}
