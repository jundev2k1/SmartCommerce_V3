using SmartEcommerce.BuildingBlock.Domain.Abstractions;
using SmartEcommerce.BuildingBlock.Domain.Exceptions;
using SmartEcommerce.BuildingBlock.SharedKernel.Constants;
using SmartEcommerce.BuildingBlock.SharedKernel.Extensions;

namespace SmartEcommerce.BuildingBlock.Domain.ValueObjects;

public sealed class LanguageCode : StringValueObject
{
    private LanguageCode(string val) : base(val) { }

    public static LanguageCode Create(string val)
    {
        if (!IsValid(val))
            throw ExceptionFactory.InvalidRange("Language code is not valid.");

        return new LanguageCode(val);
    }

    public static bool IsValid(string val)
        => val.IsNotNullOrWhiteSpace()
            && LanguageCodeConstant.SupportedLanguages.Contains(val);
}
