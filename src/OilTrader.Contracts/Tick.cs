namespace OilTrader.Contracts;

public sealed record Tick(
    string Symbol,
    decimal Bid,
    decimal Ask,
    DateTimeOffset Time,
    long Volume);
