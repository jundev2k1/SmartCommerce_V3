using SmartEcommerce.BuildingBlock.Application.Abstractions.Persistence;
using SmartEcommerce.BuildingBlock.Persistence.Ef.UnitOfWork;

namespace SmartEcommerce.User.Persistence.Engine.UnitOfWork;

public sealed class UnitOfWork(UserDbContext context)
    : EfUnitOfWork<UserDbContext>(context), IUnitOfWork
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
