using SmartEcommerce.BuildingBlock.Application.Exceptions;

using SmartEcommerce.Product.Application.Abstractions.Persistence.ProductCategories;
using SmartEcommerce.Product.Domain.ValueObjects;

namespace SmartEcommerce.Product.Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed class CreateProductCategoryHandler(
    IProductCategoryReadService categoryReadService,
    IProductCategoryWriteService categoryWriteService) : ICommandHandler<CreateProductCategoryCommand, CreateProductCategoryResponse>
{
    public async Task<CreateProductCategoryResponse> Handle(CreateProductCategoryCommand request, CancellationToken ct = default)
    {
        if (await categoryReadService.CodeExistsAsync(request.Code, ct))
            throw new ConflictException($"ProductCategory with code ({request.Code}) already exists");

        if (request.ParentCategoryId is not null)
        {
            _ = await categoryReadService.GetByIdAsync(request.ParentCategoryId.Value, ct)
                ?? throw new NotFoundException(nameof(ProductCategory), request.ParentCategoryId.Value);
        }

        var category = ProductCategory.Create(
            Guid.CreateVersion7(),
            CategoryCode.Create(request.Code),
            request.Name.Trim(),
            request.Description.Trim(),
            request.ParentCategoryId);
        await categoryWriteService.CreateAsync(category, ct);

        return new CreateProductCategoryResponse(category.Id);
    }
}
