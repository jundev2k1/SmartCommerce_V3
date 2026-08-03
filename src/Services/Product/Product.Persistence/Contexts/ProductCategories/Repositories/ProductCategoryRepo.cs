using SmartEcommerce.Product.Persistence.Engine;

namespace SmartEcommerce.Product.Persistence.Contexts.ProductCategories.Repositories;

public sealed class ProductCategoryRepo(ProductDbContext dbContext)
    : ProductBaseRepository<ProductCategory>(dbContext), IProductCategoryRepository
{
}
