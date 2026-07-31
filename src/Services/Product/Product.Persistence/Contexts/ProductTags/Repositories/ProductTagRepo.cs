using Product.Persistence.Engine;

namespace Product.Persistence.Contexts.ProductTags.Repositories;

public sealed class ProductTagRepo(ProductDbContext dbContext)
    : ProductBaseRepository<ProductTag>(dbContext), IProductTagRepository
{
}
