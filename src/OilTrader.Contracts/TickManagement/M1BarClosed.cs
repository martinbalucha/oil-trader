using OilTrader.Contracts.Messaging;

namespace OilTrader.Contracts.TickManagement;

/// <summary>
/// Carries information about M1 bar being closed
/// </summary>
/// <param name="Bar">Closed M1 bar</param>
public sealed record M1BarClosed(Bar Bar) : IMessage
{
    public Guid Id { get; } = Guid.NewGuid();
}
