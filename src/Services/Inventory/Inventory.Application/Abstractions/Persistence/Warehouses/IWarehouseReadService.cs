using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;

namespace SmartEcommerce.Inventory.Application.Abstractions.Persistence.Warehouses;

public interface IWarehouseReadService
{
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<PaginatedResult<Warehouse>> SearchAsync(CriteriaRequest request, CancellationToken ct = default);
}
