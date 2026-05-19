namespace OilTrader.Contracts.TickManagement;

public interface ITickService
{
    /// <summary>
    /// Processes new incoming tick
    /// </summary>
    /// <param name="tick">Newly registered Tick</param>
    /// <param name="ct">Optional: cancellation token</param>
    /// <returns>Completed task</returns>
    Task ProcessAsync(Tick tick, CancellationToken ct = default);
}
