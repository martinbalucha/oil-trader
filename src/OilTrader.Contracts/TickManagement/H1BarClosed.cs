using OilTrader.Contracts.Messaging;

namespace OilTrader.Contracts.TickManagement;

/// <summary>
/// Carries information about M1 bar being closed
/// </summary>
/// <param name="Bar">Closed H1 Bar</param>
public record H1BarClosed(Bar Bar) : IMessage
{
    public Guid Id { get; } = Guid.NewGuid();
}
