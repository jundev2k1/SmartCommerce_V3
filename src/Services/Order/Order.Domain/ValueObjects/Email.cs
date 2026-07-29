using BuildingBlock.SharedKernel.Extensions;
using BuildingBlock.SharedKernel.RegexPatterns;

namespace Order.Domain.ValueObjects;

public sealed class Email : StringValueObject
{
    private Email(string val) : base(val) { }

    public static Email Create(string val)
    {
        if (!IsValid(val))
            throw ExceptionFactory.InvalidRange("Email is not valid.");

        return new Email(val);
    }

    public static bool IsValid(string val)
        => val.IsNotNullOrWhiteSpace() && RegexPatterns.Email().IsMatch(val);
}
