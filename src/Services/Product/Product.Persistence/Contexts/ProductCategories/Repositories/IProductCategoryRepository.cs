using BuildingBlock.Persistence.Repository;

namespace Product.Persistence.Contexts.ProductCategories.Repositories;

public interface IProductCategoryRepository : IRepository<ProductCategory>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
