using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;

using SmartEcommerce.Product.Application.Abstractions.Persistence.Products;

namespace SmartEcommerce.Product.Application.Features.Products.Commands.DeleteVariation;

/// <summary>Aggregate enforces "cannot remove the last variation" and auto-promotes a new Default if needed - see Product.RemoveVariation.</summary>
public sealed class DeleteVariationHandler(
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<DeleteVariationCommand, DeleteVariationResponse>
{
    public async Task<DeleteVariationResponse> Handle(DeleteVariationCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.DeleteVariationAsync(
                request.VariationId,
                ct);

            await outboxStore.EnqueueAsync(
                new ProductVariationDeletedIntegrationEvent(
                    request.ProductId,
                    request.VariationId,
                    correlationId),
                ct);
        }, ct: ct);

        return new DeleteVariationResponse();
    }
}
