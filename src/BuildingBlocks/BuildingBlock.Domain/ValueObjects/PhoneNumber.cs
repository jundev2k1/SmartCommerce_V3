using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Domain.Exceptions;
using BuildingBlock.SharedKernel.Extensions;
using BuildingBlock.SharedKernel.RegexPatterns;

namespace BuildingBlock.Domain.ValueObjects;

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
