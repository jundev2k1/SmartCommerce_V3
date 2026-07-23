using BuildingBlock.Application.Abstractions.Persistence;
using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Persistence.Inventories;
using Inventory.Persistence.Inventories.Repositories;

namespace Inventory.Persistence.Inventories.Write;

public sealed class InventoryWriteService(
    InventoryDbContext dbContext,
    IInventoryRepository repo,
    IUnitOfWork unitOfWork) : IInventoryWriteService
{
    public async Task AddAsync(InventoryEntity entity, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.AddAsync(entity, ct);
        }, ct: ct);
    }

    public async Task DeleteByProductIdAsync(Guid productId, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.DeleteByProductIdAsync(productId, ct);
        }, ct: ct);
    }

    public async Task DeleteByVariationIdAsync(Guid productVariationId, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.DeleteByVariationIdAsync(productVariationId, ct);
        }, ct: ct);
    }

    public async Task StageUpdateAsync(Guid id, Func<InventoryEntity, Task> updateAction, CancellationToken ct = default)
    {
        var inventory = await dbContext.Inventories
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new NotFoundException(nameof(InventoryEntity), id);

        await updateAction(inventory);
    }
}
