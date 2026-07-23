using BuildingBlock.Application.Exceptions;

using Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using Inventory.Application.Abstractions.Persistence.Inventories;

using Mapster;

namespace Inventory.Application.Features.Inventories.Queries.GetInventoryHistory;

public sealed class GetInventoryHistoryHandler(
    IInventoryReadService inventoryReadService,
    IInventoryTransactionReadService transactionReadService) : IQueryHandler<GetInventoryHistoryQuery, GetInventoryHistoryResponse>
{
    public async Task<GetInventoryHistoryResponse> Handle(GetInventoryHistoryQuery request, CancellationToken ct = default)
    {
        _ = await inventoryReadService.GetByIdAsync(request.InventoryId, ct)
            ?? throw new NotFoundException("Inventory", request.InventoryId);

        var transactions = await transactionReadService.GetHistoryAsync(request.InventoryId, ct);

        return new GetInventoryHistoryResponse(transactions.Adapt<List<InventoryTransactionDto>>());
    }
}
