using Microsoft.Extensions.Logging;
using OilTrader.Contracts;
using OilTrader.Contracts.Messaging;
using OilTrader.Contracts.Ticking;

namespace OilTrader.Domain;

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
