using System.Text;
using System.Text.Json;

namespace OilTrader.Tests.Integration.Infrastructure;

public static class TickTestPayloads
{
    public static readonly DateTimeOffset DefaultTime = new(2026, 5, 19, 12, 0, 0, TimeSpan.Zero);

    public static string ValidJson(
        string symbol = "XBIUSD",
        decimal bid = 100.5m,
        decimal ask = 100.7m,
        DateTimeOffset? time = null,
        long volume = 42) =>
        JsonSerializer.Serialize(new
        {
            symbol,
            bid,
            ask,
            time = (time ?? DefaultTime).ToString("O"),
            volume,
        });

    public static StringContent ValidContent(string symbol = "XBIUSD") =>
        JsonContent(ValidJson(symbol));

    public static StringContent JsonContent(string json) =>
        new(json, Encoding.UTF8, "application/json");

    public static string MissingSymbolJson() =>
        """
        {
          "bid": 100.5,
          "ask": 100.7,
          "time": "2026-05-19T12:00:00+00:00",
          "volume": 42
        }
        """;

    public static string NullSymbolJson() =>
        """
        {
          "symbol": null,
          "bid": 100.5,
          "ask": 100.7,
          "time": "2026-05-19T12:00:00+00:00",
          "volume": 42
        }
        """;

    public static string ZeroBidJson() => ValidJson(bid: 0);

    public static string MalformedJson() => "{ not valid json";
}
