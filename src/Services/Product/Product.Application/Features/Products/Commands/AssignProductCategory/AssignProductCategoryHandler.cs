using BuildingBlock.Application.Abstractions.Outbox;
using BuildingBlock.Application.Abstractions.Services;
using BuildingBlock.Application.Exceptions;
using BuildingBlock.Contract.Events.Product;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.Products.Commands.AssignProductCategory;

public sealed class AssignProductCategoryHandler(
    IProductRepository productRepo,
    IProductCategoryRepository categoryRepo,
    IUnitOfWork unitOfWork,
    IOutboxStore outboxStore,
    ICurrentUserService currentUser) : ICommandHandler<AssignProductCategoryCommand, AssignProductCategoryResponse>
{
    public async Task<AssignProductCategoryResponse> Handle(AssignProductCategoryCommand request, CancellationToken ct = default)
    {
        _ = await categoryRepo.GetByIdAsync(request.CategoryId, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.CategoryId);

        var correlationId = currentUser.GetCorrelationId() ?? Guid.NewGuid().ToString();

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productRepo.UpdateAsync(request.ProductId, async (product) =>
            {
                product.AssignCategory(request.CategoryId);
                await Task.CompletedTask;
            }, ct);

            await outboxStore.EnqueueAsync(
                new ProductCategoryAssignedIntegrationEvent(request.ProductId, request.CategoryId, correlationId), ct);
        }, ct: ct);

        return new AssignProductCategoryResponse();
    }
}
