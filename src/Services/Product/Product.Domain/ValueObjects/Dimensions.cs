namespace SmartEcommerce.Product.Domain.ValueObjects;

/// <summary>Physical shipping dimensions of a ProductVariation, in centimeters.</summary>
public sealed class Dimensions : ValueObject
{
    public decimal Length { get; }
    public decimal Width { get; }
    public decimal Height { get; }

    private Dimensions(decimal length, decimal width, decimal height)
    {
        Length = length;
        Width = width;
        Height = height;
    }

    public static bool IsValid(decimal length, decimal width, decimal height) =>
        GetValidationError(length, width, height) is null;

    public static bool TryCreate(decimal length, decimal width, decimal height, out Dimensions? dimensions)
    {
        if (GetValidationError(length, width, height) is not null)
        {
            dimensions = null;
            return false;
        }

        dimensions = new Dimensions(length, width, height);
        return true;
    }

    public static Dimensions Create(decimal length, decimal width, decimal height)
    {
        var error = GetValidationError(length, width, height);
        if (error is not null)
            throw error;

        return new Dimensions(length, width, height);
    }

    private static InvalidArgumentException? GetValidationError(decimal length, decimal width, decimal height)
    {
        if (length <= 0 || width <= 0 || height <= 0)
            return ExceptionFactory.InvalidRange("Dimensions must all be greater than zero.");

        return null;
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Length;
        yield return Width;
        yield return Height;
    }
}
