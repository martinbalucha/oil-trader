namespace OilTrader.Contracts;

/// <summary>
/// An aggregation period. The span of market time that one candlestick bar covers.
/// It is not a calendar concept; it is a measurement of how ticks are grouped.
/// </summary>
public sealed class Timeframe
{
    /// <summary>
    /// One-minute bars.
    /// </summary>
    public static readonly Timeframe M1 = new("M1", TimeSpan.FromMinutes(1));

    /// <summary>
    /// One-hour bars.
    /// </summary>
    public static readonly Timeframe H1 = new("H1", TimeSpan.FromHours(1));

    /// <summary>
    /// The broker-standard code for this timeframe.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The length of one bar in this timeframe.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Truncates <paramref name="timestamp"/> to the start of the bar period
    /// it falls within for this timeframe.
    /// </summary>
    /// <param name="timestamp">Any tick or event timestamp.</param>
    /// <returns>
    /// The <see cref="DateTimeOffset"/> at which the enclosing bar opened.
    /// For example, 14:37:22 truncated to <see cref="M1"/> returns 14:37:00.
    /// </returns>
    public DateTimeOffset Truncate(DateTimeOffset timestamp)
    {
        return new DateTimeOffset(timestamp.Ticks - timestamp.Ticks % Duration.Ticks, timestamp.Offset);
    }

    /// <summary>
    /// Returns the time at which a bar that opened at <paramref name="openTime"/>
    /// closes under this timeframe.
    /// </summary>
    /// <param name="openTime">
    /// The <see cref="DateTimeOffset"/> returned by a previous call to
    /// <see cref="Truncate"/>.
    /// </param>
    /// <returns><paramref name="openTime"/> plus <see cref="Duration"/>.</returns>
    public DateTimeOffset CloseTime(DateTimeOffset openTime) => openTime + Duration;

    private Timeframe(string name, TimeSpan duration)
    {
        Name = name;
        Duration = duration;
    }

    public override string ToString() => Name;
}
