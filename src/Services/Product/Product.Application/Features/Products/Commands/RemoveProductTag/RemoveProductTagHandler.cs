using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.Products.Commands.RemoveProductTag;

public sealed class RemoveProductTagHandler(
    IProductRepository productRepo,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<RemoveProductTagCommand, RemoveProductTagResponse>
{
    public async Task<RemoveProductTagResponse> Handle(RemoveProductTagCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productRepo.UpdateAsync(request.ProductId, async (product) =>
            {
                product.RemoveTag(request.TagId);
                await Task.CompletedTask;
            }, ct);

            await outboxStore.EnqueueAsync(
                new ProductTagRemovedIntegrationEvent(request.ProductId, request.TagId, correlationId), ct);
        }, ct: ct);

        return new RemoveProductTagResponse();
    }
}
