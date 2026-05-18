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
}
