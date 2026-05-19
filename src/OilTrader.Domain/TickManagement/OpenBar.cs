using OilTrader.Contracts;

namespace OilTrader.Domain.TickManagement;

/// <summary>
/// Mutable accumulator for the in-progress bar.
/// </summary>
public sealed class OpenBar
{
    private readonly Symbol _symbol;
    private readonly Timeframe _timeframe;

    public DateTimeOffset OpenTime { get; }

    private readonly Price _open;
    private Price _high;
    private Price _low;
    private Price _close;
    private long _volume;

    private OpenBar(Symbol symbol, Timeframe timeframe, DateTimeOffset openTime,
        Price open, Price high, Price low, Price close, long volume)
    {
        _symbol = symbol;
        _timeframe = timeframe;
        OpenTime = openTime;
        _open = open;
        _high = high;
        _low = low;
        _close = close;
        _volume = volume;
    }

    /// <summary>
    /// First tick of a new period seeds all four prices with bid.
    /// </summary>
    /// <param name="tick">Market tick</param>
    /// <param name="timeframe">Timeframe</param>
    /// <param name="openTime">Opening time</param>
    /// <returns>Initialized open bar</returns>
    public static OpenBar Start(Tick tick, Timeframe timeframe, DateTimeOffset openTime)
        => new(tick.Symbol, timeframe, openTime, tick.Bid, tick.Bid, tick.Bid, tick.Bid, tick.Volume);

    /// <summary>
    /// Processes a tick against this bar's period.
    /// </summary>
    /// <returns>Returns the completed <see cref="Bar"/> if the tick belongs to a new period.
    /// Otherwise, the tick gets updated and null is returned.</returns>
    public Bar? Accept(Tick tick, Timeframe timeframe)
    {
        if (tick.Time < OpenTime)
        {
            return null;
        }

        // By truncating, we are specifically checking against the period of the bar,
        // so no need to adjust for the close time.
        if (timeframe.Truncate(tick.Time) > OpenTime)
        {
            return Build();
        }

        Update(tick);
        return null;
    }

    private void Update(Tick tick)
    {
        if (tick.Bid > _high)
        {
            _high = tick.Bid;
        }

        if (tick.Bid < _low)
        {
            _low = tick.Bid;
        }

        _close = tick.Bid;
        _volume += tick.Volume;
    }

    private Bar Build() => Bar.Create(_symbol, _timeframe, OpenTime, _open, _high, _low, _close, _volume);
}
