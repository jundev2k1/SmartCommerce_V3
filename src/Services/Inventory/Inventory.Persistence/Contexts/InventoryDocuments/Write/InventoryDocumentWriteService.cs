using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using Inventory.Persistence.Contexts.InventoryDocuments.Repositories;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.InventoryDocuments.Write;

public sealed class InventoryDocumentWriteService(
    IInventoryDocumentRepository repo,
    IUnitOfWork unitOfWork) : IInventoryDocumentWriteService
{
    public async Task AddAsync(CreateInventoryDocumentRequest request, CancellationToken ct = default)
    {
        var entity = InventoryDocument.Create(
            request.Number,
            request.Type,
            request.Reason,
            request.SourceWarehouseId,
            request.DestinationWarehouseId,
            request.Description);

        await repo.AddAsync(entity, ct);
        await unitOfWork.ExecuteTransactionAsync(ct);
    }
}
