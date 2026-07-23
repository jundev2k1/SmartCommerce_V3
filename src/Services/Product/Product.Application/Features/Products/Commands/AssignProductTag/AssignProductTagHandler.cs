using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;
using Product.Application.Abstractions.Persistence.ProductTags;

namespace Product.Application.Features.Products.Commands.AssignProductTag;

public sealed class AssignProductTagHandler(
    IProductWriteService productWriteService,
    IProductTagReadService tagReadService,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<AssignProductTagCommand, AssignProductTagResponse>
{
    public async Task<AssignProductTagResponse> Handle(AssignProductTagCommand request, CancellationToken ct = default)
    {
        _ = await tagReadService.GetByIdAsync(request.TagId, ct)
            ?? throw new NotFoundException(nameof(ProductTag), request.TagId);

        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await outboxStore.EnqueueAsync(
            new ProductTagAssignedIntegrationEvent(request.ProductId, request.TagId, correlationId), ct);

        await productWriteService.UpdateAsync(request.ProductId, async (product) =>
        {
            product.AssignTag(request.TagId);
            await Task.CompletedTask;
        }, ct);

        return new AssignProductTagResponse();
    }
}
