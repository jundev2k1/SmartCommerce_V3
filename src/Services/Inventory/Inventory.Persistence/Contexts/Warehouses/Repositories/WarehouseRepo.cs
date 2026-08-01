using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;
using Inventory.Application.Features.Warehouses.Search;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.Warehouses.Repositories;

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
