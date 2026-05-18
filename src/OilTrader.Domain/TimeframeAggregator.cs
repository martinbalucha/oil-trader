using Microsoft.Extensions.Logging;
using OilTrader.Contracts;
using OilTrader.Contracts.Messaging;
using OilTrader.Contracts.TickManagement;

namespace OilTrader.Domain;

/// <summary>
/// The aggregator maintains two "open bars" simultaneously: one M1 and one H1.
/// Every incoming tick is measured against those boundaries. When a tick crosses into a new minute,
/// the old M1 bar is completed and broadcast; when it crosses into a new hour, the H1 bar closes too.
/// </summary>
public class TimeframeAggregator : ITimeframeAggregator
{
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<TimeframeAggregator> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private OpenBar? _currentM1;
    private OpenBar? _currentH1;

    public TimeframeAggregator(IMessagePublisher publisher, ILogger<TimeframeAggregator> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task FeedAsync(Tick tick)
    {
        ArgumentNullException.ThrowIfNull(tick);

        await _semaphore.WaitAsync();

        try
        {
            Bar? m1Closed = _currentM1?.Accept(tick, Timeframe.M1);

            if (m1Closed is not null || _currentM1 is null)
            {
                _currentM1 = OpenBar.Start(tick, Timeframe.M1, Timeframe.M1.Truncate(tick.Time));
            }

            Bar? h1Closed = _currentH1?.Accept(tick, Timeframe.H1);

            if (h1Closed is not null || _currentH1 is null)
            {
                _currentH1 = OpenBar.Start(tick, Timeframe.H1, Timeframe.H1.Truncate(tick.Time));
            }

            if (m1Closed is not null)
            {
                _logger.LogDebug("M1 bar closed for {Symbol} opened at {OpenTime}", m1Closed.Symbol, m1Closed.OpenTime);
                await _publisher.PublishAsync(new M1BarClosed(m1Closed));
            }

            if (h1Closed is not null)
            {
                _logger.LogDebug("H1 bar closed for {Symbol} opened at {OpenTime}", h1Closed.Symbol, h1Closed.OpenTime);
                await _publisher.PublishAsync(new H1BarClosed(h1Closed));
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Mutable accumulator for the in-progress bar.
    /// </summary>
    private sealed class OpenBar
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
}
