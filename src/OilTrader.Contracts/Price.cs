namespace OilTrader.Contracts;

/// <summary>
/// Represents price of the tradeable instrument
/// </summary>
public sealed record Price
{
    /// <summary>
    /// Price value
    /// </summary>
    public decimal Value { get; }

    private Price(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new price object
    /// </summary>
    /// <param name="value">Value of the price</param>
    /// <returns>Price object</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the price is not positive</exception>
    public static Price From(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Price must be positive.");
        }

        return new Price(value);
    }

    public static bool operator >=(Price a, Price b) => a.Value >= b.Value;
    public static bool operator <=(Price a, Price b) => a.Value <= b.Value;
}
