namespace Order.Domain.Enums;

public enum TaxMethod : byte
{
    /// <summary>Tax is calculated using a fixed monetary amount.</summary>
    FixedAmount = 1,

    /// <summary>Tax is calculated using a percentage.</summary>
    Percentage = 2
}