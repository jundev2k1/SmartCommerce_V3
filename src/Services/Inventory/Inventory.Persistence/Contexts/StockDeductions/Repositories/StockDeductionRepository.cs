using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.StockDeductions.Repositories;

public sealed class StockDeductionRepository(InventoryDbContext dbContext)
    : InventoryBaseRepository<StockDeduction, Guid>(dbContext), IStockDeductionRepository
{
}
