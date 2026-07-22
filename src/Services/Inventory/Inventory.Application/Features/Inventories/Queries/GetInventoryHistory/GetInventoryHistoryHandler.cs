using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Repositories;

using Mapster;

namespace Inventory.Application.Features.Inventories.Queries.GetInventoryHistory;

public sealed class GetInventoryHistoryHandler(
    IInventoryRepository inventoryRepo,
    IInventoryTransactionRepository transactionRepo) : IQueryHandler<GetInventoryHistoryQuery, GetInventoryHistoryResponse>
{
    public async Task<GetInventoryHistoryResponse> Handle(GetInventoryHistoryQuery request, CancellationToken ct = default)
    {
        _ = await inventoryRepo.GetByIdAsync(request.InventoryId, ct)
            ?? throw new NotFoundException("Inventory", request.InventoryId);

        var transactions = await transactionRepo.GetHistoryAsync(request.InventoryId, ct);

        return new GetInventoryHistoryResponse(transactions.Adapt<List<InventoryTransactionDto>>());
    }
}
