using OilTrader.Contracts.Messaging;

namespace OilTrader.Contracts.Ticking;

/// <summary>
/// Carries information about M1 bar being closed
/// </summary>
public record H1BarClosed : IMessage
{
    public Guid Id { get; } = Guid.NewGuid();
}
