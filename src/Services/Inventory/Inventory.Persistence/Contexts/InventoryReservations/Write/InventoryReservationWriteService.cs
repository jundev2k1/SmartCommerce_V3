using BuildingBlock.Application.Abstractions.Persistence;
using Inventory.Application.Abstractions.Persistence.InventoryReservations;
using Inventory.Persistence.Contexts.InventoryReservations.Repositories;
using Inventory.Persistence.Engine;

namespace Inventory.Persistence.Contexts.InventoryReservations.Write;

public sealed class InventoryReservationWriteService(
    IInventoryReservationRepository repo,
    IUnitOfWork unitOfWork) : IInventoryReservationWriteService
{
    public async Task AddAsync(CreateInventoryReservationRequest request, CancellationToken ct = default)
    {
        var entity = InventoryReservation.Create(
            request.Number,
            request.Type,
            request.InventoryId,
            request.WarehouseId,
            request.ProductId,
            request.ProductVariantId,
            request.Quantity,
            request.ReferenceType,
            request.ReferenceId,
            request.ExternalReference,
            request.ExpiredAt,
            request.Reason);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.AddAsync(entity, ct);
        }, ct: ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await repo.DeleteWithNoTrackingAsync(r => r.InventoryId == inventoryId, ct);
        }, ct: ct);
    }
}
