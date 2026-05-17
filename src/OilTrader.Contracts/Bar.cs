namespace OilTrader.Contracts;

/// <summary>
/// Summary of all price activity within a fixed time window.
/// Instead of thousands of individual ticks, you get four prices that describe what happened during that period
/// </summary>
public sealed record Bar
{
    /// <summary>
    /// The instrument this bar belongs to.
    /// </summary>
    public Symbol Symbol { get; }

    /// <summary>
    /// The aggregation period that defines this bar's duration and boundary logic.
    /// </summary>
    public Timeframe Timeframe { get; }

    /// <summary>
    /// The timestamp at which this bar period opened
    /// </summary>
    public DateTimeOffset OpenTime { get; }

    /// <summary>
    /// The timestamp at which this bar period closed.
    /// Computed as <c><see cref="OpenTime"/> + <see cref="Timeframe.Duration"/></c>;
    /// no tick at exactly this time is guaranteed to exist.
    /// </summary>
    public DateTimeOffset CloseTime => Timeframe.CloseTime(OpenTime);

    /// <summary>
    /// The bid price of the first tick in the bar period.
    /// </summary>
    public Price Open { get; }

    /// <summary>
    /// The highest bid price observed during the bar period.
    /// </summary>
    public Price High { get; }

    /// <summary>
    /// The lowest bid price observed during the bar period.
    /// </summary>
    public Price Low { get; }

    /// <summary>
    /// The bid price of the last tick before the bar period ended.
    /// </summary>
    public Price Close { get; }

    /// <summary>
    /// The number of price updates (ticks) received during the bar period.
    /// </summary>
    /// <remarks>
    /// This is MT5 tick volume — a count of price changes — not exchange-traded
    /// lot volume. It is a relative liquidity indicator only; do not compare it
    /// across instruments or brokers.
    /// </remarks>
    public long Volume { get; }

    private Bar(Symbol symbol, Timeframe tf, DateTimeOffset openTime, Price open, Price high, Price low,
        Price close, long volume)
    {
        Symbol = symbol;
        Timeframe = tf;
        OpenTime = openTime;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }

    /// <summary>
    /// Creates a new bar object
    /// </summary>
    /// <returns>A new bar object</returns>
    /// <exception cref="ArgumentException">If high is not the highest, low is not the lowest of volume is negative</exception>
    public static Bar Create(Symbol symbol, Timeframe tf, DateTimeOffset openTime, Price open, Price high, Price low,
        Price close, long volume)
    {
        if (high < open || high < close || high < low)
        {
            throw new ArgumentException("High must be the highest price.");
        }

        if (low > open || low > close || low > high)
        {
            throw new ArgumentException("Low must be the lowest price.");
        }

        if (volume < 0)
        {
            throw new ArgumentException("Volume cannot be negative.");
        }

        return new Bar(symbol, tf, openTime, open, high, low, close, volume);
    }
}
