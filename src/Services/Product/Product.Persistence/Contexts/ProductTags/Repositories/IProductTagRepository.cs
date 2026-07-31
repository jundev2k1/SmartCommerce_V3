using BuildingBlock.Persistence.Repository;

namespace Product.Persistence.Contexts.ProductTags.Repositories;

public interface IProductTagRepository : IRepository<ProductTag>
{
    // Leave empty for now... Reserved for future scaling if the repository requires specific functions
}
