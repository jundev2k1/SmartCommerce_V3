namespace Inventory.Application.Abstractions.Persistence.Warehouses;

public interface IWarehouseWriteService
{
    /// <summary>Commits via bare SaveChangesAsync, matching CreateWarehouseHandler's current shape (not ExecuteTransactionAsync).</summary>
    Task CreateAsync(Warehouse warehouse, CancellationToken ct = default);
}
