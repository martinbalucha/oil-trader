namespace OilTrader.Contracts;

/// <summary>
/// The smallest atomic unit of market data; a single price update from the broker.
/// Every time the bid or ask price changes by even one pip, the broker emits a new tick.
/// </summary>
public sealed record Tick
{
    public Symbol Symbol { get; }
    public Price Bid { get; }
    public Price Ask { get; }
    public DateTimeOffset Time { get; }
    public long Volume { get; }
    public Price Spread => Price.From(Ask.Value - Bid.Value);

    private Tick(Symbol symbol, Price bid, Price ask, DateTimeOffset time, long volume)
    {
        Symbol = symbol;
        Bid = bid;
        Ask = ask;
        Time = time;
        Volume = volume;
    }

    /// <summary>
    /// Creates a new tick object
    /// </summary>
    /// <returns>A new tick object</returns>
    /// <exception cref="ArgumentException">If ask is smaller than bid, tick it not set or volume is negative</exception>
    public static Tick Create(Symbol symbol, Price bid, Price ask, DateTimeOffset time, long volume)
    {
        if (ask < bid)
        {
            throw new ArgumentException("Ask must be >= Bid.");
        }
        if (time == default)
        {
            throw new ArgumentException("Tick time must be set.");
        }
        if (volume < 0)
        {
            throw new ArgumentException("Volume cannot be negative.");
        }
        return new Tick(symbol, bid, ask, time, volume);
    }
}
