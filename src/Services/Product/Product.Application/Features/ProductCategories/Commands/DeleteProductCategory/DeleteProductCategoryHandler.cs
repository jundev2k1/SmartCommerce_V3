using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;

namespace Product.Application.Features.ProductCategories.Commands.DeleteProductCategory;

public sealed class DeleteProductCategoryHandler(
    IProductCategoryRepository categoryRepo,
    IProductRepository productRepo,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteProductCategoryCommand, DeleteProductCategoryResponse>
{
    public async Task<DeleteProductCategoryResponse> Handle(DeleteProductCategoryCommand request, CancellationToken ct = default)
    {
        _ = await categoryRepo.GetByIdAsync(request.ProductCategoryId, ct)
            ?? throw new NotFoundException(nameof(ProductCategory), request.ProductCategoryId);

        if (await categoryRepo.HasChildrenAsync(request.ProductCategoryId, ct))
            throw new ConflictException("Cannot delete a category that has child categories.");

        if (await productRepo.ExistsWithCategoryAsync(request.ProductCategoryId, ct))
            throw new ConflictException("Cannot delete a category that is still assigned to products.");

        await categoryRepo.DeleteAsync(request.ProductCategoryId, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new DeleteProductCategoryResponse();
    }
}
