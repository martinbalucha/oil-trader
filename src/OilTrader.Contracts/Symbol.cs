namespace OilTrader.Contracts;

/// <summary>
/// Normalized identifier of a tradeable instrument as recognized by the broker
/// </summary>
public sealed record Symbol
{
    /// <summary>
    /// Always uppercase, non-empty 1–32 characters, no whitespace.
    /// </summary>
    public string Value { get; }

    private Symbol(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates new symbol object
    /// </summary>
    /// <param name="value">Value</param>
    /// <returns>Symbol object</returns>
    /// <exception cref="ArgumentException">If the value is null or whitespace</exception>
    public static Symbol From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Symbol cannot be null or whitespace.", nameof(value));
        }

        return new Symbol(value.ToUpperInvariant());
    }

    public override string ToString() => Value;
}
