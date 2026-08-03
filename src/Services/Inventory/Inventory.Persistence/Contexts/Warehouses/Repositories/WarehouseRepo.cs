using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Criteria;
using SmartEcommerce.Inventory.Application.Features.Warehouses.Search;
using SmartEcommerce.Inventory.Persistence.Engine;

namespace SmartEcommerce.Inventory.Persistence.Contexts.Warehouses.Repositories;

public sealed class WarehouseRepo(InventoryDbContext dbContext)
    : InventoryBaseRepository<Warehouse, Guid>(dbContext), IWarehouseRepository
{
    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == code, ct);
    }

    public async Task<PaginatedResult<Warehouse>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .ApplyCriteria(WarehouseCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
