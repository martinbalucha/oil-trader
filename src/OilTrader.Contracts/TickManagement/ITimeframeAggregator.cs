namespace OilTrader.Contracts.TickManagement;

/// <summary>
/// Compresses an unbounded stream of ticks into a small set of OHLC bars. Instead of thousands of prices,
/// consumers get four: what the price was at the open of the period, the highest and lowest it reached,
/// and where it was when the period ended.
/// </summary>
public interface ITimeframeAggregator
{
    /// <summary>
    /// Submits a tick to the aggregator for processing.
    /// </summary>
    /// <param name="tick">The market tick to process.</param>
    /// <returns>A task that completes once the tick has been processed</returns>
    /// <exception cref="ArgumentNullException">If tick is null</exception>
    Task FeedAsync(Tick tick);
}
