using SmartEcommerce.Product.Persistence.Engine;

namespace SmartEcommerce.Product.Persistence.Contexts.ProductTags.Repositories;

public sealed class ProductTagRepo(ProductDbContext dbContext)
    : ProductBaseRepository<ProductTag>(dbContext), IProductTagRepository
{
}
