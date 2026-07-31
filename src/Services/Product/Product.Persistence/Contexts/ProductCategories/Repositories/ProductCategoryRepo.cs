using Product.Persistence.Engine;

namespace Product.Persistence.Contexts.ProductCategories.Repositories;

public sealed class ProductCategoryRepo(ProductDbContext dbContext)
    : ProductBaseRepository<ProductCategory>(dbContext), IProductCategoryRepository
{
}
