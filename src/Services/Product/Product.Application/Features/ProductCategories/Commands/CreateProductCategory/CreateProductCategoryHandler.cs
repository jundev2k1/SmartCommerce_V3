using BuildingBlock.Application.Exceptions;

using Product.Application.Abstractions.Repositories;
using Product.Domain.ValueObjects;

namespace Product.Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed class CreateProductCategoryHandler(
    IProductCategoryRepository categoryRepo,
    IUnitOfWork uow) : ICommandHandler<CreateProductCategoryCommand, CreateProductCategoryResponse>
{
    public async Task<CreateProductCategoryResponse> Handle(CreateProductCategoryCommand request, CancellationToken ct = default)
    {
        if (await categoryRepo.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"ProductCategory with code ({request.Code}) already exists");

        if (request.ParentCategoryId is not null)
        {
            _ = await categoryRepo.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new NotFoundException(nameof(ProductCategory), request.ParentCategoryId.Value);
        }

        var category = ProductCategory.Create(
            Guid.CreateVersion7(),
            CategoryCode.Create(request.Code),
            request.Name.Trim(),
            request.Description.Trim(),
            request.ParentCategoryId);
        await categoryRepo.AddAsync(category, ct);
        await uow.SaveChangesAsync(ct);

        return new CreateProductCategoryResponse(category.Id);
    }
}
