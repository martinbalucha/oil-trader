using OilTrader.Contracts;
using OilTrader.Contracts.TickManagement;
using OilTrader.Domain;

namespace OilTrader.Tests.Unit;

public class OpenBarTests
{
    [Fact]
    public void Start_ValidTick_SeedsAllFourOhlcPricesToBid()
    {
        // Arrange
        var firstTime = new DateTimeOffset(2025, 1, 1, 10, 0, 15, TimeSpan.Zero);
        var boundaryTime = new DateTimeOffset(2025, 1, 1, 10, 1, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(101.5m), Price.From(102m), firstTime, 1);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        // Act
        var closed = bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(200m), Price.From(201m), boundaryTime, 1),
            Timeframe.M1);

        // Assert
        Assert.NotNull(closed);
        Assert.Equal(first.Bid, closed.Open);
        Assert.Equal(first.Bid, closed.High);
        Assert.Equal(first.Bid, closed.Low);
        Assert.Equal(first.Bid, closed.Close);
    }

    [Fact]
    public void Start_ValidTick_SetsSymbolTimeframeAndOpenTime()
    {
        // Arrange
        var tickTime = new DateTimeOffset(2025, 1, 1, 10, 0, 27, TimeSpan.Zero);
        var tick = Tick.Create(Symbol.From("XBIUSD"), Price.From(150m), Price.From(151m), tickTime, 1);
        var expectedOpen = Timeframe.M1.Truncate(tick.Time);
        var nextPeriodTick = Tick.Create(Symbol.From("XBIUSD"), Price.From(151m), Price.From(152m),
            expectedOpen.AddMinutes(1), 1);

        // Act
        var openBar = OpenBar.Start(tick, Timeframe.M1, expectedOpen);
        var closed = openBar.Accept(nextPeriodTick, Timeframe.M1);

        // Assert
        Assert.Equal(expectedOpen, openBar.OpenTime);
        Assert.NotNull(closed);
        Assert.Equal(tick.Symbol, closed.Symbol);
        Assert.Same(Timeframe.M1, closed.Timeframe);
        Assert.Equal(expectedOpen, closed.OpenTime);
    }

    [Fact]
    public void Start_ValidTick_SetsInitialVolumeFromTick()
    {
        // Arrange
        var firstTime = new DateTimeOffset(2025, 1, 1, 14, 3, 0, TimeSpan.Zero);
        var boundaryTime = firstTime.AddMinutes(1);
        var volume = 4_827L;
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(88m), Price.From(88.01m), firstTime, volume);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        // Act
        var closed = bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(89m), Price.From(90m), boundaryTime, 999),
            Timeframe.M1);

        // Assert
        Assert.NotNull(closed);
        Assert.Equal(volume, closed.Volume);
    }

    [Fact]
    public void Accept_TickInSamePeriod_ReturnsNull()
    {
        // Arrange
        var minuteOpen = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(100m), Price.From(101m),
            minuteOpen.AddSeconds(5), 100);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        var samePeriod = Tick.Create(Symbol.From("XBIUSD"), Price.From(104m), Price.From(104.5m),
            minuteOpen.AddSeconds(58), 1);

        // Act
        var result = bar.Accept(samePeriod, Timeframe.M1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Accept_TickInSamePeriod_UpdatesHighWhenBidExceedsCurrentHigh()
    {
        // Arrange
        var minuteOpen = new DateTimeOffset(2025, 6, 1, 15, 30, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(100m), Price.From(101m),
            minuteOpen.AddSeconds(12), 1);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(117.25m), Price.From(118m),
                minuteOpen.AddSeconds(40), 1),
            Timeframe.M1));

        var boundaryTick = Tick.Create(Symbol.From("XBIUSD"), Price.From(1m), Price.From(2m),
            minuteOpen.AddMinutes(1), 1);

        // Act
        var closed = bar.Accept(boundaryTick, Timeframe.M1);

        // Assert
        Assert.NotNull(closed);
        Assert.Equal(first.Bid, closed.Open);
        Assert.Equal(117.25m, closed.High.Value);
        Assert.Equal(first.Bid, closed.Low);
        Assert.Equal(117.25m, closed.Close.Value);
    }

    [Fact]
    public void Accept_TickInSamePeriod_UpdatesLowWhenBidBelowCurrentLow()
    {
        // Arrange
        var minuteOpen = new DateTimeOffset(2025, 1, 20, 9, 0, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(200m), Price.From(201m),
            minuteOpen.AddSeconds(22), 1);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(77.75m), Price.From(79m),
                minuteOpen.AddSeconds(51), 1),
            Timeframe.M1));

        var boundaryTick = Tick.Create(Symbol.From("XBIUSD"), Price.From(220m), Price.From(221m),
            minuteOpen.AddMinutes(1), 1);

        // Act
        var closed = bar.Accept(boundaryTick, Timeframe.M1);

        // Assert
        Assert.NotNull(closed);
        Assert.Equal(first.Bid, closed.Open);
        Assert.Equal(first.Bid, closed.High);
        Assert.Equal(77.75m, closed.Low.Value);
        Assert.Equal(77.75m, closed.Close.Value);
    }

    [Fact]
    public void Accept_TickInSamePeriod_UpdatesClose()
    {
        // Arrange
        var minuteOpen = new DateTimeOffset(2025, 3, 3, 12, 0, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(50m), Price.From(50.1m),
            minuteOpen.AddSeconds(1), 1);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(55m), Price.From(55.5m),
                minuteOpen.AddSeconds(10), 1),
            Timeframe.M1));
        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(52.5m), Price.From(53m),
                minuteOpen.AddSeconds(45), 1),
            Timeframe.M1));

        var boundaryTick = Tick.Create(Symbol.From("XBIUSD"), Price.From(99m), Price.From(100m),
            minuteOpen.AddMinutes(1), 1);

        // Act
        var closed = bar.Accept(boundaryTick, Timeframe.M1);

        // Assert
        Assert.NotNull(closed);
        Assert.Equal(50m, closed.Open.Value);
        Assert.Equal(55m, closed.High.Value);
        Assert.Equal(50m, closed.Low.Value);
        Assert.Equal(52.5m, closed.Close.Value);
    }

    [Fact]
    public void Accept_TickInSamePeriod_AccumulatesVolume()
    {
        // Arrange
        var minuteOpen = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(10m), Price.From(11m),
            minuteOpen.AddSeconds(3), 100);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(10m), Price.From(11m),
                minuteOpen.AddSeconds(20), 50),
            Timeframe.M1));
        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(10m), Price.From(11m),
                minuteOpen.AddSeconds(59), 25),
            Timeframe.M1));

        var boundaryTick = Tick.Create(Symbol.From("XBIUSD"), Price.From(10m), Price.From(11m),
            minuteOpen.AddMinutes(1), 1);

        // Act
        var closed = bar.Accept(boundaryTick, Timeframe.M1);

        // Assert
        Assert.NotNull(closed);
        Assert.Equal(175L, closed.Volume);
    }

    [Fact]
    public void Accept_TickExactlyAtOpenTime_TreatedAsInPeriodAndReturnsNull()
    {
        // Arrange
        var openTime = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(100m), Price.From(101m),
            openTime.AddSeconds(30), 1);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        var atOpen = Tick.Create(Symbol.From("XBIUSD"), Price.From(90m), Price.From(91m), openTime, 1);

        // Act
        var result = bar.Accept(atOpen, Timeframe.M1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Accept_TickAtPeriodBoundary_ReturnsClosedBarWithCorrectOhlcv()
    {
        // Arrange
        var minuteOpen = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(100m), Price.From(101m),
            minuteOpen.AddSeconds(5), 10);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(110m), Price.From(111m),
                minuteOpen.AddSeconds(10), 20),
            Timeframe.M1));
        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(105m), Price.From(106m),
                minuteOpen.AddSeconds(55), 30),
            Timeframe.M1));

        var nextPeriodFirst = Tick.Create(Symbol.From("XBIUSD"), Price.From(500m), Price.From(501m),
            minuteOpen.AddMinutes(1).AddSeconds(3), 1000);

        // Act
        var closed = bar.Accept(nextPeriodFirst, Timeframe.M1);

        // Assert
        Assert.NotNull(closed);
        Assert.Equal(100m, closed.Open.Value);
        Assert.Equal(110m, closed.High.Value);
        Assert.Equal(100m, closed.Low.Value);
        Assert.Equal(105m, closed.Close.Value);
        Assert.Equal(60L, closed.Volume);
    }

    [Fact]
    public void Accept_LateTickBeforeOpenTime_ReturnsNullAndDoesNotAlterAccumulatedState()
    {
        // Arrange
        var minuteOpen = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(100m), Price.From(101m),
            minuteOpen.AddSeconds(10), 10);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(120m), Price.From(121m),
                minuteOpen.AddSeconds(20), 20),
            Timeframe.M1));

        var lateTick = Tick.Create(Symbol.From("XBIUSD"), Price.From(9_999m), Price.From(10_000m),
            minuteOpen.AddSeconds(-1), 1_000_000);

        // Act
        var lateResult = bar.Accept(lateTick, Timeframe.M1);

        var boundaryTick = Tick.Create(Symbol.From("XBIUSD"), Price.From(500m), Price.From(501m),
            minuteOpen.AddMinutes(1), 1);
        var closed = bar.Accept(boundaryTick, Timeframe.M1);

        // Assert
        Assert.Null(lateResult);
        Assert.NotNull(closed);
        Assert.Equal(100m, closed.Open.Value);
        Assert.Equal(120m, closed.High.Value);
        Assert.Equal(100m, closed.Low.Value);
        Assert.Equal(120m, closed.Close.Value);
        Assert.Equal(30L, closed.Volume);
    }

    [Fact]
    public void Accept_MultipleSamePeriodTicks_HighAndLowReflectExtremesNotJustFirstAndLast()
    {
        // Arrange
        var minuteOpen = new DateTimeOffset(2025, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var first = Tick.Create(Symbol.From("XBIUSD"), Price.From(100m), Price.From(101m),
            minuteOpen.AddSeconds(5), 1);
        var openTime = Timeframe.M1.Truncate(first.Time);
        var bar = OpenBar.Start(first, Timeframe.M1, openTime);

        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(120m), Price.From(121m),
                minuteOpen.AddSeconds(25), 1),
            Timeframe.M1));
        Assert.Null(bar.Accept(
            Tick.Create(Symbol.From("XBIUSD"), Price.From(90m), Price.From(91m),
                minuteOpen.AddSeconds(58), 1),
            Timeframe.M1));

        var boundaryTick = Tick.Create(Symbol.From("XBIUSD"), Price.From(1m), Price.From(2m),
            minuteOpen.AddMinutes(1), 1);

        // Act
        var closed = bar.Accept(boundaryTick, Timeframe.M1);

        // Assert
        Assert.NotNull(closed);
        Assert.Equal(100m, closed.Open.Value);
        Assert.Equal(120m, closed.High.Value);
        Assert.Equal(90m, closed.Low.Value);
        Assert.Equal(90m, closed.Close.Value);
    }
}
