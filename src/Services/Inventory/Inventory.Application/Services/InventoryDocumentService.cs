using Inventory.Application.Abstractions.Persistence.InventoryDocuments;
using Inventory.Application.Abstractions.Services;

namespace Inventory.Application.Services;

/// <summary>
/// Owns document lifecycle operations: creation, submission, approval, completion.
/// Ensures consistent workflow across all document types.
/// </summary>
public sealed class InventoryDocumentService(
    IInventoryDocumentReadService readService,
    IInventoryDocumentWriteService writeService) : IInventoryDocumentService
{
    /// <summary>Creates and immediately completes a document (for immediate operations).</summary>
    public async Task<InventoryDocument> CreateAndCompleteAsync(
        InventoryDocumentType type,
        InventoryDocumentReason reason,
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        string description,
        CancellationToken ct = default)
    {
        var document = InventoryDocument.Create(
            type: type,
            reason: reason,
            sourceWarehouseId: sourceWarehouseId,
            destinationWarehouseId: destinationWarehouseId,
            description: description);

        CompleteWorkflow(document);
        var request = new CreateInventoryDocumentRequest(
            Number: document.Number,
            Type: document.Type,
            Reason: document.Reason,
            SourceWarehouseId: document.SourceWarehouseId,
            DestinationWarehouseId: document.DestinationWarehouseId,
            Description: document.Description);
        await writeService.AddAsync(request, ct);

        return document;
    }

    /// <summary>Creates and immediately completes a document using explicit number (for idempotency).</summary>
    public async Task<InventoryDocument> CreateAndCompleteAsync(
        string number,
        InventoryDocumentType type,
        InventoryDocumentReason reason,
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        string description,
        CancellationToken ct = default)
    {
        var document = InventoryDocument.Create(
            number: number,
            type: type,
            reason: reason,
            sourceWarehouseId: sourceWarehouseId,
            destinationWarehouseId: destinationWarehouseId,
            description: description);

        CompleteWorkflow(document);
        var request = new CreateInventoryDocumentRequest(
            Number: document.Number,
            Type: document.Type,
            Reason: document.Reason,
            SourceWarehouseId: document.SourceWarehouseId,
            DestinationWarehouseId: document.DestinationWarehouseId,
            Description: document.Description);
        await writeService.AddAsync(request, ct);

        return document;
    }

    /// <summary>Creates a draft document for manual workflow (submit → approve → complete).</summary>
    public InventoryDocument CreateDraft(
        InventoryDocumentType type,
        InventoryDocumentReason reason,
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        string description = "")
    {
        return InventoryDocument.Create(
            type: type,
            reason: reason,
            sourceWarehouseId: sourceWarehouseId,
            destinationWarehouseId: destinationWarehouseId,
            description: description);
    }

    /// <summary>Transitions document through workflow: Draft → PendingApproval → Approved → Completed.</summary>
    public void CompleteWorkflow(InventoryDocument document)
    {
        if (document.Status == InventoryDocumentStatus.Draft)
            document.Submit();

        if (document.Status == InventoryDocumentStatus.PendingApproval)
            document.Approve(Guid.Empty);

        if (document.Status == InventoryDocumentStatus.Approved)
            document.Complete();
    }

    /// <summary>Saves a document to persistence.</summary>
    public async Task SaveAsync(InventoryDocument document, CancellationToken ct = default)
    {
        var request = new CreateInventoryDocumentRequest(
            Number: document.Number,
            Type: document.Type,
            Reason: document.Reason,
            SourceWarehouseId: document.SourceWarehouseId,
            DestinationWarehouseId: document.DestinationWarehouseId,
            Description: document.Description);
        await writeService.AddAsync(request, ct);
    }

    /// <summary>Retrieves an existing document by number (useful for idempotency checks).</summary>
    public async Task<InventoryDocument?> GetByNumberAsync(string number, CancellationToken ct = default)
    {
        return await readService.GetByNumberAsync(number, ct);
    }
}
