using Product.Domain.ValueObjects;

namespace Product.Domain.Tests.ValueObjects;

public class TagCodeTests : UppercaseCodeValueObjectTests<TagCode>
{
    protected override TagCode Create(string value) => TagCode.Create(value);
    protected override bool TryCreate(string? value, out TagCode? result) => TagCode.TryCreate(value, out result);
    protected override bool IsValid(string? value) => TagCode.IsValid(value);
}
