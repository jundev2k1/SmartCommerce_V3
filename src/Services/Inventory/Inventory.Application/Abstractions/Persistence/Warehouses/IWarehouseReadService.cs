namespace Inventory.Application.Abstractions.Persistence.Warehouses;

public interface IWarehouseReadService
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default);
}
