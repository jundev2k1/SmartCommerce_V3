using SmartEcommerce.BuildingBlock.Application.Abstractions.Common;
using SmartEcommerce.BuildingBlock.Criteria.Requests;
using SmartEcommerce.BuildingBlock.Persistence.Ef.Criteria;
using SmartEcommerce.Inventory.Application.Features.Inventories.Search;
using SmartEcommerce.Inventory.Application.Features.InventorySerials.Search;
using SmartEcommerce.Inventory.Persistence.Engine;

namespace SmartEcommerce.Inventory.Persistence.Contexts.InventorySerials.Repositories;

public sealed class InventorySerialRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<InventorySerial, Guid>(dbContext), IInventorySerialRepository
{
    public async Task<InventorySerial?> GetBySerialNumberAsync(
        string serialNumber,
        CancellationToken ct = default)
    {
        return await _dbContext.InventorySerials
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SerialNumber == serialNumber, ct);
    }

    public async Task<IReadOnlyList<InventorySerial>> GetByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventorySerials
            .AsNoTracking()
            .Where(s => s.InventoryId == inventoryId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<InventorySerial>> GetAvailableByInventoryIdAsync(
        Guid inventoryId,
        CancellationToken ct = default)
    {
        return await _dbContext.InventorySerials
            .AsNoTracking()
            .Where(s => s.InventoryId == inventoryId &&
                        s.Status == InventorySerialStatus.Available)
            .ToListAsync(ct);
    }

    public async Task<PaginatedResult<InventorySerial>> SearchAsync(
        CriteriaRequest request,
        CancellationToken ct = default)
    {
        return await _dbContext.InventorySerials
            .AsNoTracking()
            .ApplyCriteria(InventorySerialCriteriaDefinition.Instance, request)
            .ToCriteriaPagedResultAsync(request, ct);
    }
}
