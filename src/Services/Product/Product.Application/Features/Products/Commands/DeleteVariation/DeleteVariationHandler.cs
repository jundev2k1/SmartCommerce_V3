using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;

namespace Product.Application.Features.Products.Commands.DeleteVariation;

/// <summary>Aggregate enforces "cannot remove the last variation" and auto-promotes a new Default if needed - see Product.RemoveVariation.</summary>
public sealed class DeleteVariationHandler(
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<DeleteVariationCommand, DeleteVariationResponse>
{
    public async Task<DeleteVariationResponse> Handle(DeleteVariationCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.DeleteVariationAsync(request.VariationId, ct);

            await outboxStore.EnqueueAsync(
                new ProductVariationDeletedIntegrationEvent(request.ProductId, request.VariationId, correlationId),
                ct);
        }, ct: ct);

        return new DeleteVariationResponse();
    }
}
