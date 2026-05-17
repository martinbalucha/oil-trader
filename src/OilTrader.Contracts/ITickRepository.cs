namespace OilTrader.Contracts;

public interface ITickRepository
{
    /// <summary>
    /// Adds a new tick record
    /// </summary>
    /// <param name="tick">Tick to be recorded</param>
    /// <param name="ct">Optional: Cancellation token</param>
    Task AddAsync(Tick tick, CancellationToken ct = default);

    /// <summary>
    ///
    /// </summary>
    /// <param name="count"></param>
    /// <param name="ct">Optional: Cancellation token</param>
    /// <returns>Async collection of the recent ticks</returns>
    IAsyncEnumerable<Tick> GetRecentAsync(int count, CancellationToken ct = default);
}
