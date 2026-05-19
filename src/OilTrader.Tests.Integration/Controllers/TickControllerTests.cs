using System.Net;
using Microsoft.Extensions.DependencyInjection;
using OilTrader.Contracts;
using OilTrader.Tests.Integration.Infrastructure;
using WebTickRequest = OilTrader.Web.Models.TickRequest;

namespace OilTrader.Tests.Integration.Controllers;

[Collection(IntegrationTestCollection.HappyPath)]
public class TickControllerTests
{
    private readonly OilTraderWebApplicationFactory _factory;

    public TickControllerTests(OilTraderWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostTickAsync_ValidRequest_Returns204()
    {
        // Arrange
        var client = _factory.CreateClient();
        var symbol = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();

        // Act
        var response = await client.PostTickAsync(TickTestPayloads.ValidJson(symbol));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, response.Content.Headers.ContentLength ?? 0);
    }

    [Fact]
    public async Task PostTickAsync_ValidRequest_PersistsTickInRepository()
    {
        // Arrange
        var client = _factory.CreateClient();
        var symbol = $"TEST-{Guid.NewGuid():N}".ToUpperInvariant();
        const decimal bid = 100.5m;
        const decimal ask = 100.7m;
        var time = TickTestPayloads.DefaultTime;
        const long volume = 42;

        var request = TickTestPayloads.ValidJson(symbol, bid, ask, time, volume);

        // Act
        var response = await client.PostTickAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        await using var scope = _factory.Services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITickRepository>();
        var ticks = await repo.GetRecentAsync(20).ToListAsync();

        Assert.Contains(
            ticks,
            t => t.Symbol.Value == symbol
                 && t.Bid.Value == bid
                 && t.Ask.Value == ask
                 && t.Time == time
                 && t.Volume == volume);
    }

    [Fact]
    public async Task PostTickAsync_MissingSymbol_Returns400WithValidationProblem()
    {
        var client = _factory.CreateClient();

        var response = await client.PostTickAsync(TickTestPayloads.MissingSymbolJson());

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        AssertProblemJson(response);
        var problem = await response.ReadValidationProblemAsync();
        Assert.NotNull(problem);
        Assert.True(problem.HasValidationErrorFor(nameof(WebTickRequest.Symbol))
                    || problem.HasValidationErrorFor("symbol"));
    }

    [Fact]
    public async Task PostTickAsync_NullSymbol_Returns400WithValidationProblem()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostTickAsync(TickTestPayloads.NullSymbolJson());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertProblemJson(response);
        var problem = await response.ReadValidationProblemAsync();
        Assert.NotNull(problem);
        Assert.True(problem.HasValidationErrorFor(nameof(WebTickRequest.Symbol))
                    || problem.HasValidationErrorFor("symbol"));
    }

    [Fact]
    public async Task PostTickAsync_ZeroBid_Returns400WithValidationProblem()
    {
        var client = _factory.CreateClient();

        var response = await client.PostTickAsync(TickTestPayloads.ZeroBidJson());

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        AssertProblemJson(response);
        var problem = await response.ReadValidationProblemAsync();
        Assert.NotNull(problem);
        Assert.True(problem.HasValidationErrorFor(nameof(WebTickRequest.Bid))
                    || problem.HasValidationErrorFor("bid"));
    }

    [Fact]
    public async Task PostTickAsync_MalformedJson_Returns400()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostTickAsync(TickTestPayloads.MalformedJson());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static void AssertProblemJson(HttpResponseMessage response)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        Assert.Equal("application/problem+json", mediaType);
    }
}
