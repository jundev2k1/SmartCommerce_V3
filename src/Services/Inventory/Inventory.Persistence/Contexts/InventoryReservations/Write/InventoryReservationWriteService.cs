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

        await repo.AddAsync(entity, ct);
        await unitOfWork.ExecuteTransactionAsync(ct);
    }

    public async Task DeleteByInventoryIdAsync(Guid inventoryId, CancellationToken ct = default)
    {
        var reservations = await repo.GetByInventoryIdAsync(inventoryId, ct);
        foreach (var reservation in reservations)
        {
            repo.Remove(reservation);
        }
        await unitOfWork.ExecuteTransactionAsync(ct);
    }
}
