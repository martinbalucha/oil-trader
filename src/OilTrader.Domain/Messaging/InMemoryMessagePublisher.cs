using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using OilTrader.Contracts.Messaging;

namespace OilTrader.Domain.Messaging;

/// <summary>
/// In-memory version of the message publisher.
/// </summary>
public class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly ILogger<InMemoryMessagePublisher> _logger;
    private readonly Channel<IMessage> _channel = Channel.CreateUnbounded<IMessage>();

    public InMemoryMessagePublisher(ILogger<InMemoryMessagePublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, CancellationToken ct = default) where T : class, IMessage
    {
        await _channel.Writer.WriteAsync(message, ct);

        _logger.LogInformation("Successfully published {Type} message with id='{MessageId}'",
            message.GetType().Name, message.Id);
    }
}
