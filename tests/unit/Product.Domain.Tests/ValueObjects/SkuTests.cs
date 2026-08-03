using SmartEcommerce.Product.Domain.ValueObjects;

namespace SmartEcommerce.Product.Domain.Tests.ValueObjects;

public class SkuTests : UppercaseCodeValueObjectTests<Sku>
{
    protected override Sku Create(string value) => Sku.Create(value);
    protected override bool TryCreate(string? value, out Sku? result) => Sku.TryCreate(value, out result);
    protected override bool IsValid(string? value) => Sku.IsValid(value);
}
