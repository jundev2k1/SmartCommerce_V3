namespace Inventory.Application.Abstractions.Persistence.InventoryDocuments;

public sealed record CreateInventoryDocumentRequest(
    string Number,
    InventoryDocumentType Type,
    InventoryDocumentReason Reason,
    Guid? SourceWarehouseId = null,
    Guid? DestinationWarehouseId = null,
    string Description = "");

public interface IInventoryDocumentWriteService
{
    Task AddAsync(CreateInventoryDocumentRequest request, CancellationToken ct = default);
}
