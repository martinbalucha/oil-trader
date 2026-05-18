namespace OilTrader.Contracts.Messaging;

/// <summary>
/// Represents an an application event
/// </summary>
public interface IMessage
{
    /// <summary>
    /// ID of the message
    /// </summary>
    Guid Id { get; }
}
