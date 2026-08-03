using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;

using SmartEcommerce.Product.Application.Abstractions.Persistence.Products;

namespace SmartEcommerce.Product.Application.Features.Products.Commands.RemoveProductTag;

public sealed class RemoveProductTagHandler(
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<RemoveProductTagCommand, RemoveProductTagResponse>
{
    public async Task<RemoveProductTagResponse> Handle(RemoveProductTagCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.RemoveTagAsync(
                request.ProductId,
                request.TagId,
                ct);

            await outboxStore.EnqueueAsync(
                new ProductTagRemovedIntegrationEvent(
                    request.ProductId,
                    request.TagId,
                    correlationId),
                ct);
        }, ct: ct);

        return new RemoveProductTagResponse();
    }
}
