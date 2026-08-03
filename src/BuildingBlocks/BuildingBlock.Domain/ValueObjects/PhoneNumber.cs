using SmartEcommerce.BuildingBlock.Domain.Abstractions;
using SmartEcommerce.BuildingBlock.Domain.Exceptions;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;
using SmartEcommerce.BuildingBlock.SharedKernel.RegexPatterns;

namespace SmartEcommerce.BuildingBlock.Domain.ValueObjects;

public sealed class PhoneNumber : StringValueObject
{
    private PhoneNumber(string val) : base(val) { }

    public static PhoneNumber Create(string val)
    {
        if (!IsValid(val))
            throw ExceptionFactory.InvalidRange("Phone number is not valid.");

        return new PhoneNumber(val);
    }

    public static bool IsValid(string val)
        => val.IsNotNullOrWhiteSpace()
            && val.Length <= 30
            && RegexPatterns.PhoneNumber().IsMatch(val);
}
