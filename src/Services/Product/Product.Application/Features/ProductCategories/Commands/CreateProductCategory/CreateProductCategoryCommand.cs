namespace Product.Application.Features.ProductCategories.Commands.CreateProductCategory;

public sealed record CreateProductCategoryCommand(
    string Code,
    string Name,
    string Description,
    Guid? ParentCategoryId = null) : ICommand<CreateProductCategoryResponse>;

public sealed record CreateProductCategoryResponse(Guid ProductCategoryId);
