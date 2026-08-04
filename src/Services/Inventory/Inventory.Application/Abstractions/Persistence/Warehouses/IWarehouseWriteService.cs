using SmartEcommerce.Inventory.Application.Features.Warehouses.DTOs;

namespace SmartEcommerce.Inventory.Application.Abstractions.Persistence.Warehouses;

public interface IWarehouseWriteService
{
    /// <summary>
    /// Commits via bare SaveChangesAsync, matching CreateWarehouseHandler's current shape (not
    /// ExecuteTransactionAsync). Returns the created Id only - CreateWarehouseHandler needs
    /// nothing else from the entity.
    /// </summary>
    Task<Guid> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default);
}
