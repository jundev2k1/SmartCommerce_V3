using BuildingBlock.Domain.Abstractions;
using BuildingBlock.Domain.Exceptions;
using BuildingBlock.SharedKernel.Extensions;
using BuildingBlock.SharedKernel.RegexPatterns;

namespace BuildingBlock.Domain.ValueObjects;

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
        => val.IsNotNullOrWhiteSpace()
            && val.Length <= 100
            && RegexPatterns.Email().IsMatch(val);
}
