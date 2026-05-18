using Microsoft.Extensions.Logging;
using OilTrader.Contracts;
using OilTrader.Contracts.Messaging;
using OilTrader.Contracts.Ticking;

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

    public TimeframeAggregator(IMessagePublisher publisher, ILogger<TimeframeAggregator> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public Task FeedAsync(Tick tick)
    {
        throw new NotImplementedException();
    }
}
