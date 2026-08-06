namespace NovaCore.Promotion.Domain.Entities.Gifts;

/// <summary>A member product/variant of a GiftProgram - Quantity reuses the shared BuildingBlock Value Object.</summary>
public sealed class GiftItem : BaseEntity<Guid>, IAuditable
{
    public Guid ProgramId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? VariantId { get; private set; }
    public Quantity Quantity { get; private set; } = default!;

    private GiftItem() { }

    /// <summary>Only GiftProgram may construct a GiftItem - see GiftProgram.AddItem.</summary>
    internal static GiftItem Create(Guid programId, Guid productId, Quantity quantity, Guid? variantId)
    {
        return new GiftItem
        {
            Id = Guid.CreateVersion7(),
            ProgramId = programId,
            ProductId = productId,
            VariantId = variantId,
            Quantity = quantity,
        };
    }

    public void UpdateQuantity(Quantity quantity)
    {
        Quantity = quantity;
    }
}
