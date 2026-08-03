using SmartEcommerce.BuildingBlock.Application.Exceptions;

using SmartEcommerce.Inventory.Application.Abstractions.Persistence.InventoryTransactions;
using SmartEcommerce.Inventory.Application.Abstractions.Persistence.Inventories;

using Mapster;

namespace SmartEcommerce.Inventory.Application.Features.Inventories.Queries.GetInventoryHistory;

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
