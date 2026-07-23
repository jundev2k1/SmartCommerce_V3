using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;

namespace Product.Application.Features.Products.Commands.RemoveProductTag;

public sealed class RemoveProductTagHandler(
    IProductWriteService productWriteService,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<RemoveProductTagCommand, RemoveProductTagResponse>
{
    public async Task<RemoveProductTagResponse> Handle(RemoveProductTagCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await outboxStore.EnqueueAsync(
            new ProductTagRemovedIntegrationEvent(request.ProductId, request.TagId, correlationId), ct);

        await productWriteService.UpdateAsync(request.ProductId, async (product) =>
        {
            product.RemoveTag(request.TagId);
            await Task.CompletedTask;
        }, ct);

        return new RemoveProductTagResponse();
    }
}
