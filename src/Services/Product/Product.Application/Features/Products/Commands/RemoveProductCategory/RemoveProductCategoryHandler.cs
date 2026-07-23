using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.Products;

namespace Product.Application.Features.Products.Commands.RemoveProductCategory;

public sealed class RemoveProductCategoryHandler(
    IProductWriteService productWriteService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<RemoveProductCategoryCommand, RemoveProductCategoryResponse>
{
    public async Task<RemoveProductCategoryResponse> Handle(RemoveProductCategoryCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.RemoveCategoryAsync(request.ProductId, request.CategoryId, ct);

            await outboxStore.EnqueueAsync(
                new ProductCategoryRemovedIntegrationEvent(request.ProductId, request.CategoryId, correlationId), ct);
        }, ct: ct);

        return new RemoveProductCategoryResponse();
    }
}
