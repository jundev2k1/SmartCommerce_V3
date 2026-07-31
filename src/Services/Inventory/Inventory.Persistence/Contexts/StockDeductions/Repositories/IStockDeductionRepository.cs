using BuildingBlock.Persistence.Repository;

namespace Inventory.Persistence.Contexts.StockDeductions.Repositories;

public interface IStockDeductionRepository : IRepository<StockDeduction, Guid>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
