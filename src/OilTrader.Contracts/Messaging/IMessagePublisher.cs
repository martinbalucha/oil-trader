namespace OilTrader.Contracts.Messaging;

public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message to the message bus-+
    /// </summary>
    /// <param name="message">Message to be published</param>
    /// <param name="ct">Optional: cancellation token</param>
    /// <typeparam name="T">Type of the message</typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">When message is null</exception>
    /// <exception cref="MessagePublishingException">When an error occurs during the publishing process</exception>
    Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class, IMessage;
}
