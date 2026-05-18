namespace OilTrader.Contracts.Ticking;

/// <summary>
/// Compresses an unbounded stream of ticks into a small set of OHLC bars. Instead of thousands of prices,
/// consumers get four: what the price was at the open of the period, the highest and lowest it reached,
/// and where it was when the period ended.
/// </summary>
public interface ITimeframeAggregator
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="tick"></param>
    /// <returns></returns>
    Task FeedAsync(Tick tick);
}
