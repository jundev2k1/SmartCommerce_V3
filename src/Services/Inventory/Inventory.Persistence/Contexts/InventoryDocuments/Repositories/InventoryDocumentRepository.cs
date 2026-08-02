using BuildingBlock.Application.Abstractions.Common;
using BuildingBlock.Criteria.Requests;
using BuildingBlock.Persistence.Ef.Criteria;
using Inventory.Application.Features.InventoryDocuments.Search;
using Inventory.Application.Features.Inventories.Search;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.InventoryDocuments.Repositories;

public sealed class InventoryDocumentRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventoryDocument, Guid>(dbContext), IInventoryDocumentRepository
{
    public async Task<InventoryDocument?> GetByNumberAsync(
        string number,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Number == number, ct);
    }

    public async Task<IReadOnlyList<InventoryDocument>> GetBySourceWarehouseIdAsync(
        Guid sourceWarehouseId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryDocuments
            .AsNoTracking()
            .Where(d => d.SourceWarehouseId == sourceWarehouseId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventoryDocument>> GetByDestinationWarehouseIdAsync(
        Guid destinationWarehouseId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryDocuments
            .AsNoTracking()
            .Where(d => d.DestinationWarehouseId == destinationWarehouseId)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventoryDocument>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventoryDocuments
            .AsNoTracking()
            .ApplyCriteria(InventoryDocumentCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
