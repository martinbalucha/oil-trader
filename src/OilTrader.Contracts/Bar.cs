namespace OilTrader.Contracts;

/// <summary>
///
/// </summary>
/// <param name="Symbol"></param>
/// <param name="Timeframe"></param>
/// <param name="OpenTime"></param>
/// <param name="Open"></param>
/// <param name="High"></param>
/// <param name="Low"></param>
/// <param name="Close"></param>
/// <param name="Volume"></param>
public sealed record Bar(
    string Symbol,
    Timeframe Timeframe,
    DateTimeOffset OpenTime,
    decimal Open, decimal High, decimal Low, decimal Close,
    long Volume);
