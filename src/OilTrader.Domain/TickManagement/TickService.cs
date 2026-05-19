using Microsoft.Extensions.Logging;
using OilTrader.Contracts;
using OilTrader.Contracts.TickManagement;

namespace OilTrader.Domain.TickManagement;

public class TickService : ITickService
{
    private readonly ITimeframeAggregator _timeframeAggregator;
    private readonly ITickRepository _repository;
    private readonly ILogger<TickService> _logger;

    public TickService(ITimeframeAggregator timeframeAggregator, ITickRepository repository, ILogger<TickService> logger)
    {
        _timeframeAggregator = timeframeAggregator;
        _repository = repository;
        _logger = logger;
    }

    public async Task ProcessAsync(Tick tick, CancellationToken ct = default)
    {
        await _repository.AddAsync(tick, ct);
        await _timeframeAggregator.FeedAsync(tick);

        _logger.LogDebug("Tick processed {Symbol}", tick.Symbol);
    }
}
