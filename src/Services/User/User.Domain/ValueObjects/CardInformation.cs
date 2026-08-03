namespace SmartEcommerce.User.Domain.ValueObjects;

/// <summary>
/// Display-only card metadata for a UserPaymentMethod. Never holds a full PAN or CVV - only the
/// masked number and last 4 digits the issuer/gateway already considers safe to display, backed
/// by an out-of-band Token/ExternalPaymentMethodId for the actual charge.
/// </summary>
public sealed class CardInformation : ValueObject
{
    public string CardHolderName { get; }
    public string MaskedNumber { get; }
    public string Last4Digits { get; }
    public int ExpireMonth { get; }
    public int ExpireYear { get; }
    public CardBrand Brand { get; }

    private CardInformation(string cardHolderName, string maskedNumber, string last4Digits, int expireMonth, int expireYear, CardBrand brand)
    {
        CardHolderName = cardHolderName;
        MaskedNumber = maskedNumber;
        Last4Digits = last4Digits;
        ExpireMonth = expireMonth;
        ExpireYear = expireYear;
        Brand = brand;
    }

    public static CardInformation Create(
        string cardHolderName,
        string maskedNumber,
        string last4Digits,
        int expireMonth,
        int expireYear,
        CardBrand brand)
    {
        if (string.IsNullOrWhiteSpace(cardHolderName))
            throw ExceptionFactory.RequiredField("Card holder name cannot be empty.");

        ValidateMaskedNumber(maskedNumber);
        ValidateLast4Digits(last4Digits);

        if (expireMonth is < 1 or > 12)
            throw ExceptionFactory.InvalidRange("Card expiry month must be between 1 and 12.");

        if (expireYear < 1000 || expireYear > 9999)
            throw ExceptionFactory.InvalidRange("Card expiry year must be a 4-digit year.");

        return new CardInformation(cardHolderName.Trim(), maskedNumber.Trim(), last4Digits.Trim(), expireMonth, expireYear, brand);
    }

    /// <summary>Rejects anything that looks like a full, unmasked PAN - this Value Object must
    /// never carry real card data, only what the gateway already treats as safe to display.</summary>
    private static void ValidateMaskedNumber(string maskedNumber)
    {
        if (string.IsNullOrWhiteSpace(maskedNumber))
            throw ExceptionFactory.RequiredField("Masked card number cannot be empty.");

        var digitCount = maskedNumber.Count(char.IsDigit);
        if (digitCount > 4)
            throw ExceptionFactory.InvalidFormat("Masked card number must not expose more than the last 4 digits.");
    }

    private static void ValidateLast4Digits(string last4Digits)
    {
        if (last4Digits.Length != 4 || !last4Digits.All(char.IsDigit))
            throw ExceptionFactory.InvalidFormat("Card last-4-digits must be exactly 4 numeric characters.");
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return CardHolderName;
        yield return MaskedNumber;
        yield return Last4Digits;
        yield return ExpireMonth;
        yield return ExpireYear;
        yield return Brand;
    }

    public override string ToString() => $"{Brand} **** {Last4Digits} ({ExpireMonth:D2}/{ExpireYear})";
}
