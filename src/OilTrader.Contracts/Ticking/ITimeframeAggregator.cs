namespace OilTrader.Contracts.Ticking;

/// <summary>
///
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
