namespace Product.Domain.Entities;

/// <summary>
/// Explicit many-to-many join entity between Product and ProductTag - see
/// ProductCategoryMapping remarks for why this exists instead of a raw id collection.
/// </summary>
public sealed class ProductTagMapping : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public Guid TagId { get; private set; }

    private ProductTagMapping() { }

    internal static ProductTagMapping Create(Guid id, Guid productId, Guid tagId)
    {
        return new ProductTagMapping
        {
            Id = id,
            ProductId = productId,
            TagId = tagId,
        };
    }
}
