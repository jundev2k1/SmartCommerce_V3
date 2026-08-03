using SmartEcommerce.BuildingBlock.Application.Abstractions.Outbox;
using SmartEcommerce.BuildingBlock.Application.Abstractions.Services;
using SmartEcommerce.BuildingBlock.Application.Exceptions;
using SmartEcommerce.BuildingBlock.Contract.Events.Product;

using SmartEcommerce.Product.Application.Abstractions.Persistence.ProductCategories;
using SmartEcommerce.Product.Application.Abstractions.Persistence.Products;

namespace SmartEcommerce.Product.Application.Features.Products.Commands.AssignProductCategory;

public sealed class AssignProductCategoryHandler(
    IProductWriteService productWriteService,
    IProductCategoryReadService categoryReadService,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<AssignProductCategoryCommand, AssignProductCategoryResponse>
{
    public async Task<AssignProductCategoryResponse> Handle(AssignProductCategoryCommand request, CancellationToken ct = default)
    {
        _ = await categoryReadService.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.CategoryId);

        var correlationId = currentUser.GetCorrelationId();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productWriteService.AssignCategoryAsync(
                request.ProductId,
                request.CategoryId,
                ct);

            await outboxStore.EnqueueAsync(
                new ProductCategoryAssignedIntegrationEvent(
                    request.ProductId,
                    request.CategoryId,
                    correlationId),
                ct);
        }, ct: ct);

        return new AssignProductCategoryResponse();
    }
}
