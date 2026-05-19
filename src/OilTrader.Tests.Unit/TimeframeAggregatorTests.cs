using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OilTrader.Contracts;
using OilTrader.Contracts.Messaging;
using OilTrader.Contracts.TickManagement;
using OilTrader.Domain;
using OilTrader.Domain.TickManagement;

namespace OilTrader.Tests.Unit;

public class TimeframeAggregatorTests
{
    private readonly Mock<IMessagePublisher> _publisherMock = new();

    private TimeframeAggregator CreateSut() =>
        new(_publisherMock.Object, NullLogger<TimeframeAggregator>.Instance);

    private static Tick TickUtc(Symbol symbol, int year, int month, int day, int hour, int minute, int second,
        decimal bid, decimal askSpreadDelta, long volume)
    {
        var time = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero);
        var bidPrice = Price.From(bid);
        var askPrice = Price.From(bid + askSpreadDelta);
        return Tick.Create(symbol, bidPrice, askPrice, time, volume);
    }

    [Fact]
    public async Task FeedAsync_NullTick_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.FeedAsync(null!));
    }

    [Fact]
    public async Task FeedAsync_FirstTickEver_DoesNotPublishAnyMessage()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var tick = TickUtc(symbol, 2026, 5, 18, 14, 37, 22, 100m, 1m, 1);

        // Act
        await sut.FeedAsync(tick);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FeedAsync_MultipleTicksWithinSameMinute_DoesNotPublishM1Bar()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var first = TickUtc(symbol, 2026, 5, 18, 10, 0, 10, 100m, 1m, 1);
        var second = TickUtc(symbol, 2026, 5, 18, 10, 0, 50, 101m, 1m, 2);

        // Act
        await sut.FeedAsync(first);
        await sut.FeedAsync(second);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FeedAsync_MultipleTicksWithinSameHour_DoesNotPublishH1Bar()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var first = TickUtc(symbol, 2026, 5, 18, 10, 0, 30, 100m, 1m, 1);
        var second = TickUtc(symbol, 2026, 5, 18, 10, 1, 30, 105m, 1m, 1);

        // Act
        await sut.FeedAsync(first);
        await sut.FeedAsync(second);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FeedAsync_FirstTickOfNewMinute_PublishesExactlyOneM1BarClosed()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var firstMinute = TickUtc(symbol, 2026, 5, 18, 11, 5, 0, 100m, 1m, 1);
        var secondMinute = TickUtc(symbol, 2026, 5, 18, 11, 6, 0, 110m, 1m, 1);

        // Act
        await sut.FeedAsync(firstMinute);
        await sut.FeedAsync(secondMinute);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FeedAsync_FirstTickOfNewMinute_PublishedM1BarHasCorrectOhlcv()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var t1 = TickUtc(symbol, 2026, 5, 18, 12, 0, 0, 100m, 1m, 10);
        var t2 = TickUtc(symbol, 2026, 5, 18, 12, 0, 30, 105m, 1m, 20);
        var t3 = TickUtc(symbol, 2026, 5, 18, 12, 1, 0, 110m, 1m, 5);

        M1BarClosed? captured = null;
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()))
            .Callback<IMessage, CancellationToken>((m, _) => captured = (M1BarClosed)m)
            .Returns(Task.CompletedTask);

        // Act
        await sut.FeedAsync(t1);
        await sut.FeedAsync(t2);
        await sut.FeedAsync(t3);

        // Assert
        Assert.NotNull(captured);
        var bar = captured!.Bar;
        Assert.Equal(symbol, bar.Symbol);
        Assert.Equal(Timeframe.M1, bar.Timeframe);
        Assert.Equal(Price.From(100m), bar.Open);
        Assert.Equal(Price.From(105m), bar.High);
        Assert.Equal(Price.From(100m), bar.Low);
        Assert.Equal(Price.From(105m), bar.Close);
        Assert.Equal(30L, bar.Volume);
    }

    [Fact]
    public async Task FeedAsync_FirstTickOfNewMinute_PublishedM1BarOpenTimeIsStartOfCompletedMinute()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var expectedOpen = new DateTimeOffset(2026, 5, 18, 12, 0, 0, TimeSpan.Zero);
        var t1 = TickUtc(symbol, 2026, 5, 18, 12, 0, 15, 100m, 1m, 1);
        var t2 = TickUtc(symbol, 2026, 5, 18, 12, 1, 0, 101m, 1m, 1);

        M1BarClosed? captured = null;
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()))
            .Callback<IMessage, CancellationToken>((m, _) => captured = (M1BarClosed)m)
            .Returns(Task.CompletedTask);

        // Act
        await sut.FeedAsync(t1);
        await sut.FeedAsync(t2);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(expectedOpen, captured!.Bar.OpenTime);
    }

    [Fact]
    public async Task FeedAsync_FirstTickOfNewHour_PublishesExactlyOneH1BarClosed()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var inHour = TickUtc(symbol, 2026, 5, 18, 13, 59, 0, 100m, 1m, 1);
        var nextHour = TickUtc(symbol, 2026, 5, 18, 14, 0, 0, 101m, 1m, 1);

        // Act
        await sut.FeedAsync(inHour);
        await sut.FeedAsync(nextHour);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FeedAsync_FirstTickOfNewHour_PublishesBothM1AndH1BarClosed()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var inHour = TickUtc(symbol, 2026, 5, 18, 15, 59, 30, 100m, 1m, 1);
        var nextHour = TickUtc(symbol, 2026, 5, 18, 16, 0, 0, 101m, 1m, 1);

        // Act
        await sut.FeedAsync(inHour);
        await sut.FeedAsync(nextHour);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task FeedAsync_FirstTickOfNewHour_H1BarSpansAllIntermediateM1Ticks()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var ticks = new[]
        {
            TickUtc(symbol, 2026, 5, 18, 14, 0, 5, 100m, 1m, 1),
            TickUtc(symbol, 2026, 5, 18, 14, 5, 0, 120m, 1m, 2),
            TickUtc(symbol, 2026, 5, 18, 14, 30, 0, 90m, 1m, 3),
            TickUtc(symbol, 2026, 5, 18, 14, 45, 0, 115m, 1m, 4),
        };
        var nextHour = TickUtc(symbol, 2026, 5, 18, 15, 0, 0, 99m, 1m, 5);

        H1BarClosed? h1Captured = null;
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()))
            .Callback<IMessage, CancellationToken>((m, _) => h1Captured = (H1BarClosed)m)
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        foreach (var t in ticks)
        {
            await sut.FeedAsync(t);
        }

        await sut.FeedAsync(nextHour);

        // Assert
        Assert.NotNull(h1Captured);
        var bar = h1Captured!.Bar;
        Assert.Equal(Timeframe.H1, bar.Timeframe);
        Assert.Equal(new DateTimeOffset(2026, 5, 18, 14, 0, 0, TimeSpan.Zero), bar.OpenTime);
        Assert.Equal(Price.From(100m), bar.Open);
        Assert.Equal(Price.From(120m), bar.High);
        Assert.Equal(Price.From(90m), bar.Low);
        Assert.Equal(Price.From(115m), bar.Close);
        Assert.Equal(10L, bar.Volume);
    }

    [Fact]
    public async Task FeedAsync_LateTickEarlierThanCurrentBarOpenTime_IsDiscardedSilently()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var valid = TickUtc(symbol, 2026, 5, 18, 12, 1, 0, 100m, 1m, 10);
        var late = TickUtc(symbol, 2026, 5, 18, 12, 0, 30, 999m, 1m, 100);
        var samePeriod = TickUtc(symbol, 2026, 5, 18, 12, 1, 30, 102m, 1m, 5);

        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await sut.FeedAsync(valid);
        await sut.FeedAsync(late);
        await sut.FeedAsync(samePeriod);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task FeedAsync_MultipleConsecutiveMinuteCrossings_PublishesM1ForEachCrossing()
    {
        // Arrange
        var sut = CreateSut();
        var symbol = Symbol.From("XBIUSD");
        var t0 = TickUtc(symbol, 2026, 5, 18, 10, 0, 0, 100m, 1m, 1);
        var t1 = TickUtc(symbol, 2026, 5, 18, 10, 1, 0, 101m, 1m, 1);
        var t2 = TickUtc(symbol, 2026, 5, 18, 10, 2, 0, 102m, 1m, 1);

        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await sut.FeedAsync(t0);
        await sut.FeedAsync(t1);
        await sut.FeedAsync(t2);

        // Assert
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<M1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<H1BarClosed>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
