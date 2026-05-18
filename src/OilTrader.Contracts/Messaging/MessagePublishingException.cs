namespace OilTrader.Contracts.Messaging;

/// <summary>
/// Represents an error during the message publishing process
/// </summary>
public class MessagePublishingException : Exception
{
    public MessagePublishingException(string message) : base(message) {}
    public MessagePublishingException(string message, Exception innerException) : base(message, innerException){}
}
