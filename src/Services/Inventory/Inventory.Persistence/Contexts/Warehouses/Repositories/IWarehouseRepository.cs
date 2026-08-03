using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Repository;

namespace SmartEcommerce.Inventory.Persistence.Contexts.Warehouses.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse, Guid>
{
    Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default);

    Task<PaginatedResult<Warehouse>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default);
}
