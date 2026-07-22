using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.Products.Commands.RemoveProductCategory;

public sealed class RemoveProductCategoryHandler(
    IProductRepository productRepo,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<RemoveProductCategoryCommand, RemoveProductCategoryResponse>
{
    public async Task<RemoveProductCategoryResponse> Handle(RemoveProductCategoryCommand request, CancellationToken ct = default)
    {
        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productRepo.UpdateAsync(request.ProductId, async (product) =>
            {
                product.RemoveCategory(request.CategoryId);
                await Task.CompletedTask;
            }, ct);

            await outboxStore.EnqueueAsync(
                new ProductCategoryRemovedIntegrationEvent(request.ProductId, request.CategoryId, correlationId), ct);
        }, ct: ct);

        return new RemoveProductCategoryResponse();
    }
}
