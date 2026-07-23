using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Persistence.ProductCategories;
using Product.Application.Abstractions.Persistence.Products;

namespace Product.Application.Features.Products.Commands.AssignProductCategory;

public sealed class AssignProductCategoryHandler(
    IProductWriteService productWriteService,
    IProductCategoryReadService categoryReadService,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<AssignProductCategoryCommand, AssignProductCategoryResponse>
{
    public async Task<AssignProductCategoryResponse> Handle(AssignProductCategoryCommand request, CancellationToken ct = default)
    {
        _ = await categoryReadService.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.CategoryId);

        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await outboxStore.EnqueueAsync(
            new ProductCategoryAssignedIntegrationEvent(request.ProductId, request.CategoryId, correlationId), ct);

        await productWriteService.UpdateAsync(request.ProductId, async (product) =>
        {
            product.AssignCategory(request.CategoryId);
            await Task.CompletedTask;
        }, ct);

        return new AssignProductCategoryResponse();
    }
}
