using System.Collections.Concurrent;
using OilTrader.Contracts;

namespace OilTrader.Domain;

public class QueueTickRepository : ITickRepository
{
    internal const int MaxSize = 1000;

    private readonly object _lock = new();
    private readonly ConcurrentQueue<Tick> _queue = new();

    public Task AddAsync(Tick tick, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _queue.Enqueue(tick);
            while (_queue.Count > MaxSize)
            {
                _queue.TryDequeue(out _);
            }
        }

        return Task.CompletedTask;
    }

    public IAsyncEnumerable<Tick> GetRecentAsync(int count, CancellationToken ct = default)
    {
        count = count < 0 ? 0 : count;

        var snapshot = _queue.ToArray();

        var result = snapshot.Length <= count
            ? snapshot
            : snapshot[^count..];

        return result.ToAsyncEnumerable();
    }
}
