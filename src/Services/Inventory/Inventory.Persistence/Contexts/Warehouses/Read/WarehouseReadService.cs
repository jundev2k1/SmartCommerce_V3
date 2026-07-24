using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;

using Inventory.Application.Abstractions.Persistence.Warehouses;
using Inventory.Application.Features.Warehouses.Search;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.Warehouses.Read;

public sealed class WarehouseReadService(InventoryDbContext dbContext) : IWarehouseReadService
{
    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<Warehouse?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Code == code, ct);
    }

    public async Task<PaginatedResult<Warehouse>> SearchAsync(CriteriaRequest request, CancellationToken ct = default)
    {
        return await dbContext.Warehouses
            .AsNoTracking()
            .ApplyCriteria(WarehouseCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
