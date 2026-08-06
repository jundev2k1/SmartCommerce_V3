namespace NovaCore.Promotion.Domain.Entities.ProductSets;

/// <summary>A priced offer for a ProductBundle - reuses the shared Money Value Object, same split (Currency scalar + currency-less Money) already used by Voucher/CampaignBudget. Not navigated from ProductBundle, so construction is public.</summary>
public sealed class BundlePrice : BaseEntity<Guid>, IAuditable
{
    public Guid BundleId { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public Money Price { get; private set; } = default!;

    private BundlePrice() { }

    public static BundlePrice Create(Guid bundleId, string currency, Money price)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw ExceptionFactory.RequiredField("Currency cannot be empty.");

        return new BundlePrice
        {
            Id = Guid.CreateVersion7(),
            BundleId = bundleId,
            Currency = currency,
            Price = price,
        };
    }

    public void UpdatePrice(Money price)
    {
        Price = price;
    }
}
